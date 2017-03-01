import { Component, AfterViewInit, OnChanges, SimpleChanges, ViewChild, Input } from '@angular/core';
import { ChartDimension } from '../../models/index';
import { ChartDirective } from '../../directives/index';

@Component({
    selector: 'chart-2d',
    template: `<div #chart
                     chart 
                     [width]="width"
                     [height]="height"
                     [margin]="{ top: 50, right: 50, bottom: 100, left: 100 }">
               </div>`
})
export class Chart2dComponent implements AfterViewInit, OnChanges {

    @Input() dimension: ChartDimension = null;
    @Input() width: number = 300;
    @Input() height: number = 900;

    @ViewChild('chart') chart: ChartDirective;

    ngAfterViewInit() {
        this.initialize();
    }

    ngOnChanges(changes: SimpleChanges) {
        this.initialize();
    }

    private initialize(): void {
        if (this.dimension == null) {
            // clear
        }
    }
}
