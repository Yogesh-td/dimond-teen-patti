using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace TeenPatti.Helpers
{
    public class GifPlayer : MonoBehaviour
    {
        [SerializeField] Image img_source;

        GIF_IMAGE gif_data;
        int gif_current_frame;
        Action animation_end_callback;

        public void Apply_Gif(GIF_IMAGE data)
        {
            gif_data = data;
            gif_current_frame = 0;

            StartCoroutine(nameof(Playing));
        }
        public void Apply_Gif(GIF_IMAGE data, Action _animation_end_callback)
        {
            gif_data = data;
            gif_current_frame = 0;
            animation_end_callback = _animation_end_callback;

            StartCoroutine(nameof(PlayOnce));
        }
        public void Play()
        {
            StopCoroutine(nameof(Playing));
            StartCoroutine(nameof(Playing));
        }


        IEnumerator Playing()
        {
            float waitTime = 1f / gif_data.fps;
            while (true)
            {
                img_source.sprite = gif_data.all_sprites[gif_current_frame];
                yield return new WaitForSeconds(waitTime);

                gif_current_frame++;
                if (gif_current_frame >= gif_data.all_sprites.Length)
                    gif_current_frame = 0;
            }
        }
        IEnumerator PlayOnce()
        {
            float waitTime = 1f / gif_data.fps;
            while (true)
            {
                img_source.sprite = gif_data.all_sprites[gif_current_frame];
                yield return new WaitForSeconds(waitTime);

                gif_current_frame++;
                if (gif_current_frame >= gif_data.all_sprites.Length)
                    break;
            }

            animation_end_callback?.Invoke();
        }
    }
}