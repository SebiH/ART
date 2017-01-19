import {
    Component,
    OnInit,
    OnDestroy,
    OnChanges,
    SimpleChanges,
    Input,
    ViewChild,
    ElementRef,
} from '@angular/core';
import * as d3 from 'd3';

@Component({
    selector: 'scatter-plot',
    templateUrl: './app/components/scatter-plot/scatter-plot.html',
    styleUrls: [ './app/components/scatter-plot/scatter-plot.css' ]
})
export class ScatterPlotComponent implements OnInit, OnDestroy, OnChanges {

    @ViewChild('graph') private graphElement: ElementRef;

    constructor() {

    }

    ngOnInit() {

    }

    ngOnDestroy() {

    }


    ngOnChanges(changes: SimpleChanges): void {
        console.log(changes);
    }
}
