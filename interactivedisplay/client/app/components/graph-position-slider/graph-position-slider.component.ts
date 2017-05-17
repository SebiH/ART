import { Component, Input, OnInit, OnDestroy, ElementRef } from '@angular/core';
import { Surface } from '../../models/index';
import { SurfaceProvider } from '../../services/index';
import { ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';

@Component({
    selector: 'graph-position-slider',
    template: `
<div class="slider">
    <div class="bar"
         [ngStyle]="getBarStyle()"
         [ngClass]="{ touched: isTouched }"
         moveable
         (moveStart)="touchStart($event)"
         (moveUpdate)="touchUpdate($event)"
         (moveEnd)="touchEnd($event)">
    </div>
</div>`,
    styles: [
        '.slider { width: 100%; height: 100%; position: relative; }',
        `.bar {
            width: 100%;
            height: 75px;
            position: absolute;
            left: 0;

            border-top: 1px solid black;
            border-bottom: 1px solid black;

            background: #607D8B;
            transition: background 0.5s linear;
        }`,
        '.bar.touched { background: #8BC34A; }'
    ],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class GraphPositionSliderComponent implements OnInit, OnDestroy {
    private surface: Surface = null;
    private isActive: boolean = true;

    private isTouched: boolean = false;

    private totalHeight: number;
    private barHeight: number = 75;

    constructor(
        private surfaceProvider: SurfaceProvider,
        private elementRef: ElementRef,
        private changeDetector: ChangeDetectorRef) {
    }

    ngOnInit() {
        this.totalHeight = this.elementRef.nativeElement.children[0].clientHeight;
        this.surface = this.surfaceProvider.getSurface();
        this.surface.onUpdate
            .takeWhile(() => this.isActive)
            .subscribe(() => this.changeDetector.markForCheck());
    }

    ngOnDestroy() {
        this.isActive = false;
    }

    private getBarStyle(): any {
        return {
            top: this.offsetToPixel(this.surface.offset) + 'px'
        };
    }

    private touchStart(event): void {
        this.isTouched = true;
    }

    private touchUpdate(event): void {
        let isInsideBounds = 0 <= event.relativePos.y && event.relativePos.y <= 75;
        let isAtLimit = this.offsetToPixel(this.surface.offset) <= 0
                        || this.offsetToPixel(this.surface.offset) >= this.totalHeight - this.barHeight;

        if (!isAtLimit || isInsideBounds) {
            let newOffset = this.offsetToPixel(this.surface.offset) - event.deltaY;
            this.surface.offset = this.pixelToOffset(Math.min(Math.max(0, newOffset), this.totalHeight - this.barHeight));
            this.surfaceProvider.sync();

        }
    }

    private touchEnd(event): void {
        this.isTouched = false;
    }


    private pixelToOffset(pixel: number): number {
        return 1 - (pixel / (this.totalHeight - this.barHeight));
    }

    private offsetToPixel(offset: number): number {
        return (1 - offset) * (this.totalHeight - this.barHeight);
    }
}
