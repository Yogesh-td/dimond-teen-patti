using Global.Helpers;
using System;
using TeenPatti.App.Settings;
using UnityEngine;
using UnityEngine.Audio;

namespace TeenPatti.Audios
{
    public class AudioManager : Singleton<AudioManager>
    {
        [System.Serializable]
        public class MUSIC_DATA
        {
            public MUSICS music_type;
            public AudioClip music_clip;
        }
        [System.Serializable]
        public class SOUND_DATA
        {
            public SOUNDS sound_type;
            public AudioClip sound_clip;
        }

        [Header("Audio Controllers")]
        [SerializeField] AudioMixer audio_mixer;

        [Header("Music Settings")]
        [SerializeField] AudioSource music_source;
        [SerializeField] MUSIC_DATA[] music_data;

        [Header("Sound Settings")]
        [SerializeField] AudioSource sound_source;
        [SerializeField] SOUND_DATA[] sound_data;


        private void Start()
        {
            Update_Music_Volume(CoreSettings.Instance.Music ? 0 : -80);
        }


        #region Music Methods
        private MUSIC_DATA Get_MusicClip(MUSICS music_type)
        {
            return Array.Find(music_data, x => x.music_type == music_type);
        }
        public void Play_Music(MUSICS music_type)
        {
            MUSIC_DATA music_handler = Get_MusicClip(music_type);
            if (music_handler == null)
                return;

            float delay = 1f;
            iTween.AudioTo(music_source.gameObject, 0, 1, delay);
            Timer.Schedule(this, delay, () => 
            {
                music_source.clip = music_handler.music_clip;
                music_source.Play();

                iTween.AudioTo(music_source.gameObject, 0.5f, 1, delay);
            });
        }
        public void Set_Music_Status(bool value)
        {
            float delay = 1f;
            float currentvolume = 0;

            audio_mixer.GetFloat("music_volume", out currentvolume);
            iTween.ValueTo(music_source.gameObject, iTween.Hash
            (
                "from", currentvolume,
                "to", value ? 0 : -80f,
                "time", delay,
                "onupdatetarget", this.gameObject,
                "onupdate", nameof(Update_Music_Volume)
            ));
        }
        private void Update_Music_Volume(float value)
        {
            audio_mixer.SetFloat("music_volume", value);
        }
        #endregion


        #region Sound Methods
        private SOUND_DATA Get_SoundClip(SOUNDS sound_type)
        {
            return Array.Find(sound_data, x => x.sound_type == sound_type);
        }
        public void Play_Sound(SOUNDS sound_type)
        {
            SOUND_DATA sound_handler = Get_SoundClip(sound_type);
            if (sound_handler == null)
                return;

            sound_source.PlayOneShot(sound_handler.sound_clip);
        }
        public void Set_Sound_Status(bool value)
        {
            audio_mixer.SetFloat("sound_volume", value ? 0 : -80);
        }
        #endregion
    }

    public enum MUSICS
    {
        BACKGROUND_IDLE,
        BACKGROUND_GAMEPLAY
    }
    public enum SOUNDS
    {
        BUTTON_TAP,
        BUTTON_CLOSE,
        SLIDER_CHANGE,
        PACK,
        SHOW,
        ADD_BET,
        SUBSTRACT_BET,
        BET,
        TURN,
        CARD_RECEIVE,
        BOOT_COLLECTING,
        RECEIVED_CHAT,
        CARD_FLIP,
        WIN
    }
}