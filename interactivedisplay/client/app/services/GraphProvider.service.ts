import { Injectable } from '@angular/core';
import { Http } from '@angular/http';
import { Observable } from 'rxjs/Observable';
import { ReplaySubject } from 'rxjs/ReplaySubject';
import { SocketIO } from './SocketIO.service';
import { Graph } from '../models/index';
import * as _ from 'lodash';

const COLOURS = [
    // material colour palette, see https://material.io/guidelines/style/color.html
    '#F44336', // red
    '#9C27B0', // purple
    '#3F51B5', // indigo
    '#2196F3', // blue
    '#4CAF50', // green
    '#FFEB3B', // yellow
    '#FF9800', // orange
    '#9E9E9E', // grey
];

@Injectable()
export class GraphProvider {
    private graphs: Graph[] = [];
    private graphObserver: ReplaySubject<Graph[]> = new ReplaySubject<Graph[]>(1);
    private graphSelectionChanged: ReplaySubject<boolean> = new ReplaySubject<boolean>(1);
    private idCounter: number = 0;

    private delayedGraphDataUpdate: Function;
    private delayedGraphPositionUpdate: Function;

    constructor(private socketio: SocketIO, private http: Http) {
        this.http.get('/api/graph/list')
            .subscribe(response => {
                // response gives graphs as interface, *not* as class
                this.graphs = _.map(<any[]>response.json().graphs, g => Graph.fromJson(g));
                if (this.graphs.length > 0) {
                    this.idCounter = _.max(<number[]>_.map(this.graphs, 'id')) + 1;
                }

                this.graphSelectionChanged.next(false);
                for (let graph of this.graphs) {
                    this.attachListeners(graph);
                    if (graph.isSelected) {
                        this.graphSelectionChanged.next(true);
                    }
                }

                this.recalculateGraphIndices();
                this.graphObserver.next(this.graphs);

                // for live editing via console
                window['graphs'] = this.graphs;
            });

        this.delayedGraphDataUpdate = _.debounce(this.updateGraphData, 0);
        this.delayedGraphPositionUpdate = _.debounce(this.updateGraphPosition, 0);
    }

    public onGraphSelectionChanged(): Observable<boolean> {
        return this.graphSelectionChanged.asObservable();
    }

    public selectGraph(graph: Graph) {
        for (let g of this.graphs) {
            if (g.isSelected && g !== graph) {
                g.isSelected = false;
                g.updateData(['isSelected']);
            }
        }

        if (graph) {
            graph.isSelected = true;
            graph.updateData(['isSelected']);
            this.graphSelectionChanged.next(true);
        } else {
            this.graphSelectionChanged.next(false);
        }
    }

    public setGraphOffset(graph: Graph, offset: number) {
        graph.posOffset = offset;

        let sortedGraphs = _.sortBy(this.graphs, 'listIndex');
        let graphIndex = sortedGraphs.indexOf(graph);

        if (graph.posOffset < 0 && graphIndex > 0) {
            let prevGraph = sortedGraphs[graphIndex - 1];
            while (-graph.posOffset > prevGraph.width / 2) {
                this.moveLeft(graph);
                graph.posOffset += prevGraph.width;
            }
        } else if (graph.posOffset > 0 && graphIndex < sortedGraphs.length - 1) {
            let nextGraph = sortedGraphs[graphIndex + 1];
            while (graph.posOffset > nextGraph.width / 2) {
                this.moveRight(graph);
                graph.posOffset -= nextGraph.width;
            }
        }

        graph.updatePosition();
    }

    public moveLeft(graph: Graph) {
        let prevGraph = _.find(this.graphs, g => g.nextGraphId === graph.id);

        if (prevGraph) {
            let prevPrevGraph = _.find(this.graphs, g => g.nextGraphId === prevGraph.id);

            prevGraph.nextGraphId = graph.nextGraphId;
            graph.nextGraphId = prevGraph.id;

            prevGraph.updatePosition();
            graph.updatePosition();

            if (prevPrevGraph) {
                prevPrevGraph.nextGraphId = graph.id;
                prevPrevGraph.updatePosition();
            }
        }

        this.recalculateGraphIndices();
    }

    public moveRight(graph: Graph) {
        let nextGraph = _.find(this.graphs, g => graph.nextGraphId === g.id);
        let prevGraph = _.find(this.graphs, g => g.nextGraphId === graph.id);

        if (nextGraph) {
            graph.nextGraphId = nextGraph.nextGraphId;
            nextGraph.nextGraphId = graph.id;
            nextGraph.updatePosition();
            graph.updatePosition();

            if (prevGraph) {
                prevGraph.nextGraphId = nextGraph.id;
                prevGraph.updatePosition();
            }
        }

        this.recalculateGraphIndices();
    }

    public addGraph(): Graph {
        let graph = new Graph();
        graph.id = this.idCounter++;
        graph.color = COLOURS[graph.id % COLOURS.length];

        this.attachListeners(graph);

        if (this.graphs.length > 0) {
            graph.nextGraphId = _.find(this.graphs, g => g.listIndex === 0).id;
        }

        this.socketio.sendMessage('+graph', graph.toJson());

        this.graphs.push(graph);
        this.recalculateGraphIndices();
        this.graphObserver.next(this.graphs);

        return graph;
    }

    private attachListeners(graph: Graph): void {
        graph.onDataUpdate.subscribe(g => {
            this.graphDataUpdateQueue[g.id] = g.data;
            this.delayedGraphDataUpdate();
        });
        graph.onPositionUpdate.subscribe(g => {
            this.graphPositionUpdateQueue[g.id] = g;
            this.delayedGraphPositionUpdate();
        });
    }

    public getGraphs(): Observable<Graph[]> {
        return this.graphObserver.asObservable();
    }

    public removeGraph(graph: Graph): void {
        this.socketio.sendMessage('-graph', graph.id);
        _.pull(this.graphs, graph);

        let prevGraph = _.find(this.graphs, g => g.nextGraphId == graph.id);
        if (prevGraph) {
            prevGraph.nextGraphId = graph.nextGraphId;
        }
        this.recalculateGraphIndices();

        this.graphObserver.next(this.graphs);
    }



    private graphDataUpdateQueue: { [id: number]: any } = {};
    private graphPositionUpdateQueue: { [id: number]: any } = {};

    private updateGraphData(): void {
        let graphs = _.values(this.graphDataUpdateQueue);

        if (graphs.length > 0) {
            this.socketio.sendMessage('graph-data', {
                graphs: graphs
            });
            this.graphDataUpdateQueue = {};
        }
    }

    private updateGraphPosition(): void {
        let graphs = _.values(this.graphPositionUpdateQueue);

        if (graphs.length > 0) {
            this.socketio.sendMessage('graph-position', {
                graphs: graphs
            });
            this.graphPositionUpdateQueue = {};
        }
    }


    private recalculateGraphIndices(): void {
        let maxIndex = this.graphs.length - 1;
        let index = maxIndex;
        // start at the end, work backwards
        let graph = _.find(this.graphs, g => g.nextGraphId === -1);

        while (index >= 0 && graph) {
            graph.listIndex = index;
            index--;
            graph = _.find(this.graphs, g => g.nextGraphId === graph.id);
        }
    }
}
