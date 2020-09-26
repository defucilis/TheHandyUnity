using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;


namespace Defucilis.TheHandyUnity
{
    public class HandyConnection
    {
        private static readonly HttpClient Client = new HttpClient();
        
        public static string ConnectionKey { get; set; }
        public static HandyStatus Status { get; set; }
        public static HandyLogMode LogMode { get; set; }

        public static Action OnCommandStart { get; set; }
        public static Action OnCommandEnd { get; set; }
        public static Action<HandyStatus> OnStatusChanged { get; set; }

        //============================================================================================================//
        //                                                                                                            //
        //                                            MACHINE COMMANDS                                                //
        //                                                                                                            //
        //============================================================================================================//

        public static void SetMode(HandyStatus status, Action<HandyStatus> onSuccess = null, Action<string> onError = null)
        {
            DoCommand(
                "Set Mode", 
                async () => {
                    var response = await GetAsync(GetUrl("setMode", new Dictionary<string, string>
                    {
                        {"mode", ((int)status).ToString()}
                    }));
                    return response;
                }, 
                responseJson =>
                {
                    var newStatus = (HandyStatus) responseJson["mode"].AsInt;
                    if(Status != newStatus) OnStatusChanged?.Invoke(newStatus);
                    Status = newStatus;
                    onSuccess?.Invoke(newStatus);
                }, 
                onError
            );
        }
        
        public static void ToggleMode(HandyStatus status, Action<HandyStatus> onSuccess = null, Action<string> onError = null)
        {
            DoCommand(
                "Toggle Mode", 
                async () => {
                    var response = await GetAsync(GetUrl("toggleMode", new Dictionary<string, string>
                    {
                        {"mode", ((int)status).ToString()}
                    }));
                    return response;
                }, 
                responseJson =>
                {
                    var newStatus = (HandyStatus) responseJson["mode"].AsInt;
                    if(Status != newStatus) OnStatusChanged?.Invoke(newStatus);
                    Status = newStatus;
                    onSuccess?.Invoke(newStatus);
                }, 
                onError
            );
        }

        public static void SetSpeedPercent(int percent, Action<HandyStatusData> onSuccess = null, Action<string> onError = null)
        {
            //ensure we're in automatic mode before setting speed
            EnforceMode(HandyStatus.Automatic, () =>
            {
                DoCommand(
                    "Set Speed (Percent)", 
                    async () => {
                        var response = await GetAsync(GetUrl("setSpeed", new Dictionary<string, string>
                        {
                            {"speed", percent.ToString("0")},
                            {"type", "%25"}
                        }));
                        return response;
                    }, 
                    responseJson =>
                    {
                        onSuccess?.Invoke(new HandyStatusData()
                        {
                            CurrentPosition = responseJson["currentPosition"].AsFloat
                        });
                    }, 
                    onError
                );
            }, onError);
        }
        
        public static void SetSpeed(float speedMm, Action<HandyStatusData> onSuccess = null, Action<string> onError = null)
        {
            //ensure we're in automatic mode before setting speed
            EnforceMode(HandyStatus.Automatic, () =>
            {
                DoCommand(
                    "Set Speed (mm)", 
                    async () => {
                        var response = await GetAsync(GetUrl("setSpeed", new Dictionary<string, string>
                        {
                            {"speed", speedMm.ToString("0")},
                            {"type", "mm%2Fs"}
                        }));
                        return response;
                    }, 
                    responseJson =>
                    {
                        onSuccess?.Invoke(new HandyStatusData()
                        {
                            CurrentPosition = responseJson["currentPosition"].AsFloat
                        });
                    }, 
                    onError
                );
            }, onError);
        }
        
        public static void StepSpeedUp(Action<HandySpatialData> onSuccess = null, Action<string> onError = null)
        {
            //ensure we're in automatic mode before setting speed
            EnforceMode(HandyStatus.Automatic, () =>
            {
                DoCommand(
                    "Step Speed Up", 
                    async () => {
                        var response = await GetAsync(GetUrl("stepSpeed", new Dictionary<string, string>
                        {
                            {"step", "true"}
                        }));
                        return response;
                    }, 
                    responseJson =>
                    {
                        onSuccess?.Invoke(new HandySpatialData()
                        {
                            PercentageValue = responseJson["speedPercent"].AsFloat,
                            RawValue = responseJson["speed"].AsFloat * 0.25f // x 0.25 due to bug in API v1.0.0
                        });
                    }, 
                    onError
                );
            }, onError);
        }
        
        public static void StepSpeedDown(Action<HandySpatialData> onSuccess = null, Action<string> onError = null)
        {
            //ensure we're in automatic mode before setting speed
            EnforceMode(HandyStatus.Automatic, () =>
            {
                DoCommand(
                    "Step Speed Down", 
                    async () => {
                        var response = await GetAsync(GetUrl("stepSpeed", new Dictionary<string, string>
                        {
                            {"step", "false"}
                        }));
                        return response;
                    }, 
                    responseJson =>
                    {
                        onSuccess?.Invoke(new HandySpatialData()
                        {
                            PercentageValue = responseJson["speedPercent"].AsFloat,
                            RawValue = responseJson["speed"].AsFloat * 0.25f // x 0.25 due to bug in API v1.0.0
                        });
                    }, 
                    onError
                );
            }, onError);
        }

        public static void SetStrokePercent(int percent, Action<HandyStatusData> onSuccess = null, Action<string> onError = null)
        {
            DoCommand(
                "Set Stroke (Percent)", 
                async () => {
                    var response = await GetAsync(GetUrl("setStroke", new Dictionary<string, string>
                    {
                        {"stroke", percent.ToString("0")},
                        {"type", "%25"}
                    }));
                    return response;
                }, 
                responseJson =>
                {
                    onSuccess?.Invoke(new HandyStatusData()
                    {
                        CurrentPosition = responseJson["currentPosition"].AsFloat
                    });
                }, 
                onError
            );
        }
        
        public static void SetStroke(float strokeMm, Action<HandyStatusData> onSuccess = null, Action<string> onError = null)
        {
            DoCommand(
                "Set Stroke (mm)", 
                async () => {
                    var response = await GetAsync(GetUrl("setStroke", new Dictionary<string, string>
                    {
                        {"stroke", strokeMm.ToString("0")},
                        {"type", "mm%2Fs"}
                    }));
                    return response;
                }, 
                responseJson =>
                {
                    onSuccess?.Invoke(new HandyStatusData()
                    {
                        CurrentPosition = responseJson["currentPosition"].AsFloat
                    });
                }, 
                onError
            );
        }
        
        public static void StepStrokeUp(Action<HandySpatialData> onSuccess = null, Action<string> onError = null)
        {
            DoCommand(
                "Step Stroke Up", 
                async () => {
                    var response = await GetAsync(GetUrl("stepStroke", new Dictionary<string, string>
                    {
                        {"step", "true"}
                    }));
                    return response;
                }, 
                responseJson =>
                {
                    onSuccess?.Invoke(new HandySpatialData()
                    {
                        RawValue = responseJson["stroke"].AsFloat * 2f,
                        PercentageValue = responseJson["stroke"].AsFloat
                    });
                }, 
                onError
            );
        }
        
        public static void StepStrokeDown(Action<HandySpatialData> onSuccess = null, Action<string> onError = null)
        {
            DoCommand(
                "Step Stroke Down", 
                async () => {
                    var response = await GetAsync(GetUrl("stepStroke", new Dictionary<string, string>
                    {
                        {"step", "false"}
                    }));
                    return response;
                }, 
                responseJson =>
                {
                    onSuccess?.Invoke(new HandySpatialData()
                    {
                        RawValue = responseJson["stroke"].AsFloat * 2f,
                        PercentageValue = responseJson["stroke"].AsFloat
                    });
                }, 
                onError
            );
        }
        
        //============================================================================================================//
        //                                                                                                            //
        //                                                GET DATA                                                    //
        //                                                                                                            //
        //============================================================================================================//
        
        public static void GetVersion(Action<HandyVersionData> onSuccess = null, Action<string> onError = null)
        {
            DoCommand(
                "Get Version", 
                async () => {
                    var response = await GetAsync(GetUrl("getVersion"));
                    return response;
                }, 
                responseJson => {
                    onSuccess?.Invoke(new HandyVersionData()
                    {
                        CurrentVersion = responseJson["version"],
                        LatestVersion = responseJson["latest"]
                    });
                }, 
                onError
            );
        }
        
        public static void GetSettings(Action<HandyStatusData> onSuccess = null, Action<string> onError = null)
        {
            DoCommand(
                "Get Settings", 
                async () => {
                    var response = await GetAsync(GetUrl("getSettings"));
                    return response;
                }, 
                responseJson =>
                {
                    var newStatus = (HandyStatus) responseJson["mode"].AsInt;
                    if (newStatus != Status) OnStatusChanged?.Invoke(newStatus);
                    
                    Status = newStatus;
                    onSuccess?.Invoke(new HandyStatusData()
                    {
                        Status = newStatus,
                        CurrentPosition = responseJson["position"].AsFloat,
                        Speed = responseJson["speed"].AsFloat,
                        Stroke = responseJson["stroke"].AsFloat
                    });
                }, 
                onError
            );
        }
        
        public static void GetStatus(Action<HandyStatus> onSuccess = null, Action<string> onError = null)
        {
            DoCommand(
                "Get Status", 
                async () => {
                    var response = await GetAsync(GetUrl("getStatus"));
                    return response;
                }, 
                responseJson => {
                    onSuccess?.Invoke(HandyStatus.Off);
                }, 
                onError
            );
        }
        
        //============================================================================================================//
        //                                                                                                            //
        //                                                  SYNC                                                      //
        //                                                                                                            //
        //============================================================================================================//

        public static void GetServerTime(Action<float> onSuccess = null, Action<string> onError = null)
        {
            
        }

        public static void SyncPrepare(string url, string fileName = "", int fileSize = -1, Action onSuccess = null, Action<string> onError = null)
        {
            
        }

        public static void SyncPlay(int time = 0, long serverTime = -1, Action<HandyPlayingData> onSuccess = null, Action<string> onError = null)
        {
            
        }
        
        public static void SyncPause(Action<HandyPlayingData> onSuccess = null, Action<string> onError = null)
        {
            
        }

        public static void SyncOffset(int offset, Action<HandyPlayingData> onSuccess = null, Action<string> onError = null)
        {
            
        }
        
        
        //============================================================================================================//
        //============================================================================================================//
        //============================================================================================================//
        //                                                                                                            //
        //                                            INTERNAL + UTILITY                                              //
        //                                                                                                            //
        //============================================================================================================//
        //============================================================================================================//
        //============================================================================================================//


        private static void EnforceMode(HandyStatus mode, Action command, Action<string> onError)
        {
            if (Status != mode) {
                SetMode(mode, status => command(), onError);
            } else {
                command();
            }
        }
        
        private static async void DoCommand(string commandDescription, Func<Task<string>> request, Action<JSONNode> handleSuccess, Action<string> onError)
        {
            OnCommandStart?.Invoke();
            TryLogVerbose(commandDescription);
            if (CheckConnectionKey(commandDescription, onError)) {
                OnCommandEnd?.Invoke();
                return;
            }

            var response = await request();
            var responseJson = JSONNode.Parse(response);
            TryLogResponse(commandDescription, responseJson);
            
            if (ReportErrors(commandDescription, responseJson, onError)) {
                OnCommandEnd?.Invoke();
                return;
            }
            
            try {
                handleSuccess(responseJson);
                OnCommandEnd?.Invoke();
            } catch (Exception e) {
                TryLogError(commandDescription, e.Message, onError);
                OnCommandEnd?.Invoke();
            }
        }

        private static void TryLogVerbose(string commandDescription)
        {
            if ((int) LogMode >= (int) HandyLogMode.Verbose) {
                Debug.Log($"<color=blue>Beginning command {commandDescription}</color>");
            }
        }

        private static void TryLogError(string commandDescription, string error, Action<string> onError)
        {
            if ((int) LogMode >= (int) HandyLogMode.Errors) {
                Debug.LogError($"<color=red>{commandDescription} failed with error: {error}</color>");
            }
            onError?.Invoke(error);
        }
        
        private static void TryLogResponse(string commandString, JSONNode responseJson)
        {
            if ((int) LogMode >= (int) HandyLogMode.Responses) {
                Debug.Log($"{commandString} response: {responseJson.ToString(2)}");
            }
        }

        private static bool CheckConnectionKey(string commandDescription, Action<string> onError)
        {
            if (!string.IsNullOrEmpty(ConnectionKey)) return false;
            TryLogError(commandDescription, "No connection key provided", onError);
            return true;

        }
        
        private static bool ReportErrors(string commandDescription, JSONNode responseJson, Action<string> onError)
        {
            var error = string.Empty;
            if (responseJson["success"].IsNull) error = "Invalid response";
            if (!responseJson["success"].AsBool) error = responseJson["error"].IsNull 
                ? "Unknown error" 
                : (string)responseJson["error"];

            if (string.IsNullOrEmpty(error)) {
                return false;
            }

            TryLogError(commandDescription, error, onError);
            return true;
        }

        public static async void ConnectAsync(string connectionKey)
        {
            if (connectionKey == "") {
                var output = new JSONObject();
                output.Add("error", "No connection key provided");
                //OnResponse.Invoke(this, output);
            }

            ConnectionKey = connectionKey;
            var result = await GetAsync(GetUrl("getStatus"));
            //OnResponse.Invoke(this, JSONNode.Parse(result));
        }

        public static async void SetSpeedAsync(float speed)
        {
            if (Status != HandyStatus.Off && speed <= 0f) {
                var modeResult = await GetAsync(GetUrl("setMode", new Dictionary<string, string>
                {
                    {"mode", ((int) HandyStatus.Off).ToString()}
                }));
                //OnResponse.Invoke(this, JSONNode.Parse(modeResult));
                Status = HandyStatus.Off;
                return;
            } else if (Status == HandyStatus.Off && speed > 0f) {
                var modeResult = await GetAsync(GetUrl("setMode", new Dictionary<string, string>
                {
                    {"mode", ((int) HandyStatus.Automatic).ToString()}
                }));
                Status = HandyStatus.Automatic;
                //OnResponse.Invoke(this, JSONNode.Parse(modeResult));
            }

            var result = await GetAsync(GetUrl("setSpeed", new Dictionary<string, string>
            {
                {"speed", (speed * 100f).ToString("0")},
                {"type", "%25"}
            }));
            //OnResponse.Invoke(this, JSONNode.Parse(result));
        }

        public static async void SetStrokeAsync(float stroke)
        {
            var result = await GetAsync(GetUrl("setStroke", new Dictionary<string, string>
            {
                {"stroke", (stroke * 100f).ToString("0")},
                {"type", "%25"}
            }));
            //OnResponse.Invoke(this, JSONNode.Parse(result));
        }

        public static async void GetCsvUrl(Vector2Int[] data, Action<string> onConversionComplete)
        {
            var csv = "#{\"type\":\"handy\"}";
            foreach (var item in data) {
                csv += $"\n{item.x},{item.y}";
            }

            var bytes = Encoding.ASCII.GetBytes(csv);
            var result = await PostAsync("https://www.handyfeeling.com/api/sync/upload", new MemoryStream(bytes));
            Debug.Log(result);
            var json = JSONNode.Parse(result);
            var url = json["url"].IsNull ? "" : json["url"].ToString().Replace("\"", "");
            Debug.Log(url);
            if (!string.IsNullOrEmpty(url)) {
                var prepareResponse = await PrepareSync(url, bytes.Length);
                Debug.Log(prepareResponse);
            }

            onConversionComplete?.Invoke(url);
            //OnResponse.Invoke(this, JSONNode.Parse(result));
        }

        public static async Task<string> PrepareSync(string url, int size)
        {
            return await GetAsync(GetUrl("syncPrepare", new Dictionary<string, string>
            {
                {"url", url},
                {"timeout", "30000"} //30 seconds
            }));
        }

        public async Task<string> SyncPlay(int time = 0)
        {
            return await GetAsync(GetUrl("syncPlay", new Dictionary<string, string>
            {
                {"play", "true"},
                {"time", time.ToString()}
            }));
        }

        public async Task<string> SyncPause()
        {
            return await GetAsync(GetUrl("syncPlay", new Dictionary<string, string>
            {
                {"play", "false"}
            }));
        }

        public static async Task<string> GetAsync(string uri)
        {
            var response = await Client.GetAsync(uri);
            try {
                response.EnsureSuccessStatusCode();
            } catch (HttpRequestException e) {
                return "{success: false, error: " + e.Message + "}";
            }

            return await response.Content.ReadAsStringAsync();
        }

        public static async Task<string> PostAsync(string uri, Stream file)
        {
            var content = new MultipartFormDataContent
            {
                {
                    new StreamContent(file), "syncFile", "test.csv"
                }
            };
            var response = await Client.PostAsync(uri, content);
            try {
                response.EnsureSuccessStatusCode();
            } catch (HttpRequestException e) {
                return "{success: false, error: " + e.Message + "}";
            }

            return await response.Content.ReadAsStringAsync();
        }

        private static string GetUrl(string endpoint, Dictionary<string, string> urlParams = null,
            bool noConnectionKey = false)
        {
            var url = noConnectionKey
                ? $"https://www.handyfeeling.com/api/v1/{endpoint}"
                : $"https://www.handyfeeling.com/api/v1/{ConnectionKey}/{endpoint}";
            if (urlParams == null) return url;

            var first = true;
            foreach (var pair in urlParams) {
                if (first) url += "?";
                else url += "&";

                url += pair.Key + "=" + pair.Value;
                first = false;
            }

            Debug.Log(url);
            return url;
        }
    }
}