import { Injectable } from '@angular/core';
import { Http } from '@angular/http';
import { ReplaySubject } from 'rxjs/ReplaySubject';
import { SocketIO } from './SocketIO.service';
import { Graph } from '../models/index';
import * as _ from 'lodash';

const COLOURS = [
    // material colour palette, see https://material.io/guidelines/style/color.html
    "#F44336", // red
    "#9C27B0", // purple
    "#3F51B5", // indigo
    "#2196F3", // blue
    "#4CAF50", // green
    "#FFEB3B", // yellow
    "#FF9800", // orange
    "#9E9E9E", // grey
];

@Injectable()
export class GraphProvider {
    private graphs: Graph[] = [];
    private graphObserver: ReplaySubject<Graph[]> = new ReplaySubject<Graph[]>(1);
    private idCounter: number = 0;

    private delayedGraphDataUpdate: Function;
    private delayedGraphPositionUpdate: Function;

    constructor(private socketio: SocketIO, private http: Http) {
        this.http.get('/api/graph/list')
            .subscribe(response => {
                // response gives graphs as interface, *not* as class
                this.graphs = _.map(<any[]>response.json().graphs, (g) => Graph.fromJson(g));
                if (this.graphs.length > 0) {
                    this.idCounter = _.max(<number[]>_.map(this.graphs, 'id')) + 1;
                }

                for (let graph of this.graphs) {
                    this.attachListeners(graph);
                }

                this.graphObserver.next(this.graphs);

                // for live editing via console
                window['graphs'] = this.graphs;
            });

        this.delayedGraphDataUpdate = _.debounce(this.updateGraphData, 0);
        this.delayedGraphPositionUpdate = _.debounce(this.updateGraphPosition, 0);
    }

    public addGraph(listIndex: number = 0): Graph {
        let graph = new Graph();
        graph.id = this.idCounter++;

        graph.color = COLOURS[graph.id % COLOURS.length];

        graph.listIndex = listIndex;
        for (let g of this.graphs) {
            if (g.listIndex >= listIndex) {
                g.listIndex++;
            }
        }

        this.socketio.sendMessage('+graph', graph.toJson());
        this.graphs.push(graph);
        this.graphObserver.next(this.graphs);

        return graph;
    }

    private attachListeners(graph: Graph): void {
        graph.onDataUpdate.subscribe(g => {
            this.graphDataUpdateQueue[g.id] = g;
            this.delayedGraphDataUpdate();
        });
        graph.onPositionUpdate.subscribe(g => {
            this.graphPositionUpdateQueue[g.id] = g;
            this.delayedGraphPositionUpdate();
        });
    }

    public getGraphs(): ReplaySubject<Graph[]> {
        return this.graphObserver;
    }

    public removeGraph(graph: Graph): void {
        this.socketio.sendMessage('-graph', graph.id);
        _.pull(this.graphs, graph);
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


}
