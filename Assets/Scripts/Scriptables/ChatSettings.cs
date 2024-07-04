using System.Collections;
using TeenPatti.Helpers;
using UnityEngine;

namespace TeenPatti.Chats
{
    [CreateAssetMenu(fileName = "ChatSettings", menuName = "Scriptables/ChatSettings", order = 1)]
    public class ChatSettings : ScriptableObject
    {
        [Header("Chat Fast Messages")]
        [SerializeField] string[] chat_messages;
        [SerializeField] int chat_maxlength;

        [Header("Gif Messages")]
        [SerializeField] GIF_IMAGE[] gift_gifs;

        public string[] Chat_Messages => chat_messages;
        public int Chat_MaxLength => chat_maxlength;
        public GIF_IMAGE[] Gift_Gifs => gift_gifs;
    }
}