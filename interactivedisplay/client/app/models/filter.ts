import { Graph } from './graph';

export class Filter {
    public indices: number[];
    public path: number[][];
    public origin: Graph;
    public isColored: boolean;
    public color: string;
}
