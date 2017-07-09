export interface RawDataPoint {
    value: number;
    isNull: boolean;
}

export interface RawData {
    id: number,
    dimensions: { [dim: string]: RawDataPoint }
}
