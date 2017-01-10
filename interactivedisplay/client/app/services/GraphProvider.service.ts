import { Injectable } from '@angular/core';
import { Http } from '@angular/http';
import { ReplaySubject } from 'rxjs/ReplaySubject';
import { SocketIO } from './SocketIO.service';
import { Graph } from '../models/index';
import * as _ from 'lodash';

@Injectable()
export class GraphProvider {
    private graphs: Graph[] = [];
    private graphObserver: ReplaySubject<Graph[]> = new ReplaySubject<Graph[]>(1);
    private idCounter: number = 0;

    constructor(private socketio: SocketIO, private http: Http) {
        this.http.get('/api/graph/list')
            .subscribe(response => {
                // previously created graphs will be ignored
                this.graphs = <Graph[]>response.json().graphs;
                this.idCounter = _.max(<number[]>_.map(this.graphs, 'id')) + 1;
                this.graphObserver.next(this.graphs);
            });
    }

    public addGraph(): Graph {
        let graph = new Graph();
        graph.id = this.idCounter++;

        graph.listIndex = 0;
        for (let g of this.graphs) {
            g.listIndex++;
        }

        this.socketio.sendMessage('+graph', graph.toJson());
        this.graphs.push(graph);
        this.graphObserver.next(this.graphs);

        graph.onDataUpdate.subscribe(g => this.socketio.sendMessage('graph-data', g));
        graph.onPositionUpdate.subscribe(g => this.socketio.sendMessage('graph-position', g));

        return graph;
    }

    public getGraphs(): ReplaySubject<Graph[]> {
        return this.graphObserver;
    }

    public removeGraph(graph: Graph): void {
        this.socketio.sendMessage('-graph', graph.id);
        _.pull(this.graphs, graph);
        this.graphObserver.next(this.graphs);
    }
}
