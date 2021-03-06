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
    phases: string[];
    converter: (d: any) => number;

    hideTicks?: boolean;
    limitValues?: number[];
    autoGenerateValues?: boolean;

    values: ValueMapping[];
}


export interface MetricSqlMapping {
    type: DataRepresentation.Metric;
    dbColumn: string;
    name: string;
    phases: string[];
    converter: (d: any) => number;

    // quick hack to trigger time-based restrictions in sql
    dbTime?: boolean;
    isTimeBased: boolean;
    timeFormat?: string;

    hideTicks?: boolean;

    minValue: number;
    maxValue: number;
    bins: Bin[];
    gradient: Gradient[];
    ticks: number[];
}

export type SqlColumnMapping = CategoricalSqlMapping | MetricSqlMapping;
