export interface ChartDimension {
    data: number[];
    domain: { min: number, max: number };

    name: string;

    isMetric: boolean;
    // only if isNumeric is false
    mappings?: { value: number, name: string, color: string }[];

    // only if isNumeric is true
    bins?: { displayName: string, value?: number, range?: [number, number] }[];
    gradient?: { stop: number, color: string }[];
}

