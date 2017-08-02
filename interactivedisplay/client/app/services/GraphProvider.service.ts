import { Injectable } from '@angular/core';
import { Http } from '@angular/http';
import { Observable } from 'rxjs/Observable';
import { Subject } from 'rxjs/Subject';
import { ReplaySubject } from 'rxjs/ReplaySubject';
import { SocketIO } from './SocketIO.service';
import { Graph } from '../models/index';
import { DataProvider, Dimension } from './DataProvider.service';
import { SettingsProvider, Settings } from './SettingsProvider.service';
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
    private graphSelectionChanged: ReplaySubject<Graph> = new ReplaySubject<Graph>(1);
    private graphDeletionObserver: Subject<Graph> = new Subject<Graph>();
    private graphColorChangeObserver: Subject<Graph> = new Subject<Graph>();
    private idCounter: number = 0;
    private settings: Settings = new Settings();
    private dimensions: Dimension[] = [];

    private delayedGraphUpdate: Function;

    constructor(
        private socketio: SocketIO,
        private http: Http,
        private dataProvider: DataProvider,
        private settingsProvider: SettingsProvider) {

        this.init();
        this.socketio.on('renew-graphs', () => this.init());
        this.delayedGraphUpdate = _.debounce(this.updateGraph, 0);
        this.dataProvider.getDimensions()
            .first()
            .subscribe(dims => this.dimensions = dims);
        this.settingsProvider.getCurrent()
            .subscribe((s) => this.settings = s);
    }

    private init(): void {
        while (this.graphs.length > 0) {
            this.graphs.pop();
        }
        // this.graphObserver.next(this.graphs);

        this.http.get('/api/graph/list')
            .subscribe(response => {
                // response gives graphs as interface, *not* as class
                this.graphs = _.map(<any[]>response.json().graphs, g => Graph.fromJson(g, this.dataProvider));
                if (this.graphs.length > 0) {
                    this.idCounter = _.max(<number[]>_.map(this.graphs, 'id')) + 1;
                }

                let selectedGraph = null;
                for (let graph of this.graphs) {
                    this.attachListeners(graph);

                    if (graph.isSelected) {
                        selectedGraph = graph;
                    }
                }

                this.recalculateGraphIndices();
                this.graphObserver.next(this.graphs);
                this.graphSelectionChanged.next(selectedGraph);

                // for live editing via console
                window['graphs'] = this.graphs;
            });
    }

    public onGraphSelectionChanged(): Observable<Graph> {
        return this.graphSelectionChanged.asObservable();
    }

    public onGraphDeletion(): Observable<Graph> {
        return this.graphDeletionObserver.asObservable();
    }

    public onGraphColorChange(): Observable<Graph> {
        return this.graphColorChangeObserver.asObservable();
    }

    public isFirst(graph: Graph): boolean {
        if (this.graphs.length > 1) {
            return _.first(_.orderBy(this.graphs, (g) => g.absolutePos)).absolutePos >= graph.absolutePos;
        }
        return true;
    }

    public getPrevGraph(graph: Graph): Graph {
        let prevGraph: Graph = null;

        for (let g of this.graphs) {
            if (g.absolutePos < graph.absolutePos) {
                if (prevGraph == null || prevGraph.absolutePos < g.absolutePos) {
                    prevGraph = g;
                }
            }
        }

        return prevGraph;
    }

    public getNextGraph(graph: Graph): Graph {
        let nextGraph: Graph = null;

        for (let g of this.graphs) {
            if (g.absolutePos > graph.absolutePos) {
                if (nextGraph == null || nextGraph.absolutePos > g.absolutePos) {
                    nextGraph = g;
                }
            }
        }

        return nextGraph;
    }

    public toggleColorIncline(graph: Graph): void {
        if (!graph.colorIncline) {
            for (let g of this.graphs) {
                g.isColored = false;
                g.colorIncline = false;
            }
        }

        graph.colorIncline = !graph.colorIncline;

        if (graph.colorIncline) {
            this.graphColorChangeObserver.next(graph);
        } else {
            this.graphColorChangeObserver.next(null);
        }
    }

    public toggleSortIncline(graph: Graph): void {
        if (!graph.sortIncline) {
            let prevGraph = this.getPrevGraph(graph);
            if (prevGraph) {
                prevGraph.sortAxis = false;
                prevGraph.sortIncline = false;
            }

            let nextGraph = this.getNextGraph(graph);
            if (nextGraph) {
                nextGraph.sortIncline = false;
            }

            graph.sortAxis = false;
        }

        graph.sortIncline = !graph.sortIncline;
    }

    public setColor(graph: Graph): void {
        for (let g of this.graphs) {
            g.colorIncline = false;
            if (g != graph) {
                g.isColored = false;
            }
        }

        if (graph) {
            graph.isColored = true;
        }

        this.graphColorChangeObserver.next(graph);
    }

    public toggleSort(graph: Graph): void {

        if (!graph.sortAxis) {
            let nextGraph = this.getNextGraph(graph);
            if (nextGraph) {
                nextGraph.sortIncline = false;
            }

            graph.sortIncline = false;
        }

        graph.sortAxis = !graph.sortAxis;
    }

    public selectGraph(graph: Graph): void {
        for (let g of this.graphs) {
            if (g.isSelected && g !== graph) {
                g.isSelected = false;
            }
        }

        if (graph) {
            graph.isSelected = true;
        }

        this.recalculateGraphIndices();
        this.graphSelectionChanged.next(graph);
    }


    public addGraph(): Graph {
        let graph = new Graph(this.idCounter++);
        graph.color = COLOURS[graph.id % COLOURS.length];
        graph.isNewlyCreated = true;
        graph.phase = this.dimensions[0].phases[0];

        if (this.settings.lockDimension != '') {
            this.dataProvider.getData(this.settings.lockDimension)
                .first()
                .subscribe(data => graph.setDimX(data));
        }

        this.attachListeners(graph);
        this.socketio.sendMessage('+graph', graph.toJson());

        this.graphs.push(graph);
        this.graphObserver.next(this.graphs);

        return graph;
    }

    private attachListeners(graph: Graph): void {
        graph.onUpdate
            .subscribe(changes => {
                this.graphUpdateQueue[graph.id] = graph.toJson();
                this.delayedGraphUpdate();
            });

        graph.onUpdate
            .filter(changes => changes.indexOf('absolutePos') >= 0)
            .subscribe(changes => this.recalculateGraphIndices());
    }

    public getGraphs(): Observable<Graph[]> {
        return this.graphObserver.asObservable();
    }

    public removeGraph(graph: Graph): void {
        this.socketio.sendMessage('-graph', graph.id);
        _.pull(this.graphs, graph);
        this.recalculateGraphIndices();
        graph.destroy();
        this.graphObserver.next(this.graphs);
        this.graphDeletionObserver.next(graph);

        if (graph.isSelected) {
            this.selectGraph(null);
        }
    }


    private graphUpdateQueue: { [id: number]: any } = {};

    private updateGraph(): void {
        let graphs = _.values(this.graphUpdateQueue);

        if (graphs.length > 0) {
            this.socketio.sendMessage('graph', {
                graphs: graphs
            });
            this.graphUpdateQueue = {};
        }
    }

    public recalculateGraphIndices(): void {
        var sortedGraphs = _.sortBy(this.graphs, 'absolutePos');
        var listIndexCounter = 0;

        for (let graph of sortedGraphs) {
            if (!graph.isNewlyCreated) {
                var prevListIndex = graph.listIndex;
                graph.listIndex = listIndexCounter;

                if (graph.isPickedUp) {
                    graph.posOffset += graph.width * (prevListIndex - graph.listIndex);
                }

                listIndexCounter++;
            }
        }
    }
}
