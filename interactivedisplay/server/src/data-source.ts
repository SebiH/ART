import { Observable } from 'rxjs';
import { RawData } from './raw-data';

export interface DataSource {
    getDimensions(): { name: string, displayName: string, phases: string[] }[];
    getData(): Observable<RawData[]>;
}
