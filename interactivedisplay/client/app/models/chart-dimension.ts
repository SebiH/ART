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
