using UnityEngine;

namespace TeenPatti.Avatar.Settings
{
    [CreateAssetMenu(fileName = "AvatarSettings", menuName = "Scriptables/AvatarSettings", order = 1)]
    public class AvatarSettings : ScriptableObject
    {
        [Header("Profile Picture Avatars")]
        [SerializeField] Sprite[] all_avatars;
        [SerializeField] Sprite null_avatar;


        public Sprite Get_Null_Avatar() => null_avatar;
        public Sprite Get_Avatar(int _index) => (_index < 0 || _index >= all_avatars.Length) ? all_avatars[0] :  all_avatars[_index];
        public Sprite[] Get_All_Avatars() => all_avatars;
    }
}