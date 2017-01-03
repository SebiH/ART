
export class GraphPosition {
    public id: number = -1;

    public absolutePos: number = 0;
    public listPos: number = 0;

    public toJson(): any {
        return {
            id: this.id,
            absolutePos: this.absolutePos,
            listPos: this.listPos
        };
    }
}
