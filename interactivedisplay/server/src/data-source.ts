import { Observable } from 'rxjs';
import { RawData } from './raw-data';

export interface DataSource {
    getDimensions(): { name: string, phase: string }[];
    getData(): Observable<RawData[]>;
}
