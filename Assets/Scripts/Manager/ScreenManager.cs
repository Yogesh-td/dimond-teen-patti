using Global.Helpers;
using System;
using System.Collections;
using UnityEngine;

namespace TeenPatti.Screens
{
    public class ScreenManager : Singleton<ScreenManager>
    {
        [Header("Scene Screens")]
        [SerializeField] Base_Screen[] all_screens;

        [Header("Basic Settings")]
        [SerializeField] SCREEN_TYPE first_screen;
        [SerializeField] float delay_time;

        public override void Awake()
        {
            base.Awake();
        }
        private IEnumerator Start()
        {
            yield return new WaitForSeconds(delay_time);
            ShowScreen(first_screen);
        }



        public Base_Screen Get_Screen(SCREEN_TYPE _stype)
        {
            return Array.Find(all_screens, x => x.screen_type == _stype);
        }


        public void SwitchScreen(SCREEN_TYPE _toShow, SCREEN_TYPE _toHide)
        {
            HideScreen(_toHide);
            ShowScreen(_toShow);
        }
        public void ShowScreen(SCREEN_TYPE _toShow)
        {
            Get_Screen(_toShow)?.Show();
        }
        public void HideScreen(SCREEN_TYPE _toHide)
        {
            Get_Screen(_toHide)?.Hide();
        }


        public void Show_Warning(string content_txt, string button_txt, Action button_callback = null, bool show_close_btn = true)
        {
            Get_Screen(SCREEN_TYPE.WARNING)?.GetComponent<Screen_Warning>().Show(
                content_txt,
                button_txt,
                button_callback,
                show_close_btn);
        }
    }

    public enum SCREEN_TYPE
    {
        none,
        SPLASH,
        LOGIN,
        MENU,
        EDIT_PROFILE,
        ABOUT_US,
        HISTORY,
        SETTINGS,
        SHARE_AND_EARN,
        LEADERBOARD,
        SHOP,
        WARNING,
        CLASSIC_PLAY,
        VARIATION_PLAY,
        DIAMOND_PLAY,
        TOURNAMENT_PLAY,
        PRIVATE_PLAY,

        GAMEPLAY_COMMON,
        GAMEPLAY_LOCAL,

        GAME_MENU,
        RULES,
        SIDESHOW_RECEIVED,
        TABLE_INFO,
        CHAT,
        GIFTS,
        PRIVATE_PLAY_CREATE,
        GAME_WAITING,
        APP_LOADER,
        CHANGE_PASSWORD,
        WITHDRAWAL
    }
}