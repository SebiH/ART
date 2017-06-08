import { Component, OnInit, OnDestroy, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { DataProvider, GlobalFilterProvider } from '../../services/index';
import { ChartDimension } from '../../models/index';

import * as _ from 'lodash';

@Component({
    selector: 'admin-data',
    templateUrl: './app/components/admin-data/admin-data.html',
    styleUrls: ['./app/components/admin-data/admin-data.css'],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class AdminDataComponent implements OnInit, OnDestroy {

    private dimensions: string[] = [];
    private data: any[] = [];
    private selectedData: any[] = [];
    private viewIndex: number = -1;
    private selectedIndex: number = -1;

    private isActive: boolean = true;

    constructor(
        private dataProvider: DataProvider,
        private globalFilterProvider: GlobalFilterProvider,
        private changeDetector: ChangeDetectorRef) { }

    ngOnInit() {
        this.dataProvider.getDimensionNames()
            .first()
            .subscribe((dims) => {
                for (let dim of dims) {
                    this.dimensions.push(dim);
                    this.initDimension(dim);
                }
                this.changeDetector.detectChanges();
                this.globalFilterProvider.adminUpdateGlobalFilter();
            });

        this.globalFilterProvider.onUpdate()
            .takeWhile(() => this.isActive)
            .subscribe((filter) => {
                this.selectedData = [];
                for (let fd of filter) {
                    if (+fd.id < 100) {
                        if (fd.f == 0) {
                            this.data[fd.id].color = fd.c;
                            this.selectedData.push(this.data[fd.id]);
                        }
                    }
                }
                this.changeDetector.detectChanges();
            });
    }

    ngOnDestroy() {
        this.isActive = false;
    }

    private initDimension(dim: string): void {
        this.dataProvider.getData(dim)
            .first()
            .subscribe((chartDim) => {
                if (this.data.length == 0) {
                    for (let data of chartDim.data) {
                        this.data.push({
                            id: data.id
                        });
                    }
                }

                for (let data of chartDim.data) {
                    if (+data.id < 100) {
                        this.data[data.id][dim] = {
                            value: data.value,
                            name: this.getName(chartDim, data.value)
                        };
                    }
                }
                this.changeDetector.detectChanges();
            });
    }

    private getName(dim: ChartDimension, val: number): string {
        if (!dim.isMetric) {
            let mapping = _.find(dim.mappings, m => m.value == val);
            if (mapping) {
                return mapping.name;
            } else {
                return null;
            }
        } else if (dim.isTimeBased) {
            return this.formatDate(new Date(val * 1000), dim.timeFormat, true);
        } else {
            return null;
        }
    }

    private viewData(index: number): void {
        this.viewIndex = index;
        this.changeDetector.detectChanges();
    }

    private selectData(index: number): void {
        if (this.selectedIndex == index || index < 0) {
            this.selectedIndex = -1;
            this.globalFilterProvider.adminUpdateGlobalFilter();
        } else {
            this.selectedIndex = index;
            this.globalFilterProvider.adminFilterHack(this.selectedIndex);
        }

        this.changeDetector.detectChanges();
    }





    private ii(i, len?) {
        let s = i + "";
        len = len || 2;
        while (s.length < len) s = "0" + s;
        return s;
    }

    // adapted from http://stackoverflow.com/a/14638191/4090817
    private formatDate(date, format, utc): string {
        let MMMM = ["\x00", "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December"];
        let MMM = ["\x01", "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"];
        let dddd = ["\x02", "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday"];
        let ddd = ["\x03", "Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat"];


        let y = utc ? date.getUTCFullYear() : date.getFullYear();
        format = format.replace(/(^|[^\\])yyyy+/g, "$1" + y);
        format = format.replace(/(^|[^\\])yy/g, "$1" + y.toString().substr(2, 2));
        format = format.replace(/(^|[^\\])y/g, "$1" + y);

        let M = (utc ? date.getUTCMonth() : date.getMonth()) + 1;
        format = format.replace(/(^|[^\\])MMMM+/g, "$1" + MMMM[0]);
        format = format.replace(/(^|[^\\])MMM/g, "$1" + MMM[0]);
        format = format.replace(/(^|[^\\])MM/g, "$1" + this.ii(M));
        format = format.replace(/(^|[^\\])M/g, "$1" + M);

        let d = utc ? date.getUTCDate() : date.getDate();
        format = format.replace(/(^|[^\\])dddd+/g, "$1" + dddd[0]);
        format = format.replace(/(^|[^\\])ddd/g, "$1" + ddd[0]);
        format = format.replace(/(^|[^\\])dd/g, "$1" + this.ii(d));
        format = format.replace(/(^|[^\\])d/g, "$1" + d);

        let H = utc ? date.getUTCHours() : date.getHours();
        format = format.replace(/(^|[^\\])HH+/g, "$1" + this.ii(H));
        format = format.replace(/(^|[^\\])H/g, "$1" + H);

        let h = H > 12 ? H - 12 : H == 0 ? 12 : H;
        format = format.replace(/(^|[^\\])hh+/g, "$1" + this.ii(h));
        format = format.replace(/(^|[^\\])h/g, "$1" + h);

        let m = utc ? date.getUTCMinutes() : date.getMinutes();
        format = format.replace(/(^|[^\\])mm+/g, "$1" + this.ii(m));
        format = format.replace(/(^|[^\\])m/g, "$1" + m);

        let s = utc ? date.getUTCSeconds() : date.getSeconds();
        format = format.replace(/(^|[^\\])ss+/g, "$1" + this.ii(s));
        format = format.replace(/(^|[^\\])s/g, "$1" + s);

        let f = utc ? date.getUTCMilliseconds() : date.getMilliseconds();
        format = format.replace(/(^|[^\\])fff+/g, "$1" + this.ii(f, 3));
        f = Math.round(f / 10);
        format = format.replace(/(^|[^\\])ff/g, "$1" + this.ii(f));
        f = Math.round(f / 10);
        format = format.replace(/(^|[^\\])f/g, "$1" + f);

        let T = H < 12 ? "AM" : "PM";
        format = format.replace(/(^|[^\\])TT+/g, "$1" + T);
        format = format.replace(/(^|[^\\])T/g, "$1" + T.charAt(0));

        let t = T.toLowerCase();
        format = format.replace(/(^|[^\\])tt+/g, "$1" + t);
        format = format.replace(/(^|[^\\])t/g, "$1" + t.charAt(0));

        let tz = -date.getTimezoneOffset();
        let K = utc || !tz ? "Z" : tz > 0 ? "+" : "-";
        if (!utc) {
            tz = Math.abs(tz);
            let tzHrs = Math.floor(tz / 60);
            let tzMin = tz % 60;
            K += this.ii(tzHrs) + ":" + this.ii(tzMin);
        }
        format = format.replace(/(^|[^\\])K/g, "$1" + K);

        let day = (utc ? date.getUTCDay() : date.getDay()) + 1;
        format = format.replace(new RegExp(dddd[0], "g"), dddd[day]);
        format = format.replace(new RegExp(ddd[0], "g"), ddd[day]);

        format = format.replace(new RegExp(MMMM[0], "g"), MMMM[M]);
        format = format.replace(new RegExp(MMM[0], "g"), MMM[M]);

        format = format.replace(/\\(.)/g, "$1");

        return format;
    };


}
