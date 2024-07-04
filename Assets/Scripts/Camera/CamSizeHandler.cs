using UnityEngine;

namespace Headball.Helpers
{
    [ExecuteAlways]
    public class CamSizeHandler : MonoBehaviour
    {
        [SerializeField] Camera maincamera;
        [SerializeField] float sensitivity;
        [SerializeField] Vector2 size_clamper;

        void Update()
        {
            float unitsPerPixel = sensitivity / Screen.width;

            float desiredHalfHeight = 0.5f * unitsPerPixel * Screen.height;

            maincamera.orthographicSize = Mathf.Clamp(desiredHalfHeight, size_clamper.x, size_clamper.y);
            //GetComponent<Camera>().fieldOfView = 2 * Mathf.Atan(Mathf.Tan(fixedHorizontalFOV * Mathf.Deg2Rad * 0.5f) / GetComponent<Camera>().aspect) * Mathf.Rad2Deg;
        }
    }
}