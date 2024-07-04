using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TeenPatti.Gameplay.Effects
{
    public class Sideshow_Effect : MonoBehaviour
    {
        [SerializeField] LineRenderer line_renderer;

        [Space]
        [SerializeField] List<Vector3> line_points;
        [SerializeField] int max_points;
        [SerializeField] float delay;
        [SerializeField][Range(0, 10)] float noise;


        private void OnDisable() => Cancel_Electic();
        private void OnDestroy() => Cancel_Electic();


        public void Init_Electric(Vector3 init_point, Vector3 end_point)
        {
            line_points = new List<Vector3>();
            line_points.Add(init_point);
            for (int i = 1; i < max_points - 1; i++)
                line_points.Add(Vector3.Lerp(init_point, end_point, (float)i / (float)max_points));
            line_points.Add(end_point);

            line_renderer.positionCount = max_points;
            line_renderer.SetPositions(line_points.ToArray());

            this.gameObject.SetActive(true);
            StartCoroutine(nameof(Update_Noise));
        }
        public void Cancel_Electic()
        {
            StopCoroutine(nameof(Update_Noise));
            this.gameObject.SetActive(false);
        }



        IEnumerator Update_Noise()
        {
            yield return new WaitForSeconds(delay);
            line_renderer.SetPositions(Generate_Noise().ToArray());
            StartCoroutine(nameof(Update_Noise));
        }
        private List<Vector3> Generate_Noise()
        {
            List<Vector3> updated_points = new List<Vector3>();
            updated_points.Add(line_points[0]);
            for (int i = 1; i < line_points.Count - 1; i++)
            {
                updated_points.Add(new Vector3(
                    line_points[i].x + UnityEngine.Random.Range(-noise, noise),
                    line_points[i].y + UnityEngine.Random.Range(-noise, noise),
                    line_points[i].z
                    ));
            }
            updated_points.Add(line_points[line_points.Count - 1]);
            return updated_points;
        }
    }
}