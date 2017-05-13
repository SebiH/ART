import { Injectable } from '@angular/core';
import { Http } from '@angular/http';
import { Observable } from 'rxjs/Observable';
import { ReplaySubject } from 'rxjs/ReplaySubject';
import { SocketIO } from './SocketIO.service';

export class Settings {
    showMarkers: boolean = true;
    showOverviewChart: boolean = true;
    showMarkerOverlay: boolean = false;
};

@Injectable()
export class SettingsProvider {

    private optionSubject: ReplaySubject<Settings> = new ReplaySubject<Settings>(1);

    constructor(private http: Http, private socketio: SocketIO) {

        var initialSettings = new Settings();
        initialSettings.showMarkers = true;
        initialSettings.showOverviewChart = true;
        this.optionSubject.next(initialSettings);

        this.http.get('/api/settings')
            .subscribe(res => this.optionSubject.next(res.json() as Settings));
        this.socketio.on('settings', (s) => {
            this.optionSubject.next(JSON.parse(s) as Settings);
        });
    }

    public getCurrent(): Observable<Settings> {
        return this.optionSubject.asObservable();
    }

    public sync(settings: Settings): void {
        this.socketio.sendMessage('settings', settings);
    }
}
