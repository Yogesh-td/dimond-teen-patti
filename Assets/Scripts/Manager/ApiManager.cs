using Global.Helpers;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TeenPatti.App.Settings;
using TeenPatti.Encryption;
using TeenPatti.Helpers;
using TeenPatti.IAP;
using TeenPatti.Player;
using UnityEngine;
using UnityEngine.Networking;

namespace TeenPatti.App
{
    public class ApiManager : Singleton<ApiManager>
    {
        private string BASE_URL => CoreSettings.Instance.Api_URL;

        string URL_GetGameSettings => BASE_URL + "settings/getGameSettings";
        string URL_GetGameTables => BASE_URL + "settings/getTables";
        string URL_GetGameIaps => BASE_URL + "settings/getIaps";
        string URL_Login => BASE_URL + "users/login";
        string URL_GetProfile => BASE_URL + "users/getProfile";
        string URL_UpdateProfile => BASE_URL + "users/updateProfile";
        string URL_UpdatePassword => BASE_URL + "users/updatePassword";
        string URL_RedeemReferal => BASE_URL + "users/redeemReferal";
        string URL_ValidatePurchase => BASE_URL + "users/validatePurchase";
        string URL_GetLeaderboard => BASE_URL + "users/leaderboard";
        string URL_Withdraw => BASE_URL + "users/withdrawal";
        string URL_GetHistory => BASE_URL + "transactions/getHistory";

        public string access_token { get; private set; }


        private void Start()
        {
            AppManager.Instance.Event_OnPlayerLoggedOut += Instance_Event_OnPlayerLoggedOut;
        }
        private void OnDestroy()
        {
            AppManager.Instance.Event_OnPlayerLoggedOut -= Instance_Event_OnPlayerLoggedOut;
        }
        private void Instance_Event_OnPlayerLoggedOut()
        {
            access_token = string.Empty;
        }



        public void Api_GetGameSettings(Action<GAMESETTINGS> success = null, Action<string> fail = null)
        {
            StartCoroutine(Call_GetGameSettings(success, fail));
        }
        IEnumerator Call_GetGameSettings(Action<GAMESETTINGS> success, Action<string> fail)
        {
            string ts = DateTime.UtcNow.To_Unix_Timestamp().ToString();
            using (UnityWebRequest request = UnityWebRequest.Get(URL_GetGameSettings))
            {
                request.SetRequestHeader("ts", ts);
                request.SetRequestHeader("ts_auth", AESCryptography.Encrypt(ts));

                yield return request.SendWebRequest();
                if (request.result == UnityWebRequest.Result.Success || request.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.Log(request.result.ToString());
                    Debug.Log(request.downloadHandler.text);
                    RESPONCE_ROOT responce = JsonConvert.DeserializeObject<RESPONCE_ROOT>(request.downloadHandler.text);
                    if (!responce.metadata.status)
                    {
                        fail?.Invoke(responce.metadata.user_msg);
                        Debug.Log(responce.metadata.dev_msg);
                    }
                    else
                    {
                        Debug.Log(request.downloadHandler.text);
                        RESPONCE_DATA<GAMESETTINGS> responce_data = JsonConvert.DeserializeObject<RESPONCE_DATA<GAMESETTINGS>>(request.downloadHandler.text);
                        success?.Invoke(responce_data.data);
                    }
                }
                else
                {
                    fail?.Invoke(request.result.ToString());
                    Debug.Log(request.result.ToString());
                }
            }
        }


        public void Api_GetGameTables(Action<List<TABLESETTING>> success = null, Action<string> fail = null)
        {
            StartCoroutine(Call_GetGameTables(success, fail));
        }
        IEnumerator Call_GetGameTables(Action<List<TABLESETTING>> success, Action<string> fail)
        {
            string ts = DateTime.UtcNow.To_Unix_Timestamp().ToString();
            using (UnityWebRequest request = UnityWebRequest.Get(URL_GetGameTables))
            {
                request.SetRequestHeader("ts", ts);
                request.SetRequestHeader("ts_auth", AESCryptography.Encrypt(ts));

                yield return request.SendWebRequest();
                if (request.result == UnityWebRequest.Result.Success || request.result == UnityWebRequest.Result.ProtocolError)
                {
                    RESPONCE_ROOT responce = JsonConvert.DeserializeObject<RESPONCE_ROOT>(request.downloadHandler.text);
                    if (!responce.metadata.status)
                    {
                        fail?.Invoke(responce.metadata.user_msg);
                        Debug.Log(responce.metadata.dev_msg);
                    }
                    else
                    {
                        RESPONCE_DATA<List<TABLESETTING>> responce_data = JsonConvert.DeserializeObject<RESPONCE_DATA<List<TABLESETTING>>>(request.downloadHandler.text);
                        success?.Invoke(responce_data.data);
                    }
                }
                else
                {
                    fail?.Invoke(request.result.ToString());
                    Debug.Log(request.result.ToString());
                }
            }
        }


        public void Api_GetGameIaps(Action<List<IAPPRODUCT>> success = null, Action<string> fail = null)
        {
            StartCoroutine(Call_GetGameIaps(success, fail));
        }
        IEnumerator Call_GetGameIaps(Action<List<IAPPRODUCT>> success, Action<string> fail)
        {
            string ts = DateTime.UtcNow.To_Unix_Timestamp().ToString();
            using (UnityWebRequest request = UnityWebRequest.Get(URL_GetGameIaps))
            {
                request.SetRequestHeader("ts", ts);
                request.SetRequestHeader("ts_auth", AESCryptography.Encrypt(ts));

                yield return request.SendWebRequest();
                if (request.result == UnityWebRequest.Result.Success || request.result == UnityWebRequest.Result.ProtocolError)
                {
                    RESPONCE_ROOT responce = JsonConvert.DeserializeObject<RESPONCE_ROOT>(request.downloadHandler.text);
                    if (!responce.metadata.status)
                    {
                        fail?.Invoke(responce.metadata.user_msg);
                        Debug.Log(responce.metadata.dev_msg);
                    }
                    else
                    {
                        RESPONCE_DATA<List<IAPPRODUCT>> responce_data = JsonConvert.DeserializeObject<RESPONCE_DATA<List<IAPPRODUCT>>>(request.downloadHandler.text);
                        success?.Invoke(responce_data.data);
                    }
                }
                else
                {
                    fail?.Invoke(request.result.ToString());
                    Debug.Log(request.result.ToString());
                }
            }
        }


        public void Api_Login(string social_id, string password, Action success = null, Action<string> fail = null)
        {
            Dictionary<string, object> request = new Dictionary<string, object>();
            request.Add("social_id", social_id);
            request.Add("password", password);
            string json_params = JsonConvert.SerializeObject(request);

            StartCoroutine(Call_Login(json_params, success, fail));
        }
        IEnumerator Call_Login(string jsonRequestData, Action success, Action<string> fail)
        {
            string ts = DateTime.UtcNow.To_Unix_Timestamp().ToString();
#if UNITY_2022_OR_NEWER
            using (UnityWebRequest request = UnityWebRequest.PostWwwForm(URL_Login, ""))
#else
            using (UnityWebRequest request = UnityWebRequest.PostWwwForm(URL_Login, ""))
#endif
            {
                request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(jsonRequestData));
                request.SetRequestHeader("Content-Type", "application/json");
                request.SetRequestHeader("ts", ts);
                request.SetRequestHeader("ts_auth", AESCryptography.Encrypt(jsonRequestData));

                yield return request.SendWebRequest();
                if (request.result == UnityWebRequest.Result.Success || request.result == UnityWebRequest.Result.ProtocolError)
                {
                    RESPONCE_ROOT responce = JsonConvert.DeserializeObject<RESPONCE_ROOT>(request.downloadHandler.text);
                    if (!responce.metadata.status)
                    {
                        fail?.Invoke(responce.metadata.user_msg);
                        Debug.Log(responce.metadata.dev_msg);
                    }
                    else
                    {
                        access_token = JsonConvert.DeserializeObject<RESPONCE_DATA<string>>(request.downloadHandler.text).data;
                        success?.Invoke();
                    }
                }
                else
                {
                    fail?.Invoke(request.result.ToString());
                    Debug.Log(request.result.ToString());
                }

                request.uploadHandler.Dispose();
            }
        }


        public void Api_GetProfile(Action<PLAYERDATA> success = null, Action<string> fail = null)
        {
            StartCoroutine(Call_GetProfile(success, fail));
        }
        IEnumerator Call_GetProfile(Action<PLAYERDATA> success, Action<string> fail)
        {
            string ts = DateTime.UtcNow.To_Unix_Timestamp().ToString();
            using (UnityWebRequest request = UnityWebRequest.Get(URL_GetProfile))
            {
                request.SetRequestHeader("Content-Type", "application/json");
                request.SetRequestHeader("access_token", access_token);
                request.SetRequestHeader("ts", ts);
                request.SetRequestHeader("ts_auth", AESCryptography.Encrypt(ts));

                yield return request.SendWebRequest();
                if (request.result == UnityWebRequest.Result.Success || request.result == UnityWebRequest.Result.ProtocolError)
                {
                    RESPONCE_ROOT responce = JsonConvert.DeserializeObject<RESPONCE_ROOT>(request.downloadHandler.text);
                    if (!responce.metadata.status)
                    {
                        fail?.Invoke(responce.metadata.user_msg);
                        Debug.Log(responce.metadata.dev_msg);
                    }
                    else
                    {
                        RESPONCE_DATA<PLAYERDATA> responce_data = JsonConvert.DeserializeObject<RESPONCE_DATA<PLAYERDATA>>(request.downloadHandler.text);
                        success?.Invoke(responce_data.data);
                    }
                }
                else
                {
                    fail?.Invoke(request.result.ToString());
                    Debug.Log(request.result.ToString());
                }
            }
        }


        public void Api_UpdateProfile(string username, int avatar_index, Action<PLAYERDATA> success = null, Action<string> fail = null)
        {
            Dictionary<string, object> request = new Dictionary<string, object>();
            request.Add("username", username);
            request.Add("avatar_index", avatar_index);
            string json_params = JsonConvert.SerializeObject(request);

            StartCoroutine(Call_UpdateProfile(json_params, success, fail));
        }
        IEnumerator Call_UpdateProfile(string jsonRequestData, Action<PLAYERDATA> success, Action<string> fail)
        {
            string ts = DateTime.UtcNow.To_Unix_Timestamp().ToString();
#if UNITY_2022_OR_NEWER
            using (UnityWebRequest request = UnityWebRequest.PostWwwForm(URL_UpdateProfile, ""))
#else
            using (UnityWebRequest request = UnityWebRequest.PostWwwForm(URL_UpdateProfile, ""))
#endif
            {
                request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(jsonRequestData));
                request.SetRequestHeader("Content-Type", "application/json");
                request.SetRequestHeader("access_token", access_token);
                request.SetRequestHeader("ts", ts);
                request.SetRequestHeader("ts_auth", AESCryptography.Encrypt(jsonRequestData));

                yield return request.SendWebRequest();
                if (request.result == UnityWebRequest.Result.Success || request.result == UnityWebRequest.Result.ProtocolError)
                {
                    RESPONCE_ROOT responce = JsonConvert.DeserializeObject<RESPONCE_ROOT>(request.downloadHandler.text);
                    if (!responce.metadata.status)
                    {
                        fail?.Invoke(responce.metadata.user_msg);
                        Debug.Log(responce.metadata.dev_msg);
                    }
                    else
                    {
                        RESPONCE_DATA<PLAYERDATA> responce_data = JsonConvert.DeserializeObject<RESPONCE_DATA<PLAYERDATA>>(request.downloadHandler.text);
                        success?.Invoke(responce_data.data);
                    }
                }
                else
                {
                    fail?.Invoke(request.result.ToString());
                    Debug.Log(request.result.ToString());
                }

                request.uploadHandler.Dispose();
            }
        }


        public void Api_UpdatePassword(string current_password, string new_password, Action<string> success = null, Action<string> fail = null)
        {
            Dictionary<string, object> request = new Dictionary<string, object>();
            request.Add("current_password", current_password);
            request.Add("new_password", new_password);
            string json_params = JsonConvert.SerializeObject(request);

            StartCoroutine(Call_UpdatePassword(json_params, success, fail));
        }
        IEnumerator Call_UpdatePassword(string jsonRequestData, Action<string> success, Action<string> fail)
        {
            string ts = DateTime.UtcNow.To_Unix_Timestamp().ToString();
#if UNITY_2022_OR_NEWER
            using (UnityWebRequest request = UnityWebRequest.PostWwwForm(URL_UpdatePassword, ""))
#else
            using (UnityWebRequest request = UnityWebRequest.PostWwwForm(URL_UpdatePassword, ""))
#endif
            {
                request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(jsonRequestData));
                request.SetRequestHeader("Content-Type", "application/json");
                request.SetRequestHeader("access_token", access_token);
                request.SetRequestHeader("ts", ts);
                request.SetRequestHeader("ts_auth", AESCryptography.Encrypt(jsonRequestData));

                yield return request.SendWebRequest();
                if (request.result == UnityWebRequest.Result.Success || request.result == UnityWebRequest.Result.ProtocolError)
                {
                    RESPONCE_ROOT responce = JsonConvert.DeserializeObject<RESPONCE_ROOT>(request.downloadHandler.text);
                    if (!responce.metadata.status)
                    {
                        fail?.Invoke(responce.metadata.user_msg);
                        Debug.Log(responce.metadata.dev_msg);
                    }
                    else
                    {
                        success?.Invoke(responce.metadata.user_msg);
                        Debug.Log(responce.metadata.dev_msg);
                    }
                }
                else
                {
                    fail?.Invoke(request.result.ToString());
                    Debug.Log(request.result.ToString());
                }

                request.uploadHandler.Dispose();
            }
        }


        public void Api_GetHistory(Action<List<PLAYER_TRANSACTION>> success = null, Action<string> fail = null)
        {
            StartCoroutine(Call_GetHistory(success, fail));
        }
        IEnumerator Call_GetHistory(Action<List<PLAYER_TRANSACTION>> success, Action<string> fail)
        {
            string ts = DateTime.UtcNow.To_Unix_Timestamp().ToString();
            using (UnityWebRequest request = UnityWebRequest.Get(URL_GetHistory))
            {
                request.SetRequestHeader("access_token", access_token);
                request.SetRequestHeader("ts", ts);
                request.SetRequestHeader("ts_auth", AESCryptography.Encrypt(ts));

                yield return request.SendWebRequest();
                if (request.result == UnityWebRequest.Result.Success || request.result == UnityWebRequest.Result.ProtocolError)
                {
                    RESPONCE_ROOT responce = JsonConvert.DeserializeObject<RESPONCE_ROOT>(request.downloadHandler.text);
                    if (!responce.metadata.status)
                    {
                        fail?.Invoke(responce.metadata.user_msg);
                        Debug.Log(responce.metadata.dev_msg);
                    }
                    else
                    {
                        RESPONCE_DATA<List<PLAYER_TRANSACTION>> responce_data = JsonConvert.DeserializeObject<RESPONCE_DATA<List<PLAYER_TRANSACTION>>>(request.downloadHandler.text);
                        success?.Invoke(responce_data.data);
                    }
                }
                else
                {
                    fail?.Invoke(request.result.ToString());
                    Debug.Log(request.result.ToString());
                }
            }
        }


        public void Api_RedeemRefer(string refer_code, Action<PLAYERDATA> success = null, Action<string> fail = null)
        {
            Dictionary<string, object> request = new Dictionary<string, object>();
            request.Add("refer_code", refer_code);
            string json_params = JsonConvert.SerializeObject(request);

            StartCoroutine(Call_RedeemRefer(json_params, success, fail));
        }
        IEnumerator Call_RedeemRefer(string jsonRequestData, Action<PLAYERDATA> success, Action<string> fail)
        {
            string ts = DateTime.UtcNow.To_Unix_Timestamp().ToString();
#if UNITY_2022_OR_NEWER
            using (UnityWebRequest request = UnityWebRequest.PostWwwForm(URL_RedeemReferal, ""))
#else
            using (UnityWebRequest request = UnityWebRequest.PostWwwForm(URL_RedeemReferal, ""))
#endif
            {
                request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(jsonRequestData));
                request.SetRequestHeader("Content-Type", "application/json");
                request.SetRequestHeader("access_token", access_token);
                request.SetRequestHeader("ts", ts);
                request.SetRequestHeader("ts_auth", AESCryptography.Encrypt(jsonRequestData));

                yield return request.SendWebRequest();
                if (request.result == UnityWebRequest.Result.Success || request.result == UnityWebRequest.Result.ProtocolError)
                {
                    RESPONCE_ROOT responce = JsonConvert.DeserializeObject<RESPONCE_ROOT>(request.downloadHandler.text);
                    if (!responce.metadata.status)
                    {
                        fail?.Invoke(responce.metadata.user_msg);
                        Debug.Log(responce.metadata.dev_msg);
                    }
                    else
                    {
                        RESPONCE_DATA<PLAYERDATA> responce_data = JsonConvert.DeserializeObject<RESPONCE_DATA<PLAYERDATA>>(request.downloadHandler.text);
                        success?.Invoke(responce_data.data);
                    }
                }
                else
                {
                    fail?.Invoke(request.result.ToString());
                    Debug.Log(request.result.ToString());
                }

                request.uploadHandler.Dispose();
            }
        }


        public void Api_ValidatePurchase(string iap_id, string trx_id, Action<PLAYERDATA> success = null, Action<string> fail = null)
        {
            Dictionary<string, object> request = new Dictionary<string, object>();
            request.Add("_id", iap_id);
            request.Add("trx_id", trx_id);
            request.Add("package_name", Application.identifier);
            string json_params = JsonConvert.SerializeObject(request);

            StartCoroutine(Call_ValidatePurchase(json_params, success, fail));
        }
        IEnumerator Call_ValidatePurchase(string jsonRequestData, Action<PLAYERDATA> success, Action<string> fail)
        {
            string ts = DateTime.UtcNow.To_Unix_Timestamp().ToString();
#if UNITY_2022_OR_NEWER
            using (UnityWebRequest request = UnityWebRequest.PostWwwForm(URL_ValidatePurchase, ""))
#else
            using (UnityWebRequest request = UnityWebRequest.PostWwwForm(URL_ValidatePurchase, ""))
#endif
            {
                request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(jsonRequestData));
                request.SetRequestHeader("Content-Type", "application/json");
                request.SetRequestHeader("access_token", access_token);
                request.SetRequestHeader("ts", ts);
                request.SetRequestHeader("ts_auth", AESCryptography.Encrypt(jsonRequestData));

                yield return request.SendWebRequest();
                if (request.result == UnityWebRequest.Result.Success || request.result == UnityWebRequest.Result.ProtocolError)
                {
                    RESPONCE_ROOT responce = JsonConvert.DeserializeObject<RESPONCE_ROOT>(request.downloadHandler.text);
                    if (!responce.metadata.status)
                    {
                        fail?.Invoke(responce.metadata.user_msg);
                        Debug.Log(responce.metadata.dev_msg);
                    }
                    else
                    {
                        RESPONCE_DATA<PLAYERDATA> responce_data = JsonConvert.DeserializeObject<RESPONCE_DATA<PLAYERDATA>>(request.downloadHandler.text);
                        success?.Invoke(responce_data.data);
                    }
                }
                else
                {
                    fail?.Invoke(request.result.ToString());
                    Debug.Log(request.result.ToString());
                }

                request.uploadHandler.Dispose();
            }
        }


        public void Api_GetLeaderboard(Action<LEADERBOARD_PLAYERS_DATA> success = null, Action<string> fail = null)
        {
            StartCoroutine(Call_GetLeaderboard(success, fail));
        }
        IEnumerator Call_GetLeaderboard(Action<LEADERBOARD_PLAYERS_DATA> success, Action<string> fail)
        {
            string ts = DateTime.UtcNow.To_Unix_Timestamp().ToString();
            using (UnityWebRequest request = UnityWebRequest.Get(URL_GetLeaderboard))
            {
                request.SetRequestHeader("access_token", access_token);
                request.SetRequestHeader("ts", ts);
                request.SetRequestHeader("ts_auth", AESCryptography.Encrypt(ts));

                yield return request.SendWebRequest();
                if (request.result == UnityWebRequest.Result.Success || request.result == UnityWebRequest.Result.ProtocolError)
                {
                    RESPONCE_ROOT responce = JsonConvert.DeserializeObject<RESPONCE_ROOT>(request.downloadHandler.text);
                    if (!responce.metadata.status)
                    {
                        fail?.Invoke(responce.metadata.user_msg);
                        Debug.Log(responce.metadata.dev_msg);
                    }
                    else
                    {
                        RESPONCE_DATA<LEADERBOARD_PLAYERS_DATA> responce_data = JsonConvert.DeserializeObject<RESPONCE_DATA<LEADERBOARD_PLAYERS_DATA>>(request.downloadHandler.text);
                        success?.Invoke(responce_data.data);
                    }
                }
                else
                {
                    fail?.Invoke(request.result.ToString());
                    Debug.Log(request.result.ToString());
                }
            }
        }


        public void Api_Withdraw(int amount, Action<PLAYERDATA> success = null, Action<string> fail = null)
        {
            Dictionary<string, object> request = new Dictionary<string, object>();
            request.Add("amount", amount);
            string json_params = JsonConvert.SerializeObject(request);

            StartCoroutine(Call_Withdraw(json_params, success, fail));
        }
        IEnumerator Call_Withdraw(string jsonRequestData, Action<PLAYERDATA> success, Action<string> fail)
        {
            string ts = DateTime.UtcNow.To_Unix_Timestamp().ToString();
#if UNITY_2022_OR_NEWER
            using (UnityWebRequest request = UnityWebRequest.PostWwwForm(URL_Withdraw, ""))
#else
            using (UnityWebRequest request = UnityWebRequest.PostWwwForm(URL_Withdraw, ""))
#endif
            {
                request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(jsonRequestData));
                request.SetRequestHeader("Content-Type", "application/json");
                request.SetRequestHeader("access_token", access_token);
                request.SetRequestHeader("ts", ts);
                request.SetRequestHeader("ts_auth", AESCryptography.Encrypt(jsonRequestData));

                yield return request.SendWebRequest();
                if (request.result == UnityWebRequest.Result.Success || request.result == UnityWebRequest.Result.ProtocolError)
                {
                    RESPONCE_ROOT responce = JsonConvert.DeserializeObject<RESPONCE_ROOT>(request.downloadHandler.text);
                    if (!responce.metadata.status)
                    {
                        fail?.Invoke(responce.metadata.user_msg);
                        Debug.Log(responce.metadata.dev_msg);
                    }
                    else
                    {
                        RESPONCE_DATA<PLAYERDATA> responce_data = JsonConvert.DeserializeObject<RESPONCE_DATA<PLAYERDATA>>(request.downloadHandler.text);
                        success?.Invoke(responce_data.data);
                    }
                }
                else
                {
                    fail?.Invoke(request.result.ToString());
                    Debug.Log(request.result.ToString());
                }

                request.uploadHandler.Dispose();
            }
        }
    }



    [System.Serializable]
    public class RESPONCE_ROOT
    {
        public Metadata metadata;
    }
    [System.Serializable]
    public class RESPONCE_DATA<T>
    {
        public T data;
    }
    [System.Serializable]
    public class Metadata
    {
        public bool status;
        public string user_msg;
        public string dev_msg;
    }
}