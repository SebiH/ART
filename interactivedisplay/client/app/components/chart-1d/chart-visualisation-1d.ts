import { ChartElement } from '../../directives/index';
import { ChartDimension } from '../../models/index';

export abstract class ChartVisualisation1d extends ChartElement {
    public dimension: ChartDimension;
    public abstract invert(val: number): number;
}
