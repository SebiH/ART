export class Surface {
    public name: string = "Surface";
    public width: number;
    public height: number;
    public pixelToCmRatio: number = 0.0485; // measured 2016-11-15 on Microsoft Surface

    public constructor() {
        this.width = window.innerWidth;
        this.height = window.innerHeight;
    }

    public toJson(): any {
        return {
            name: this.name,
            pixelToCmRatio: this.pixelToCmRatio,
            width: this.width,
            height: this.height
        }
    }
}
