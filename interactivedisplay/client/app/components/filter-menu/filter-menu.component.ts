import { Component, Input } from '@angular/core';
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
export class FilterMenuComponent {
    @Input() filter: DetailFilter;

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
