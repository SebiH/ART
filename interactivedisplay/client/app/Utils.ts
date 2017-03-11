export class Utils {
    public static padLeft(str: string, length: number): string {
        while (str.length < length) {
            str = '0' + str;
        }
        return str;
    }

    public static getBaseUrl() {
        return window.location.href.replace(/\!+/, '').replace(/\#+/, '');
    }
}
