import * as os from 'os';

export class Util {

    public static GetIp(): string {
        // see https://stackoverflow.com/a/8440736/4090817
        var address = "";
        var hasAddress = false;
        var ifaces = os.networkInterfaces();


        Object.keys(ifaces).forEach(function (ifname) {
            ifaces[ifname].forEach(function (iface) {
                if ('IPv4' !== iface.family || iface.internal !== false) {
                    // skip over internal (i.e. 127.0.0.1) and non-ipv4 addresses
                    return;
                }

                if (!hasAddress) {
                    hasAddress = true;
                    address = iface.address;
                }
            });
        });

        return address;
    }
}
