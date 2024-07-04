import { CARD_HAND_TYPE } from "../game/Round_Handler";
import { Card } from "../helpers/Card";

export class ScoreGenerator
{
    Evaluate_Cards(player_cards: Card[]): [number, CARD_HAND_TYPE] {
        var player_hand_type: CARD_HAND_TYPE = CARD_HAND_TYPE.none;
        var player_score: number = this.Get_Cards_Score(player_cards);

        if (this.IsTrail(player_cards)) {
            player_hand_type = CARD_HAND_TYPE.TRAIL;
            player_score = player_score * 10000;
        }
        else if (this.IsSequence(player_cards)) {
            if (this.IsFlush(player_cards)) {
                player_hand_type = CARD_HAND_TYPE.PURE_SEQUENCE;
                player_score = player_score * (1000 + 100);
            }
            else {
                player_hand_type = CARD_HAND_TYPE.SEQUENCE;
                player_score = player_score * 1000;
            }
        }
        else if (this.IsFlush(player_cards)) {
            player_hand_type = CARD_HAND_TYPE.FLUSH;
            player_score = player_score * 100;
        }
        else if (this.IsPair(player_cards)) {
            player_hand_type = CARD_HAND_TYPE.PAIR;
            player_score = player_score * 10;
        }
        else {
            player_hand_type = CARD_HAND_TYPE.HIGHCARD;
            player_score = this.Get_HighCards_Score(player_cards);
        }

        return [player_score, player_hand_type];
    }



    private IsTrail(cards: Card[]): boolean {
        var arr = cards.filter(x => x.c_number == cards[0].c_number);
        return arr.length == 3;
    }
    private IsSequence(cards: Card[]): boolean {
        var sorted_cards = cards.map(obj => ({ ...obj })).sort((n1, n2) => n1.c_number - n2.c_number);

        var sequence = true;
        for (var i = 1; i < sorted_cards.length; i++) {
            if ((sorted_cards[i].c_number - sorted_cards[i - 1].c_number) != 1) {
                sequence = false;
                break;
            }
        }
        if (!sequence && sorted_cards[sorted_cards.length - 1].c_number == 14) {
            sorted_cards[sorted_cards.length - 1].c_number = 1;
            sequence = this.IsSequence(sorted_cards);
        }

        return sequence;
    }
    private IsFlush(cards: Card[]): boolean {
        var arr = cards.filter(x => x.c_type == cards[0].c_type);
        return arr.length == 3;
    }
    private IsPair(cards: Card[]): boolean {
        var check1 = cards.filter(x => x.c_number == cards[0].c_number);
        var check2 = cards.filter(x => x.c_number == cards[1].c_number);

        var pair = (check1.length == 2 || check2.length == 2);
        return pair;
    }


    private Get_Cards_Score(cards: Card[]): number {
        var score: number = 0;
        cards.forEach(card => {
            score += card.c_number;
        });

        return score;
    }
    private Get_HighCards_Score(cards: Card[]): number {
        var sorted_cards = cards.map(obj => ({ ...obj })).sort((n1, n2) => n1.c_number - n2.c_number);

        var score: number = 0;
        score += sorted_cards[2].c_number * 100;
        score += sorted_cards[1].c_number * 10;
        score += sorted_cards[0].c_number;

        return score / 1000;
    }
}