import * as _ from 'lodash';

export interface GradientStop {
    stop: number;
    color: string
}

export class ChartDimension {
    public data: { id: string, value: number }[];
    public domain: { min: number, max: number };

    public name: string;

    public isMetric: boolean;

    public isTimeBased: boolean;
    public timeFormat?: string;

    // only if isMetric is false
    public mappings?: { value: number, name: string, color: string }[];

    // only if isMetric is true
    public bins?: { displayName: string, value?: number, range?: [number, number] }[];
    public gradient?: GradientStop[];
    public ticks: number[];

    public getMinValue(): number {
        if (this.isMetric) {
            return this.domain.min;
        } else {
            return _.minBy(this.mappings, 'value').value - 1;
        }
    }

    public getMaxValue(): number {
        if (this.isMetric) {
            return this.domain.max;
        } else {
            return _.maxBy(this.mappings, 'value').value + 1;
        }
    }

    public static fromJson(jDim: any): ChartDimension {
        let dim = new ChartDimension();
        dim.data = jDim.data;
        dim.domain = jDim.domain;
        dim.name = jDim.name;
        dim.isMetric = jDim.isMetric;
        dim.isTimeBased = jDim.isTimeBased;
        dim.timeFormat = jDim.timeFormat;
        dim.mappings = jDim.mappings;
        dim.bins = jDim.bins;
        dim.gradient = jDim.gradient;
        dim.ticks = jDim.ticks;

        return dim;
    }
}
