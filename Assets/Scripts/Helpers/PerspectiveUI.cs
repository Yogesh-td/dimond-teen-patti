using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Global.Helpers
{
    [RequireComponent(typeof(RectTransform))]
    public class PerspectiveUI : BaseMeshEffect
    {
        public bool from_centre;
        public Vector2 perspective_senstivity;
        public RectTransform rectTrans;

        List<UIVertex> verts = new List<UIVertex>();
        Vector3 bottom_centre;

        private void Calculate_Bottom_Centre()
        {
            Vector3 screenBottomCenter = new Vector3(
                        rectTrans.position.x,
                        rectTrans.position.y - rectTrans.rect.height / 2,
                        rectTrans.position.z
                    );

            bottom_centre = Camera.main.ScreenToWorldPoint(screenBottomCenter);
        }
        private Vector3 Calculate_Vertex_Postion(Vector3 pos)
        {
            Vector3 uiVertexPosition;
            RectTransformUtility.ScreenPointToWorldPointInRectangle(rectTrans, pos, Camera.main, out uiVertexPosition);

            return uiVertexPosition;
        }
        public override void ModifyMesh(VertexHelper vh)
        {
            if (!IsActive())
                return;

            if (rectTrans == null)
                rectTrans = GetComponent<RectTransform>();

            Calculate_Bottom_Centre();
            vh.GetUIVertexStream(verts);

            for (int index = 0; index < verts.Count; index++)
            {
                var uiVertex = verts[index];
                var uiVertexPosition = Calculate_Vertex_Postion(uiVertex.position);

                Vector2 distance = new Vector2(uiVertexPosition.x - bottom_centre.x, uiVertexPosition.y - bottom_centre.y);
                if (from_centre)
                {
                    if (distance.x > 0)
                        uiVertex.position.x += Vector2.Distance(uiVertex.position, bottom_centre) * distance.y * perspective_senstivity.x;
                    else
                        uiVertex.position.x -= Vector2.Distance(uiVertex.position, bottom_centre) * distance.y * perspective_senstivity.x;

                    uiVertex.position.y -= distance.y * perspective_senstivity.y;
                }
                else
                    uiVertex.position.x += distance.y * perspective_senstivity.x;

                verts[index] = uiVertex;
            }

            vh.Clear();
            vh.AddUIVertexTriangleStream(verts);
        }
    }
}