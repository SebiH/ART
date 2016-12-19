import { Component, Input, OnInit, OnDestroy, ElementRef } from '@angular/core';
import { Observable, Subscription } from 'rxjs/Rx';
import { Marker, Graph, Point } from '../../models/index';
import { MarkerProvider, SocketIO } from '../../services/index';

@Component({
  selector: 'graph-section',
  templateUrl: './app/components/graph-section/graph-section.html',
  styleUrls: ['./app/components/graph-section/graph-section.css'],
})
export class GraphSectionComponent implements OnInit, OnDestroy {

  @Input() private graph: Graph;
  private marker: Marker;
  private prevPosition: Point;
  private timerSubscription: Subscription;
  private socketioListener;

  constructor (
    private markerProvider: MarkerProvider,
    private socketio: SocketIO,
    private elementRef: ElementRef
    ) {}

  ngOnInit() {
    this.marker = this.markerProvider.createMarker();

    let timer = Observable.timer(0, 100);
    this.timerSubscription = timer.subscribe(this.checkForChanges.bind(this));

    this.socketioListener =  () => { this.sendPosition(this.getSectionPosition()); };
    this.socketio.on('get-plane', this.socketioListener);
  }

  ngOnDestroy() {
    this.markerProvider.destroyMarker(this.marker);
    this.timerSubscription.unsubscribe();
    this.socketio.off('get-plane', this.socketioListener);
  }

  private checkForChanges() {
    let currentPosition = this.getSectionPosition();

    if (this.prevPosition == undefined || !this.prevPosition.equalTo(currentPosition)) {
      this.sendPosition(currentPosition);
      this.prevPosition = currentPosition;
    }
  }

  private getSectionPosition(): Point {
    let element = <HTMLElement>this.elementRef.nativeElement;
    let pos = element.getBoundingClientRect();

    return new Point(pos.left, pos.top);
  }

  private sendPosition(pos: Point) {
    this.socketio.sendMessage('plane-position', {
      id: this.graph.id,
      pos: pos.x
    });
  }
}
