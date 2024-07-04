export class Card
{
    c_type: CARD_TYPE;
    c_number: number;

    constructor(_c_type: CARD_TYPE, _c_number: number) {
        this.c_type = _c_type;
        this.c_number = _c_number;
    }
}

export enum CARD_TYPE {
    CLUB = 0,
    DIAMOND = 1,
    HEART = 2,
    SPADE = 3,
}