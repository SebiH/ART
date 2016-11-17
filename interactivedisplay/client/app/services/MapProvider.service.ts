import { Injectable, HostListener } from '@angular/core';
import { SocketIO } from './SocketIO.service';
import { Map } from '../models/index';

/**
 *    TODO: 
 *        - Get map arrangement from unity (low priority)
 */

@Injectable()
export class MapProvider {
    private maps: Map[] = [];

    constructor(private socketio: SocketIO) {
        this.socketio.on('get-maps', () => {
            console.log('get-maps');
            for (var map of this.maps) {
                this.syncMap(map);
            }
        });
    }

    public initMaps() {
        while (this.maps.length > 0) {
            let map = this.maps.pop();
            map.offPropertyChanged((map) => this.syncMap(map));
        }

        let mapSizeX = 300;
        let mapSizeY = mapSizeX * 0.73913043478; // current aspect ratio
        let borderSize = 10;
        let currentId = 0;
        let mapCountHor = 3;
        let mapCountVer = 2;

        let width = window.innerWidth - borderSize * 2;
        let height = window.innerHeight - borderSize * 2;

        for (let i = 0; i < mapCountHor; i++) {
            for (let j = 0; j < mapCountVer; j++) {
                for (let k = 0; k < 4; k++) {
                    let map = new Map();
                    map.id = currentId++;
                    map.posX = Math.max(i / (mapCountHor - 1) * width - mapSizeX/2, 0) + (k % 2) * (mapSizeX/4 + borderSize/2) + borderSize / 4 ;
                    map.posY = Math.max(j / (mapCountVer - 1) * height - mapSizeY/2, 0) + Math.floor(k / 2) * (mapSizeY/4 + borderSize/2) + borderSize/4;
                    map.sizeX = mapSizeX / 4 - borderSize;
                    map.onPropertyChanged((map) => this.syncMap(map));
                    this.maps.push(map);
                }
            }
        }
    }

    private syncMap(map: Map) {
        this.socketio.sendMessage('map', map.toJson());
    }

    public getMaps(): Map[] {
        return this.maps;
    }
}

