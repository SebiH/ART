import { Filter } from './filter';
import { Graph } from '../graph';

export class CategoryFilter extends Filter {

    public color: string;

    protected _category : number;
    public get category() : number {
        return this._category;
    }
    public set category(v : number) {
        if (this._category !== v) {
            this._category = v;
            this.generatePath();
        }
    }


    // todo
    private generatePath(): void {

    }


    public toJson(): any {
        let jFilter = super.toJson();
        jFilter.color = this.color;
        jFilter.category = this.category;
        
        return jFilter;
    }

    public static fromJson(jFilter: any, origin: Graph): Filter {
        let filter = new CategoryFilter(jFilter.id);
        filter.applyJsonProperties(jFilter, origin);
        return filter;
    }

    protected applyJsonProperties(jFilter: any, origin: Graph): void {
        super.applyJsonProperties(jFilter, origin);
        this.color = jFilter.color;
        this._category = jFilter.category;
    }
}
