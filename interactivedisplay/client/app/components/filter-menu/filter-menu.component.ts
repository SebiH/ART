import { Component, Input, OnInit, OnDestroy } from '@angular/core';
import { ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { FilterProvider } from '../../services/index';
import { DetailFilter, ChartDimension } from '../../models/index';

const COL_RED = '#E53935';
const COL_GREEN = '#43A047';
const COL_BLUE = '#1E88E5';
const COL_YELLOW = '#FDD835';
const COL_PURPLE = '#9C27B0';
const COL_ORANGE = '#F4511E';
const COL_CYAN = '#00BCD4';


@Component({
    selector: 'filter-menu',
    templateUrl: './app/components/filter-menu/filter-menu.html',
    styleUrls: [ './app/components/filter-menu/filter-menu.css' ],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class FilterMenuComponent implements OnInit, OnDestroy {
    @Input() filter: DetailFilter;

    private isActive: boolean = true;
    private colors: string[] = [
        COL_RED,
        COL_GREEN,
        COL_BLUE,
        COL_YELLOW,
        COL_PURPLE,
        COL_ORANGE,
        COL_CYAN
    ];

    constructor(
        private filterProvider: FilterProvider,
        private changeDetector: ChangeDetectorRef) {
    }

    ngOnInit() {
        this.filter.origin.onUpdate
            .takeWhile(() => this.isActive)
            .filter(changes => changes.indexOf('isFlipped') >= 0)
            .subscribe(() => this.changeDetector.detectChanges());
    }

    ngOnDestroy() {
        this.isActive = false;
    }

    private getAxis(axis: 'x' | 'y'): 'x' | 'y' {
        if (this.filter.origin.isFlipped) {
            return axis == 'x' ? 'y' : 'x';
        } else {
            return axis;
        }
    }


    private applyColorByValue(axis: 'x' | 'y'): void {
        this.filter.useAxisColor = axis;
        this.changeDetector.detectChanges();
    }

    private applyColor(color: string): void {
        this.filter.color = color;
        this.filter.useAxisColor = 'n';
        this.changeDetector.detectChanges();
    }

    private deleteFilter(event): void {
        this.filterProvider.removeFilter(this.filter);
    }

    private deselectFilter(): void {
        this.filter.isSelected = false;
    }
}
