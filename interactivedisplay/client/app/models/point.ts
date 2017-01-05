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

    /**
     * Checks if the point is contained inside a given polygon.
     * BoundingRect[0] == topLeft, BoundingRect[1] == bottomRight
     * See http://stackoverflow.com/a/218081/4090817
     */
    public isInPolygon(polygon: Point[], boundingRect: Point[]): boolean {
        if (this.x < boundingRect[0].x || this.x > boundingRect[1].x
            || this.y < boundingRect[0].y || this.y > boundingRect[1].y) {
            return false;
        }

        let intersections = 0;
        let startPoint = new Point(boundingRect[0].x - Number.EPSILON, this.y);
        for (let index = 0; index < polygon.length; index++) {
            if (Point.areIntersecting(polygon[index], polygon[(index + 1) % polygon.length], startPoint, this)) {
                intersections++;
            }
        }

        return (intersections & 1) === 1;
    }

    /**
     * Determines if two vectors are intersecting
     */
    private static areIntersecting(v1start: Point, v1end: Point, v2start: Point, v2end: Point): boolean {

        /* Adapted from http://stackoverflow.com/a/218081/4090817 */

        // Convert vector 1 to a line (line 1) of infinite length.
        // We want the line in linear equation standard form: A*x + B*y + C = 0
        // See: http://en.wikipedia.org/wiki/Linear_equation
        let a1 = v1end.y - v1start.y;
        let b1 = v1start.x - v1end.x;
        let c1 = (v1end.x * v1start.y) - (v1start.x * v1end.y);

        // Every point (x,y), that solves the equation above, is on the line,
        // every point that does not solve it, is not. The equation will have a
        // positive result if it is on one side of the line and a negative one 
        // if is on the other side of it. We insert (x1,y1) and (x2,y2) of vector
        // 2 into the equation above.
        let d1 = (a1 * v2start.x) + (b1 * v2start.y) + c1;
        let d2 = (a1 * v2end.x) + (b1 * v2end.y) + c1;

        // If d1 and d2 both have the same sign, they are both on the same side
        // of our line 1 and in that case no intersection is possible. Careful, 
        // 0 is a special case, that's why we don't test ">=" and "<=", 
        // but "<" and ">".
        if (d1 > 0 && d2 > 0) return false;
        if (d1 < 0 && d2 < 0) return false;

        // The fact that vector 2 intersected the infinite line 1 above doesn't 
        // mean it also intersects the vector 1. Vector 1 is only a subset of that
        // infinite line 1, so it may have intersected that line before the vector
        // started or after it ended. To know for sure, we have to repeat the
        // the same test the other way round. We start by calculating the 
        // infinite line 2 in linear equation standard form.
        let a2 = v2end.y - v2start.y;
        let b2 = v2start.x - v2end.x;
        let c2 = (v2end.x * v2start.y) - (v2start.x * v2end.y);

        // Calculate d1 and d2 again, this time using points of vector 1.
        d1 = (a2 * v1start.x) + (b2 * v1start.y) + c2;
        d2 = (a2 * v1end.x) + (b2 * v1end.y) + c2;

        // Again, if both have the same sign (and neither one is 0),
        // no intersection is possible.
        if (d1 > 0 && d2 > 0) return false;
        if (d1 < 0 && d2 < 0) return false;

        // If we get here, only two possibilities are left. Either the two
        // vectors intersect in exactly one point or they are collinear, which
        // means they intersect in any number of points from zero to infinite.
        // TODO: not sure how to handle this case
        // if ((a1 * b2) - (a2 * b1) == 0.0f) return COLLINEAR;

        // If they are not collinear, they must intersect in exactly one point.
        return true;
    }

    public toString(): string {
        return '(' + this.x + ',' + this.y + ')';
    }
}
