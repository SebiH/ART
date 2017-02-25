import { Observable } from 'rxjs/Observable';
import { Subject } from 'rxjs/Subject';
import { Point } from './point';

export type PathElement = [number, number];

export class Graph {
    public static get SelectedWidth(): number {
        return window.innerWidth * 0.9;
    }

    public id: number = -1;

    public dimY: string = "";
    public dimX: string = "";

    public color: string = "#FFFFFF";

    public selectionPolygons: PathElement[][] = [];
    public selectedDataIndices: number[] = [];
    public isSelected: boolean = false;

    public absolutePos: number = 0;
    public listIndex: number = 0;
    public width: number = 1250;

    public posOffset: number = 0;
    public isPickedUp: boolean = false;
    public isNewlyCreated: boolean = false;

    private dataSubscription: Subject<any> = new Subject<any>();
    public get onDataUpdate() : Observable<any> {
        return this.dataSubscription.asObservable();
    }

    public updateData(changes: string[]) {
        this.dataSubscription.next({
            changes: changes,
            data: {
                id: this.id,
                color: this.color,
                dimX: this.dimX,
                dimY: this.dimY,
                isSelected: this.isSelected,
                isNewlyCreated: this.isNewlyCreated,
                hasFilter: this.selectedDataIndices !== null && this.selectedDataIndices.length > 0
            }
        });
    }


    private positionSubscription: Subject<any> = new Subject<any>();
    public get onPositionUpdate(): Observable<any> {
        return this.positionSubscription.asObservable();
    }

    public updatePosition() {
        this.positionSubscription.next({
            id: this.id,
            pos: this.absolutePos,
            width: this.width
        });
    } 


    public toJson(): any {
        return {
            id: this.id,
            
            dimX: this.dimX,
            dimY: this.dimY,
            color: this.color,
            isSelected: this.isSelected,
            isNewlyCreated: this.isNewlyCreated,

            pos: this.absolutePos,
            width: this.width
        }
    }


    // inverse of .toJson()
    public static fromJson(jGraph: any): Graph {
        let graph = new Graph();
        graph.id = jGraph.id;

        graph.dimX = jGraph.dimX;
        graph.dimY = jGraph.dimY;
        graph.color = jGraph.color;
        graph.isSelected = jGraph.isSelected;

        graph.absolutePos = jGraph.pos;
        graph.width = jGraph.width;

        return graph;
    }
}
