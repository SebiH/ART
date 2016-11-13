import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { SocketIO } from '../../services/index';

@Component({
  selector: 'init-measurement',
  templateUrl: './app/components/measurement/measurement.html',
  styleUrls: ['./app/components/measurement/measurement.css'],
})
export class MeasurementComponent {
    pixelCmRatio: number = 1;

    constructor (private socketio: SocketIO, private router: Router) {}

    sendMeasurement(): void {
        this.socketio.sendMessage('pixelCmRatio', this.pixelCmRatio);
        this.router.navigateByUrl('/menu');
    }
}
