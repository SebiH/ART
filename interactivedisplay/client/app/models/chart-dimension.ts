import { Utils } from '../Utils';
import * as _ from 'lodash';

export interface GradientStop {
    stop: number;
    color: string
}

export class ChartDimension {
    public data: { id: string, value: number }[];
    public domain: { min: number, max: number };

    public name: string;

    public hideTicks: boolean;

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
            return this.domain.min - 0.0001;
        } else {
            return this.domain.min - 1;
        }
    }

    private minValCache: number;
    public getActualMinValue(): number {
        if (this.minValCache === undefined) {
            this.minValCache = _.minBy(this.data, 'value').value;
        }

        return this.minValCache;
    }

    public getMaxValue(): number {
        if (this.isMetric) {
            return this.domain.max + 0.0001;
        } else {
            return this.domain.max + 1;
        }
    }

    private maxValCache: number;
    public getActualMaxValue(): number {
        if (this.maxValCache === undefined) {
            this.maxValCache = _.maxBy(this.data, 'value').value;
        }

        return this.maxValCache;
    }

    public sortBy(dim: ChartDimension) {
        if (!dim) {
            return;
        }

        let sortedData = _.sortBy(this.data, (d) => {
            return -_.find(dim.data, (o) => o.id == d.id).value;
        });

        let oldMappings = this.mappings;
        this.mappings = [];
        for (let i = 0; i < this.data.length; i++) {

            let color = '#FFFFFF';
            if (this.isMetric) {
                color = Utils.getGradientColor(this.gradient, sortedData[i].value);
            } else {
                color = _.find(oldMappings, (m) => m.value == sortedData[i].value).color;
            }

            sortedData[i].value = i;
            this.mappings.push({
                value: i,
                name: '',
                color: ''
            });
        }

        this.domain = { min: 0, max: this.data.length };
        this.isMetric = false;
        this.hideTicks = true;
    }

    public clone(): ChartDimension {
        let dim = new ChartDimension();
        dim.data = _.cloneDeep(this.data);
        dim.domain = _.cloneDeep(this.domain);
        dim.name = this.name;
        dim.hideTicks = this.hideTicks;
        dim.isMetric = this.isMetric;
        dim.isTimeBased = this.isTimeBased;
        dim.timeFormat = this.timeFormat;
        dim.mappings = _.cloneDeep(this.mappings);
        dim.bins = _.cloneDeep(this.bins);
        dim.gradient = _.cloneDeep(this.gradient);
        dim.ticks = _.cloneDeep(this.ticks);
        return dim;
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
        dim.hideTicks = jDim.hideTicks;

        return dim;
    }
}
