import { Component, OnInit } from '@angular/core';
import { Http, Response } from '@angular/http';

@Component({
  selector: 'demo-view',
  templateUrl: './app/components/demo/demo.html',
  styleUrls: ['./app/components/demo/demo.css'],
})
export class DemoComponent implements OnInit {
    verticalSections:number[] = [];
    horizontalSections:number[] = [];

    selV = -1;
    selH = -1;

    constructor (private http: Http) {}

    ngOnInit() {
        var viewport = this.getViewport();

        var w = 0;
        while (w < viewport[0] - 150) {
            this.horizontalSections.push(w);
            w += 150;
        }

        var h = 0;
        while (h < viewport[1] - 150) {
            this.verticalSections.push(h);
            h += 150;
        }
    }

    // see: http://stackoverflow.com/a/2035211/4090817
    private getViewport():number[] {

        var viewPortWidth;
        var viewPortHeight;

        // the more standards compliant browsers (mozilla/netscape/opera/IE7) use window.innerWidth and window.innerHeight
        if (typeof window.innerWidth != 'undefined') {
            viewPortWidth = window.innerWidth,
            viewPortHeight = window.innerHeight
        }
        // IE6 in standards compliant mode (i.e. with a valid doctype as the first line in the document)
        else if (typeof document.documentElement != 'undefined'
            && typeof document.documentElement.clientWidth !=
            'undefined' && document.documentElement.clientWidth != 0) {
                viewPortWidth = document.documentElement.clientWidth,
                viewPortHeight = document.documentElement.clientHeight
        }
        // older versions of IE
        else {
            viewPortWidth = document.getElementsByTagName('body')[0].clientWidth,
            viewPortHeight = document.getElementsByTagName('body')[0].clientHeight
        }

        return [viewPortWidth, viewPortHeight];
    }

    
    onClick(ver, hor) {
        this.selV = ver;
        this.selH = hor;

        this.http.post('/click', {
            x: hor,
            y: ver
        }).subscribe();
    }
}
