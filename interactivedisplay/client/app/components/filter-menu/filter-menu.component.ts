import { Component, Input } from '@angular/core';
import { ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { FilterProvider } from '../../services/index';
import { Filter, ChartDimension } from '../../models/index';

@Component({
    selector: 'filter-menu',
    template: ``,
    styles: [],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class FilterMenuComponent {
    @Input() filter: Filter;
}
