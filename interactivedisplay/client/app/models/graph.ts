import { Observable } from 'rxjs/Observable';
import { Subject } from 'rxjs/Subject';

export class Graph {
    public id: number = -1;

    public dimY: string = "";
    public dimX: string = "";

    public selectedDataIndices: number[] = [];
    public isSelected: boolean = false;

    public absolutePos: number = 0;
    public listIndex: number = 0;
    public width: number = 100;


    private dataSubscription: Subject<any> = new Subject<any>();
    public get onDataUpdate() : Observable<any> {
        return this.dataSubscription.asObservable();
    }

    public updateData() {
        this.dataSubscription.next({
            id: this.id,
            dimX: this.dimX,
            dimY: this.dimY,
            selectedData: this.selectedDataIndices,
            isSelected: this.isSelected
        });
    }


    public positionSubscription: Subject<any> = new Subject<any>();
    public get onPositionUpdate(): Observable<any> {
        return this.positionSubscription.asObservable();
    }

    public updatePosition() {
        this.positionSubscription.next({
            id: this.id,
            pos: this.absolutePos,
            index: this.listIndex
        });
    } 


    public toJson(): any {
        return {
            id: this.id,
            
            dimX: this.dimX,
            dimY: this.dimY,
            selectedData: this.selectedDataIndices,
            isSelected: this.isSelected,

            pos: this.absolutePos,
            index: this.listIndex
        }
    }
}
