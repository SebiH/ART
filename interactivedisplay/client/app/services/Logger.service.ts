import {Injectable} from '@angular/core';

@Injectable()
export class Logger {

    constructor() { }

    log(msg: any) {
        console.log('[LOG] ' + JSON.stringify(msg));
    }

    debug(msg: any) {
        console.debug('[DEBUG] ' + JSON.stringify(msg));
    }

    error(msg: any) {
        console.error('[ERROR] ' + JSON.stringify(msg));
    }

    warn(msg: any) {
        console.warn('[WARNING] ' + JSON.stringify(msg));
    }
}
