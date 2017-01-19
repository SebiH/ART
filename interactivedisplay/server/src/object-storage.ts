/**
 *    Semi-persistent storage for querying current graph/marker data
 *    TODO: database backed?
 */

import * as _ from 'lodash';

export class ObjectStorage {
    private storage: any[] = [];

    public set(obj: any) {
        let existingObj = _.find(this.storage, o => o.id == obj.id);

        if (existingObj) {
            _.assign(existingObj, obj);
        } else {
            this.storage.push(obj);
        }
    }

    public getAll(): any {
        return this.storage;
    }

    public remove(id: number) {
        _.remove(this.storage, o => o.id == id);
    }
}
