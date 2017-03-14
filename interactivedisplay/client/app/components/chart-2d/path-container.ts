import { HtmlChartElement, ChartElement } from '../../directives/index';
import { PathSelection } from './path-selection';

export class PathContainer extends ChartElement {

    private container: HtmlChartElement;
    private width: number;
    private height: number;

    public register(root: HtmlChartElement, width: number, height: number): void {
        this.container = root.append('g');
        this.width = width;
        this.height = height;
    }

    public unregister(): void {
        this.container.remove();
    }

    public resize(width: number, height: number): void {
        // TODO
    }

    public addPath(path: PathSelection): void {
        path.register(this.container, this.width, this.height);
    }

    public removePath(path: PathSelection): void {
        path.unregister();
    }
}
