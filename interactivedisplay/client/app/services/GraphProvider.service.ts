import { Injectable } from '@angular/core';
import { SocketIO } from './SocketIO.service';
import { Graph } from '../models/index';

@Injectable()
export class GraphProvider {
    private graphs: Graph[] = [];
    private idCounter: number = 0;

    constructor(private socketio: SocketIO) {

    }

    public addGraph(dimX: string, dimY: string) {
        let graph = new Graph();
        graph.id = this.idCounter++;
        graph.dimX = dimX;
        graph.dimY = dimY;

        this.graphs.push(graph);
    }

    public getGraphs(): Graph[] {
        return this.graphs;
    }
}
