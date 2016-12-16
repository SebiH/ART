export class Point {
    public x: number;
    public y: number;

    public constructor(x: number, y: number) {
        this.x = x;
        this.y = y;
    }

    /**
     * Adds two or more points
     */
    public static add(...points: Point[]): Point {
        var x = 0;
        var y = 0;

        points.forEach((p) => {
            x += p.x;
            y += p.y;
        });

        return new Point(x, y);
    }

    /**
     * Subtracts points from firstPoint: firstPoint - point[0] - point[1] ...
     */
    public static sub(firstPoint: Point, ...points: Point[]): Point {
        var x = firstPoint.x;
        var y = firstPoint.y;

        points.forEach((p) => {
            x -= p.x;
            y -= p.y;
        });

        return new Point(x, y);
    }


    /**
     * Divides a point by a number.
     * @returns A new Point instance
     */
    public static divideBy(point: Point, by: number): Point {
        if (by !== 0) {
            return new Point(point.x / by, point.y / by);
        } else {
            // not correct, but probably better than exception
            return new Point(0, 0);
        }
    }

    public divideBy(by: number): Point {
        return Point.divideBy(this, by);
    }


    public static multiply(...points: Point[]): Point {
        var result = new Point(1, 1);

        for (var point of points) {
            result.x = result.x * point.x;
            result.y = result.y * point.y;
        }

        return result;
    }

    public multiply(point: Point) {
        return Point.multiply(this, point);
    }

    public static multiplyNum(point: Point, by: number): Point {
        return new Point(point.x * by, point.y * by);
    }

    public multiplyNum(by: number): Point {
        return Point.multiplyNum(this, by);
    }

    public static distanceBetween(p1: Point, p2: Point): number {
        return Math.sqrt(Math.pow(p2.x - p1.x, 2) + Math.pow(p2.y - p1.y, 2));
    }

    public distanceTo(p2: Point): number {
        return Point.distanceBetween(this, p2);
    }

    public equalTo(p2: Point): boolean {
        return this.x == p2.x && this.y == p2.y;
    }
}
