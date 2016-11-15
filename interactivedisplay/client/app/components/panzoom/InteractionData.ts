export class InteractionData {
    public startPosX: number;
    public startPosY: number;

    public prevPosX: number;
    public prevPosY: number;

    public currPosX: number;
    public currPosY: number;

    public constructor(posX: number, posY: number) {
        this.startPosX = posX;
        this.startPosY = posY;

        this.prevPosX = posX;
        this.prevPosY = posY;

        this.currPosX = posX;
        this.currPosY = posY;
    }

    public setPos(x: number, y: number) {
        this.prevPosX = this.currPosX;
        this.prevPosY = this.currPosY;

        this.currPosX = x;
        this.currPosY = y;
    }
}