import { Point } from './models/index';

export class Utils {
    public static padLeft(str: string, length: number): string {
        while (str.length < length) {
            str = '0' + str;
        }
        return str;
    }

    public static getBaseUrl() {
        return window.location.href.replace(/\!+/, '').replace(/\#+/, '');
    }

    public static buildBoundingRect(polygon: number[][]): Point[] {
        if (polygon.length === 0) {
            return [new Point(0, 0), new Point(0, 0)];
        }

        let topLeft = new Point(polygon[0][0], polygon[0][1]);
        let bottomRight = new Point(polygon[0][1], polygon[0][1]);

        for (let p of polygon) {
            topLeft.x = Math.min(topLeft.x, p[0]);
            topLeft.y = Math.min(topLeft.y, p[1]);
            bottomRight.x = Math.max(bottomRight.x, p[0]);
            bottomRight.y = Math.max(bottomRight.y, p[1]);
        }

        return [topLeft, bottomRight];
    }


    private static idCounter: number = 0;
    public static getId(): number {
        return this.idCounter++;
    }
}
