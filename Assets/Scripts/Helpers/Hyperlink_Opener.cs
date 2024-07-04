using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;

namespace Global.Helpers
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class Hyperlink_Opener : MonoBehaviour, IPointerClickHandler
    {
        TMP_Text m_textMeshPro;

        void Start()
        {
            m_textMeshPro = GetComponent<TMP_Text>();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            int linkIndex = TMP_TextUtilities.FindIntersectingLink(m_textMeshPro, Camera.main.ScreenToWorldPoint(eventData.position), null);
            if (linkIndex == -1)
                return;

            TMP_LinkInfo linkInfo = m_textMeshPro.textInfo.linkInfo[linkIndex];
            Application.OpenURL(linkInfo.GetLinkID());
        }
    }
}