import { Injectable } from '@angular/core';
import { ReplaySubject } from 'rxjs/ReplaySubject';
import { Observable } from 'rxjs/Observable';

import { DataProvider } from './DataProvider.service';
import { FilterProvider } from './FilterProvider.service';
import { GraphProvider } from './GraphProvider.service';
import { GlobalFilterProvider } from './GlobalFilterProvider.service';

@Injectable()
export class ServiceLoader {

    private loadedSubject: ReplaySubject<void> = new ReplaySubject<void>();

    private dimensionDataLoaded = false;
    private graphLoaded = false;
    private filterLoaded = false;

    constructor(
        dataProvider: DataProvider,
        filterProvider: FilterProvider,
        graphProvider: GraphProvider,
        globalFilterProvider: GlobalFilterProvider
        ) {

        filterProvider.getFilters()
            .first()
            .subscribe(() => {
                this.filterLoaded = true;
                this.checkCompletion();
            })

        dataProvider.getDimensions()
            .first()
            .subscribe((dims) => {
                let dimCounter = 0;

                for (let dim of dims) {
                    dataProvider.getData(dim)
                        .first()
                        .subscribe(() => {
                            dimCounter++;
                            if (dimCounter >= dims.length) {
                                this.dimensionDataLoaded = true;
                                this.checkCompletion();
                            }
                        })
                }
            });

        graphProvider.getGraphs()
            .first()
            .subscribe(() => {
                this.graphLoaded = true;
                this.checkCompletion();
            });
    }


    public onLoaded(): Observable<void> {
        return this.loadedSubject.asObservable();
    }

    private checkCompletion(): void {
        if (this.dimensionDataLoaded && this.graphLoaded && this.filterLoaded) {
            this.loadedSubject.next(null);
        }
    }
}
