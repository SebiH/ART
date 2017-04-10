export interface GradientStop {
    stop: number;
    color: string 
}

export interface ChartDimension {
    data: { id: string, value: number }[];
    domain: { min: number, max: number };

    name: string;

    isMetric: boolean;

    isTimeBased: boolean;
    timeFormat?: string;

    // only if isMetric is false
    mappings?: { value: number, name: string, color: string }[];

    // only if isMetric is true
    bins?: { displayName: string, value?: number, range?: [number, number] }[];
    gradient?: GradientStop[];
    ticks: number[];
}
