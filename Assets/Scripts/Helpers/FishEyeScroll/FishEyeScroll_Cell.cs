
namespace UnityEngine.UI.Extensions
{
    public class FishEyeScroll_Cell : MonoBehaviour
    {
        [SerializeField] float centralized_position_X;

        public Vector2 Get_Size() => GetComponent<RectTransform>().rect.size;
        public float Get_Centralized_Position_X() => centralized_position_X;
        public void Update_Centralized_Position(float value) => centralized_position_X = value;
        public void Update_Scale(float value) => transform.localScale = Vector2.one * Mathf.Abs(value);
    }
}