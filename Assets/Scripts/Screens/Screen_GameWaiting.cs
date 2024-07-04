using System.Collections;
using TeenPatti.Gameplay;
using UnityEngine;

namespace TeenPatti.Screens
{
    public class Screen_GameWaiting : Base_Screen
    {
        [Space]
        [SerializeField] EnvironmentManager env_manager;
        [SerializeField] SeatPlacementManager seat_manager;

        public override void Show()
        {
            base.Show();
            StartCoroutine(nameof(CheckRoomStates_And_Proceed));
        }

        IEnumerator CheckRoomStates_And_Proceed()
        {
            while (TPRoomManager.Instance.IsConnected == false || TPRoomManager.Instance.State.table_id == null)
                yield return new WaitForSeconds(0.5f);

            env_manager.Initialize();
            seat_manager.Initialize();

            SceneChangeManager.Instance.Hide_Loading();
            ScreenManager.Instance.SwitchScreen(SCREEN_TYPE.GAMEPLAY_COMMON, this.screen_type);
        }
    }
}