import { Component, ViewEncapsulation, OnInit, OnDestroy, ViewChild, ElementRef } from '@angular/core';
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

    @ViewChild('plot')
    private plot: ElementRef;

    private data: number[] = [];
    private path: any;

    constructor (private socketio: SocketIO) { }

    ngOnInit() {
        this.socketio.on('debug-cm-val', (data) => this.onSocketData(data));
        this.initD3();
        window['data'] = this.data;
    }

    ngOnDestroy() {
        // todo..
        // this.socketio.off('debug-cm-val', this.onSocketData)
    }

    private onSocketData(dataPoint): void {
        this.data.push(dataPoint);
        this.updateD3();
        while (this.data.length > MAX_SAMPLES) {
            this.data.shift();
        }
    }

    private line: any;
    private x: any;
    private y: any;

    private initD3(): void {
        let svg = d3.select(this.plot.nativeElement).append('svg');
        let margin = {top: 20, right: 20, bottom: 20, left: 40};
        let width = window.innerWidth - margin.left - margin.right;
        let height = window.innerHeight - margin.top - margin.bottom;
        svg.attr('height', window.innerHeight);
        svg.attr('width', window.innerWidth);

        let g = svg.append("g").attr("transform", "translate(" + margin.left + "," + margin.top + ")");
        this.x = d3.scaleLinear()
            .domain([0, MAX_SAMPLES - 1])
            .range([0, width]);
        this.y = d3.scaleLinear()
            .domain([0, 1])
            .range([height, 0]);

        this.line = d3.line()
            .x((d, i) => this.x(i))
            .y((d, i) => this.y(d));

        g.append("defs").append("clipPath")
            .attr("id", "clip")
            .append("rect")
            .attr("width", width)
            .attr("height", height);
        g.append("g")
            .attr("class", "axis axis--x")
            .attr("transform", "translate(0," + this.y(0) + ")")
            .call(d3.axisBottom(this.x));
        g.append("g")
            .attr("class", "axis axis--y")
            .call(d3.axisLeft(this.y));
        this.path = //g.append("g")
            //.attr("clip-path", "url(#clip)")
            g.append("path")

            this.path
                .datum(this.data)
                .attr('fill', 'none')
                .attr('stroke', 'black')
                .attr('stroke-width', '1.5px')
                .attr('d', this.line);
            // .transition()
            //     .duration(500)
            //     .ease(d3.easeLinear)
            //     .on("start", tick);
    }

    private updateD3(): void {
        if (this.path) {
            this.path
                .datum(this.data)
                .attr('d', this.line);
                // .each(function() {
                //     d3.select(this)
                //         .attr('d', this.line)
                //         .attr('transform', null);

                //     d3.active(this)
                //         .attr('transform', 'translate(' + this.x(-1) + ',0)')
                // })
        }
    }
}
