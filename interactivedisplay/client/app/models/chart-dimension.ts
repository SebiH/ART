export class ChartDimension {
    data: number[];
    domain: { min: number, max: number };

    name: string;

    isNumeric: boolean;
    // only if isNumeric is false
    mappings?: { value: number, name: string }[];
}
