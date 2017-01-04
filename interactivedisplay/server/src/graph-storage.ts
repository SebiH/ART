/**
 *    Semi-persistent storage for querying current graph data
 */

import * as _ from 'lodash';

export class GraphStorage {
    private storage: any[] = [];

    public setGraph(graph: any) {
        let existingGraph = _.find(this.storage, g => g.id == graph.id);

        if (existingGraph) {
            _.assign(existingGraph, graph);
        } else {
            this.storage.push(graph);
        }
    }

    public getGraphs(): any {
        // workaround since unity needs an object
        return { graphs: this.storage };
    }

    public removeGraph(id: number) {
        _.remove(this.storage, g => g.id == id);
    }
}
