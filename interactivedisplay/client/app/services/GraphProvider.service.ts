import { Injectable } from '@angular/core';
import { SocketIO } from './SocketIO.service';
import { Graph } from '../models/index';
import * as _ from 'lodash';

@Injectable()
export class GraphProvider {
    private graphs: Graph[] = [];
    private idCounter: number = 0;

    constructor(private socketio: SocketIO) {

    }

    public addGraph(dimX: string, dimY: string): Graph {
        let graph = new Graph();
        graph.id = this.idCounter++;
        graph.dimX = dimX;
        graph.dimY = dimY;
        this.socketio.sendMessage('plane-add', graph.toJson());

        this.graphs.unshift(graph);
        this.sendGraphOrderToUnity();

        return graph;
    }

    public getGraphs(): Graph[] {
        return this.graphs;
    }

    public removeGraph(graph: Graph): void {
        this.socketio.sendMessage('plane-remove', graph.id);
        _.pull(this.graphs, graph);
        this.sendGraphOrderToUnity();
    }

    public moveLeft(graph: Graph) {
        let oldIndex = this.graphs.indexOf(graph);

        if (oldIndex > 0) {
            _.pull(this.graphs, graph);
            this.graphs.splice(oldIndex - 1, 0, graph);
            this.sendGraphOrderToUnity();
        }
    }

    public moveRight(graph: Graph) {
        let oldIndex = this.graphs.indexOf(graph);

        if (oldIndex < this.graphs.length - 1) {
            _.pull(this.graphs, graph);
            this.graphs.splice(oldIndex + 1, 0, graph);
            this.sendGraphOrderToUnity();
        }
    }


    public sendGraphOrderToUnity(): void {
        let ids = _.map(this.graphs, 'id');
        this.socketio.sendMessage('plane-order', { ids: ids });
    }
}
