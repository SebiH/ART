export enum DataRepresentation {
    Metric, Categorical
}

export interface Gradient {
    stop: number;
    color: string;
}

export interface Bin {
    displayName: string;
    value?: number;
    range?: [number, number];
}

export interface ValueMapping {
    dbValue: number;
    name: string;
    color: string;
}

export interface CategoricalSqlMapping {
    type: DataRepresentation.Categorical;
    dbColumn: string;
    name: string;
    converter: (d: any) => number;

    values: ValueMapping[];
}


export interface MetricSqlMapping {
    type: DataRepresentation.Metric;
    dbColumn: string;
    name: string;
    converter: (d: any) => number;

    minValue: number;
    maxValue: number;
    bins: Bin[];
    gradient: Gradient[];
}

export type SqlColumnMapping = CategoricalSqlMapping | MetricSqlMapping;
