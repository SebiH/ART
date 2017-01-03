export class GraphDataProvider {

    private dummyData: { [id: string]: number[]; } = {};

    public constructor(filename: string) {
        // TODO: read csv or something
        for (let dimension of this.getDimensions()) {
            let dummyData: number[] = [];
            for (let i = 0; i < 100; i++) {
                dummyData[i] = Math.random() - 0.5;
            }
            this.dummyData[dimension] = dummyData;
        }
    }

    public getDimensions(): string[] {
        return [ "DUMMY_Calories", "DUMMY_Vitamin_C", "DUMMY_Vitamin_D", "DUMMY_Happiness", "DUMMY_Gewicht" ];
    }

    public getData(dimension: string): number[] {
        return this.dummyData[dimension];
    }
}
