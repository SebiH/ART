import { Point } from './models/index';
import * as _ from 'lodash';

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

    public static buildBoundingRect(polygon: [number, number][]): Point[] {
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
        return Utils.idCounter++;
    }



    public static getGradientColor(gradients: {stop: number, color: string}[], value: number): string {
        let sortedGradients = _.sortBy(gradients, 'stop');

        for (let i = 0; i < sortedGradients.length - 1; i++) {
            let currStop = sortedGradients[i];
            let nextStop = sortedGradients[i + 1];

            if (gradients[i].stop <= value && value <= gradients[i + 1].stop) {
                let range = nextStop.stop - currStop.stop;
                let gradientPos = (value - currStop.stop) / range;
                return Utils.lerpColor(currStop.color, nextStop.color, gradientPos);
            }
        }


        if (value <= 0) {
            return sortedGradients[0].color;
        } else {
            return sortedGradients[sortedGradients.length - 1].color;
        }
    }


    public static lerpColor(col1: string, col2: string, weight: number): string {
        let rgb1 = Utils.hex2rgb(col1);
        let rgb2 = Utils.hex2rgb(col2);

        // let hsv1 = Utils.rgb2hsv(rgb1.r, rgb1.g, rgb1.b);
        // let hsv2 = Utils.rgb2hsv(rgb2.r, rgb2.g, rgb2.b);

        // let resultHsv = {
        //     h: Utils.lerp(hsv1.h, hsv2.h, weight),
        //     s: Utils.lerp(hsv1.s, hsv2.s, weight),
        //     v: Utils.lerp(hsv1.v, hsv2.v, weight),
        // };
        // let resultRgb = Utils.hsv2rgb(resultHsv.h, resultHsv.s, resultHsv.v);
        let resultRgb = {
            r: Math.floor(Utils.lerp(rgb1.r, rgb2.r, weight)),
            g: Math.floor(Utils.lerp(rgb1.g, rgb2.g, weight)),
            b: Math.floor(Utils.lerp(rgb1.b, rgb2.b, weight))
        }

        return Utils.rgbToHex(resultRgb.r, resultRgb.g, resultRgb.b);
    }

    public static lerp(val1: number, val2: number, weight: number): number {
        return val1 + weight * (val2 - val1);
    }


    // adapted from http://stackoverflow.com/a/5624139/4090817
    public static rgbToHex(r, g, b): string {
        return "#" + Utils.componentToHex(r) + Utils.componentToHex(g) + Utils.componentToHex(b);
    }

    // adapted from http://stackoverflow.com/a/5624139/4090817
    public static componentToHex(c): string {
        var hex = c.toString(16);
        return hex.length == 1 ? "0" + hex : hex;
    }

    // adapted from http://stackoverflow.com/a/5624139/4090817
    public static hex2rgb(hex) {
        // Expand shorthand form (e.g. "03F") to full form (e.g. "0033FF")
        var shorthandRegex = /^#?([a-f\d])([a-f\d])([a-f\d])$/i;
        hex = hex.replace(shorthandRegex, function(m, r, g, b) {
            return r + r + g + g + b + b;
        });

        var result = /^#?([a-f\d]{2})([a-f\d]{2})([a-f\d]{2})$/i.exec(hex);
        return result ? {
            r: parseInt(result[1], 16),
            g: parseInt(result[2], 16),
            b: parseInt(result[3], 16)
        } : null;
    }



    // adapted from: http://stackoverflow.com/a/8023734/4090817
    public static rgb2hsv(r255, g255, b255) {
        let rr, gg, bb;
        let r = r255 / 255;
        let g = g255 / 255;
        let b = b255 / 255;
        let v = Math.max(r, g, b);
        let diff = v - Math.min(r, g, b);

        let h = 0;
        let s = 0;

        if (diff !== 0) {
            s = diff / v;
            rr = Utils.diffc(v, r, diff);
            gg = Utils.diffc(v, g, diff);
            bb = Utils.diffc(v, b, diff);

            if (r === v) {
                h = bb - gg;
            } else if (g === v) {
                h = (1 / 3) + rr - bb;
            } else if (b === v) {
                h = (2 / 3) + gg - rr;
            }

            if (h < 0) {
                h += 1;
            } else if (h > 1) {
                h -= 1;
            }
        }

        return {
            h: Math.round(h * 360),
            s: s,
            v: v
        };
    }

    private static diffc(v, c, diff) {
        return (v - c) / 6 / diff + 1 / 2;
    };


    // adapted from: https://github.com/tmpvar/hsv2rgb
    private static set(r, g, b, out) {
        out[0] = Math.round(r * 255);
        out[1] = Math.round(g * 255);
        out[2] = Math.round(b * 255);
    }

    // adapted from: https://github.com/tmpvar/hsv2rgb
    public static hsv2rgb(h, s, v) {
        let out = [0, 0, 0];
        h = h % 360;
        s = _.clamp(s, 0, 1);
        v = _.clamp(v, 0, 1);

        // Grey
        if (!s) {
            out[0] = out[1] = out[2] = Math.ceil(v * 255);
        } else {
            var b = ((1 - s) * v);
            var vb = v - b;
            var hm = h % 60;
            switch((h/60)|0) {
                case 0: Utils.set(v, vb * h / 60 + b, b, out); break;
                case 1: Utils.set(vb * (60 - hm) / 60 + b, v, b, out); break;
                case 2: Utils.set(b, v, vb * hm / 60 + b, out); break;
                case 3: Utils.set(b, vb * (60 - hm) / 60 + b, v, out); break;
                case 4: Utils.set(vb * hm / 60 + b, b, v, out); break;
                case 5: Utils.set(v, b, vb * (60 - hm) / 60 + b, out); break;
            }
        }

        return {
            r: out[0],
            g: out[1],
            b: out[2]
        };
    }
}
