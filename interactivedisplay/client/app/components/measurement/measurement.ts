import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { SurfaceProvider } from '../../services/index';
import { Surface } from '../../models/index';

@Component({
  selector: 'init-measurement',
  templateUrl: './app/components/measurement/measurement.html',
  styleUrls: ['./app/components/measurement/measurement.css'],
})
export class MeasurementComponent implements OnInit {
    private surface: Surface;

    constructor (private surfaceProvider: SurfaceProvider, private router: Router) {}

    ngOnInit() {
        this.surface = this.surfaceProvider.getSurface();
    }

    updateMeasurement(value: number): void {
        this.surfaceProvider.setPixelCmRatio(value);
    }

    navigateBack(): void {
        this.router.navigateByUrl('/menu');
    }
}
