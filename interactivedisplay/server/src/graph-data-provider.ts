export class GraphDataProvider {

    private dummyData: { [id: string]: number[]; } = {};

    public constructor(filename: string) {
        // TODO: read csv or something
        for (let dimension of this.getDimensions().dimensions) {
            let dummyData: number[] = [];
            for (let i = 0; i < 100; i++) {
                dummyData[i] = Math.random() - 0.5;
            }
            this.dummyData[dimension] = dummyData;
        }
    }

    public getDimensions(): any {
        // workaround since Unity needs an object type for JSON conversion
        return { dimensions:  [ "DUMMY_Calories", "DUMMY_Vitamin_C", "DUMMY_Vitamin_D", "DUMMY_Happiness", "DUMMY_Gewicht" ] };
    }

    public getData(dimension: string): any {
        return { data: this.dummyData[dimension] };
    }
}
