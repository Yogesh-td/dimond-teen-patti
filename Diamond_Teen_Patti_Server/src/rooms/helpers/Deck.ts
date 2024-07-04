import { CARD_TYPE, Card } from "./Card";

export class Deck
{
    cards: Card[] = [];

    constructor(shuffled: boolean)
    {
        Object.values(CARD_TYPE).filter(v => !isNaN(Number(v))).forEach((card_type) =>
        {
            for (var i = 2; i <= 14; i++)
            {
                this.cards.push(new Card(card_type as CARD_TYPE, i));
            }
        });

        if (shuffled)
            this.cards.sort(() => Math.random() - 0.5);
    }
}