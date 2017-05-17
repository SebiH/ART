export const Red = '#E53935';
export const Green = '#43A047';
export const LightGreen = '#8BC34A';
export const DeepGreen = '#2E7D32';
export const Blue = '#1E88E5';
export const Yellow = '#FDD835';
export const Purple = '#9C27B0';
export const Orange = '#F4511E';
export const Cyan = '#00BCD4';
export const Pink = '#E91E63';
export const White = '#FFFFFF';
export const Black = '#000000';


let colors: string[] = [Red, Green, Blue, Yellow, Purple, Orange, Cyan];
let rndCounter = 0;
export function random(): string {
    return colors[rndCounter++ % colors.length];
}
