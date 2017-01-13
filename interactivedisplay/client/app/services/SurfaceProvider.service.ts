import { Injectable } from '@angular/core';
import { Surface } from '../models/index';
import { SocketIO } from './SocketIO.service';

import * as _ from 'lodash';

@Injectable()
export class SurfaceProvider {

    private surface: Surface = new Surface();

    public constructor(private socketio: SocketIO) {
        this.sync();
    }

    public getSurface(): Surface {
        return this.surface;
    }

    public setSize(width: number, height: number): void {
        this.surface.width = width;
        this.surface.height = height;
        this.sync();
    }

    public setPixelCmRatio(ratio: number) {
        this.surface.pixelToCmRatio = ratio;
        this.sync();
    }

    public sync(): void {
        this.socketio.sendMessage('surface', this.surface.toJson());
    }
}
