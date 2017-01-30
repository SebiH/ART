import { Component, Input, OnInit, OnDestroy, ViewChild, ElementRef } from '@angular/core';
import { SocketIO } from '../../services/index';
import { ScatterPlotComponent } from '../scatter-plot/scatter-plot';

import * as d3 from 'd3';

const MAX_SAMPLES = 100;

@Component({
    selector: 'admin-change-monitor',
    templateUrl: './app/components/admin-change-monitor/admin-change-monitor.html',
    styleUrls: ['./app/components/admin-change-monitor/admin-change-monitor.css']
})
export class AdminChangeMonitorComponent implements OnInit, OnDestroy {

    @ViewChild('plot') private plot: ElementRef;

    @Input() private height: number = 500;
    @Input() private width: number = 500;
    @Input() private name: string = '';

    private data: any[] = [];

    private stabilityPath: any;
    private stabilityLine: any;

    private positionPath: any;
    private positionLine: any;

    private rotationPath: any;
    private rotationLine: any;

    private socketiofn: any;

    constructor (private socketio: SocketIO) { }

    ngOnInit() {
        this.socketiofn = (data) => this.onSocketData(data);
        this.socketio.on('debug-cm-val-' + this.name, this.socketiofn);
        this.initD3();
    }

    ngOnDestroy() {
        this.socketio.off('debug-cm-val-' + this.name, this.socketiofn)
    }

    private onSocketData(dataPoint): void {
        this.data.push(JSON.parse(dataPoint));
        this.updateD3();
        while (this.data.length > MAX_SAMPLES) {
            this.data.shift();
        }
    }


    private initD3(): void {
        let svg = d3.select(this.plot.nativeElement).append('svg');
        let margin = {top: 20, right: 20, bottom: 20, left: 40};
        let actualWidth = this.width - margin.left - margin.right;
        let actualHeight = this.height - margin.top - margin.bottom;
        svg.attr('height', this.height);
        svg.attr('width', this.width);

        let g = svg.append("g").attr("transform", "translate(" + margin.left + "," + margin.top + ")");
        let x = d3.scaleLinear()
            .domain([0, MAX_SAMPLES - 1])
            .range([0, actualWidth]);
        let y = d3.scaleLinear()
            .domain([0, 1])
            .range([actualHeight, 0]);

        this.stabilityLine = d3.line()
            .x((d, i) => x(i))
            .y((d:any, i) => y(d.stability));
        this.positionLine = d3.line()
            .x((d, i) => x(i))
            .y((d:any, i) => y(d.position));
        this.rotationLine = d3.line()
            .x((d, i) => x(i))
            .y((d:any, i) => y(d.rotation));

        g.append("defs").append("clipPath")
            .attr("id", "clip")
            .append("rect")
            .attr("width", actualWidth)
            .attr("height", actualHeight);
        g.append("g")
            .attr("transform", "translate(0," + y(0) + ")")
            .call(d3.axisBottom(x).tickValues([]));
        g.append("g")
            .call(d3.axisLeft(y).tickSizeInner(-this.width));
        this.stabilityPath = g.append("path");
        this.positionPath = g.append("path");
        this.rotationPath = g.append("path");

        this.stabilityPath
            .datum(this.data)
            .attr('fill', 'none')
            .attr('stroke', 'black')
            .attr('stroke-width', '2px')
            .attr('d', this.stabilityLine);

        this.positionPath
            .datum(this.data)
            .attr('fill', 'none')
            .attr('stroke', '#4CAF50')
            .attr('stroke-width', '1px')
            .attr('d', this.positionLine);

        this.rotationPath
            .datum(this.data)
            .attr('fill', 'none')
            .attr('stroke', '#03A9F4')
            .attr('stroke-width', '1px')
            .attr('d', this.rotationLine);
    }

    private updateD3(): void {
        this.stabilityPath
            .datum(this.data)
            .attr('d', this.stabilityLine);
        this.positionPath
            .datum(this.data)
            .attr('d', this.positionLine);
        this.rotationPath
            .datum(this.data)
            .attr('d', this.rotationLine);
    }
}
