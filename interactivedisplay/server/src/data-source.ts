import { Observable } from 'rxjs';
import { RawData } from './raw-data';

export interface DataSource {
    getDimensions(): { name: string, phases: string[] }[];
    getData(): Observable<RawData[]>;
}
