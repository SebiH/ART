
export class Graph {
    public id: number = -1;

    public dimY: string = "";
    public dimX: string = "";

    public toJson(): any {
        return {
            id: this.id,
            dimX: this.dimX,
            dimY: this.dimY
        };
    }
}
