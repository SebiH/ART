import { Observable } from 'rxjs';
import { RawData } from './raw-data';

export interface DataSource {
    getDimensions(): string[];
    getData(): Observable<RawData[]>;
}
