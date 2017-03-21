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
    dbValue: string;
    name: string;
    color: string;
}

export interface CategoricalSqlMapping {
    type: DataRepresentation.Categorical;
    dbColumn: string;
    name: string;

    values: ValueMapping[];
}


export interface MetricSqlMapping {
    type: DataRepresentation.Metric;
    dbColumn: string;
    name: string;

    minValue: number;
    maxValue: number;
    bins: Bin[];
    gradient: Gradient[];
}

export type SqlColumnMapping = CategoricalSqlMapping | MetricSqlMapping;
