using UnityEngine;

namespace TeenPatti.App.Settings
{
    [CreateAssetMenu(fileName = "DeckSettings", menuName = "Scriptables/DeckSettings", order = 1)]
    public class DeckSettings : ScriptableObject
    {
        [System.Serializable]
        public struct CARDS_COLLECTION
        {
            public int card_type_key;
            public Sprite[] cards_sprites;
        }

        [SerializeField] CARDS_COLLECTION[] cards_collection;
        [SerializeField] Sprite card_back;

        public Sprite Card_Back_Sprite => card_back;
        public Sprite Card_Joker_Sprite => cards_collection[0].cards_sprites[cards_collection[0].cards_sprites.Length - 1];

        public Sprite Get_Card_Sprite(int card_type, int card_number)
        {
            return System.Array.Find(cards_collection, x => x.card_type_key == card_type).cards_sprites[card_number - 2];
        }
    }
}