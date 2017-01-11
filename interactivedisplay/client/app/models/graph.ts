import { Observable } from 'rxjs/Observable';
import { Subject } from 'rxjs/Subject';
import { Point } from './point';

export class Graph {
    public id: number = -1;

    public dimY: string = "";
    public dimX: string = "";

    public color: string = "#FFFFFF";

    public selectionPolygon: Point[] = [];
    public selectedDataIndices: number[] = [];
    public isSelected: boolean = false;

    public absolutePos: number = 0;
    public listIndex: number = 0;
    public width: number = 700;

    public posOffset: number = 0;
    public isPickedUp: boolean = false;

    private dataSubscription: Subject<any> = new Subject<any>();
    public get onDataUpdate() : Observable<any> {
        return this.dataSubscription.asObservable();
    }

    public updateData() {
        this.dataSubscription.next({
            id: this.id,
            color: this.color,
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
            color: this.color,
            selectedData: this.selectedDataIndices,
            isSelected: this.isSelected,

            pos: this.absolutePos,
            index: this.listIndex
        }
    }


    // inverse of .toJson()
    public static fromJson(jGraph: any): Graph {
        let graph = new Graph();
        graph.id = jGraph.id;

        graph.dimX = jGraph.dimX;
        graph.dimY = jGraph.dimY;
        graph.color = jGraph.color;
        graph.selectedDataIndices = jGraph.selectedData;
        graph.isSelected = jGraph.isSelected;

        graph.absolutePos = jGraph.pos;
        graph.listIndex = jGraph.index;

        return graph;
    }
}
