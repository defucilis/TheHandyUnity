using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;


namespace Defucilis.TheHandyUnity
{
    /// <summary>
    /// Main class for accessing Handy functions
    /// </summary>
    public static class HandyConnection
    {
        /// <summary>
        /// The Handy connection key - required for all API functionality!
        /// </summary>
        public static string ConnectionKey { get; set; }
        
        /// <summary>
        /// The last known status of the Handy. Call GetStatus if you need to be certain of the actual current status.
        /// </summary>
        public static HandyMode Mode { get; set; }
        
        /// <summary>
        /// The offset time for server synchronisation, defaults to zero and is updated when you call GetServerTime
        /// </summary>
        public static long ServerTimeOffset { get; private set; }
        
        /// <summary>
        /// Mode for generating Unity Debug logs
        /// </summary>
        public static HandyLogMode LogMode { get; set; }

        /// <summary>
        /// Callback whenever an event starts - use to display loading feedback or to make sure you don't call multiple events at once
        /// </summary>
        public static Action OnCommandStart { get; set; }
        
        /// <summary>
        /// Callback whenever an event ends - use to hide loading feedback etc
        /// </summary>
        public static Action OnCommandEnd { get; set; }
        
        /// <summary>
        /// Callback whenever the Handy status changes.
        /// Some functions change the status automatically, such as SetSpeed enabling AutomaticMode, and SyncPrepare enabling Sync mode
        /// </summary>
        public static Action<HandyMode> OnStatusChanged { get; set; }
        
        private static readonly HttpClient Client = new HttpClient();

        //============================================================================================================//
        //                                                                                                            //
        //                                            MACHINE COMMANDS                                                //
        //                                                                                                            //
        //============================================================================================================//

        /// <summary>
        /// Sets the Mode for the Handy.
        /// </summary>
        /// <param name="mode">The new mode the device should be in</param>
        /// <param name="onSuccess">Callback indicating success, contains the newly set mode</param>
        /// <param name="onError">Callback indicating failure, contains the error message</param>
        public static void SetMode(HandyMode mode, Action<HandyMode> onSuccess = null, Action<string> onError = null)
        {
            DoCommand(
                "Set Mode", 
                async () => {
                    var response = await GetAsync(GetUrl("setMode", new Dictionary<string, string>
                    {
                        {"mode", ((int)mode).ToString()}
                    }));
                    return response;
                }, 
                responseJson =>
                {
                    var newStatus = (HandyMode) responseJson["mode"].AsInt;
                    if(Mode != newStatus) OnStatusChanged?.Invoke(newStatus);
                    Mode = newStatus;
                    onSuccess?.Invoke(newStatus);
                }, 
                onError
            );
        }
        
        /// <summary>
        /// Toggles between the OFF mode, and the specified mode
        /// </summary>
        /// <param name="mode">The device is toggled between OFF and this mode</param>
        /// <param name="onSuccess">Callback indicating success, contains the newly set mode</param>
        /// <param name="onError">Callback indicating failure, contains the error message</param>
        public static void ToggleMode(HandyMode mode, Action<HandyMode> onSuccess = null, Action<string> onError = null)
        {
            DoCommand(
                "Toggle Mode", 
                async () => {
                    var response = await GetAsync(GetUrl("toggleMode", new Dictionary<string, string>
                    {
                        {"mode", ((int)mode).ToString()}
                    }));
                    return response;
                }, 
                responseJson =>
                {
                    var newStatus = (HandyMode) responseJson["mode"].AsInt;
                    if(Mode != newStatus) OnStatusChanged?.Invoke(newStatus);
                    Mode = newStatus;
                    onSuccess?.Invoke(newStatus);
                }, 
                onError
            );
        }

        /// <summary>
        /// Sets the speed of the Handy as a percentage of its maximum speed of 400 mm/s.  This puts the Handy in Automatic mode, if it wasn't already.
        /// </summary>
        /// <param name="percent">The speed as a percentage (0 to 100)</param>
        /// <param name="onSuccess">Callback indicating success, contains the current Handy position</param>
        /// <param name="onError">Callback indicating failure, contains the error message</param>
        public static void SetSpeedPercent(int percent, Action<HandyStatusData> onSuccess = null, Action<string> onError = null)
        {
            percent = Mathf.Clamp(percent, 0, 100);
            
            //ensure we're in automatic mode before setting speed
            EnforceMode(HandyMode.Automatic, () =>
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
        
        /// <summary>
        /// Sets the speed of the Handy in mm/s. This puts the Handy in Automatic mode, if it wasn't already.
        /// </summary>
        /// <param name="speedMm">The speed in mm/s - 400 is the maximum value</param>
        /// <param name="onSuccess">Callback indicating success, contains the current Handy position</param>
        /// <param name="onError">Callback indicating failure, contains the error message</param>
        public static void SetSpeed(float speedMm, Action<HandyStatusData> onSuccess = null, Action<string> onError = null)
        {
            speedMm = Mathf.Clamp(speedMm, 0f, 400f);
            
            //ensure we're in automatic mode before setting speed
            EnforceMode(HandyMode.Automatic, () =>
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
        
        /// <summary>
        /// Adds 10% to the speed of the Handy.  This puts the Handy in Automatic mode, if it wasn't already.
        /// </summary>
        /// <param name="onSuccess">Callback indicating success, contains the new speed data in mm/s and as a percentage</param>
        /// <param name="onError">Callback indicating failure, contains the error message</param>
        public static void StepSpeedUp(Action<HandySpatialData> onSuccess = null, Action<string> onError = null)
        {
            //ensure we're in automatic mode before setting speed
            EnforceMode(HandyMode.Automatic, () =>
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
        
        /// <summary>
        /// Subtracts 10% from the speed of the Handy.  This puts the Handy in Automatic mode, if it wasn't already.
        /// </summary>
        /// <param name="onSuccess">Callback indicating success, contains the new speed data in mm/s and as a percentage</param>
        /// <param name="onError">Callback indicating failure, contains the error message</param>
        public static void StepSpeedDown(Action<HandySpatialData> onSuccess = null, Action<string> onError = null)
        {
            //ensure we're in automatic mode before setting speed
            EnforceMode(HandyMode.Automatic, () =>
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

        /// <summary>
        /// Sets the stroke length of the Handy as a percentage. This works in Automatic mode and in Sync mode.
        /// </summary>
        /// <param name="percent">The stroke length as a percentage (0 to 100)</param>
        /// <param name="onSuccess">Callback indicating success, contains the current Handy position</param>
        /// <param name="onError">Callback indicating failure, contains the error message</param>
        public static void SetStrokePercent(int percent, Action<HandyStatusData> onSuccess = null, Action<string> onError = null)
        {
            percent = Mathf.Clamp(percent, 0, 100);
            
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
        
        /// <summary>
        /// Sets the stroke length of the Handy in mm. The maximum value is 200
        /// </summary>
        /// <param name="strokeMm">The stroke length in mm (0 to 200)</param>
        /// <param name="onSuccess">Callback indicating success, contains the current Handy position</param>
        /// <param name="onError">Callback indicating failure, contains the error message</param>
        public static void SetStroke(float strokeMm, Action<HandyStatusData> onSuccess = null, Action<string> onError = null)
        {
            strokeMm = Mathf.Clamp(strokeMm, 0f, 200f);
            
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
        
        /// <summary>
        /// Adds 10% to the stroke length of the Handy
        /// </summary>
        /// <param name="onSuccess">Callback indicating success, contains the new stroke data in mm and as a percentage</param>
        /// <param name="onError">Callback indicating failure, contains the error message</param>
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
        
        /// <summary>
        /// Subtracts 10% from the stroke length of the Handy
        /// </summary>
        /// <param name="onSuccess">Callback indicating success, contains the new stroke data in mm and as a percentage</param>
        /// <param name="onError">Callback indicating failure, contains the error message</param>
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
        
        /// <summary>
        /// Gets the current firmware version of the Handy, as well as the latest available firmware version
        /// Use this to display a message to the user indicating that a new firmware version is available for their Handy
        /// </summary>
        /// <param name="onSuccess">Callback indicating success, contains the current and latest available firmware versions as strings</param>
        /// <param name="onError">Callback indicating failure, contains the error message</param>
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
        
        /// <summary>
        /// Gets the mode, position, speed and stroke values from the Handy
        /// </summary>
        /// <param name="onSuccess">Callback indicating success, contains the current mode, position, speed and stroke values (as percentages)</param>
        /// <param name="onError">Callback indicating failure, contains the error message</param>
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
                    var newStatus = (HandyMode) responseJson["mode"].AsInt;
                    if (newStatus != Mode) OnStatusChanged?.Invoke(newStatus);
                    
                    Mode = newStatus;
                    onSuccess?.Invoke(new HandyStatusData()
                    {
                        Mode = newStatus,
                        CurrentPosition = responseJson["position"].AsFloat,
                        Speed = responseJson["speed"].AsFloat,
                        Stroke = responseJson["stroke"].AsFloat
                    });
                }, 
                onError
            );
        }
        
        /// <summary>
        /// Gets the current mode of the Handy. This is the best way to determine whether the Handy is connected
        /// This also updates the Mode property on the HandyConnection class
        /// </summary>
        /// <param name="onSuccess">Callback indicating success, contains the current mode of the device</param>
        /// <param name="onError">Callback indicating failure, contains the error message</param>
        public static void GetStatus(Action<HandyMode> onSuccess = null, Action<string> onError = null)
        {
            DoCommand(
                "Get Status", 
                async () => {
                    var response = await GetAsync(GetUrl("getStatus"));
                    return response;
                }, 
                responseJson => {
                    onSuccess?.Invoke(HandyMode.Off);
                }, 
                onError
            );
        }
        
        //============================================================================================================//
        //                                                                                                            //
        //                                                  SYNC                                                      //
        //                                                                                                            //
        //============================================================================================================//

        /// <summary>
        /// Runs a series of checks to determine the server time sync offset value
        /// This is important if you want the Handy's movements to stay in-sync with locally-playing video content!
        /// Once this function completes, the value of ServerTimeOffset is automatically updated, and will be applied whenever relevant
        /// Be warned! You only have 120 API calls per hour per device and each trip taken during this function counts as one of them!
        /// </summary>
        /// <param name="trips">How many requests to make. The offset value is the average offset time for each trip. Accuracy improves with more trips, the API recommends 30</param>
        /// <param name="onSuccess">Callback indicating success, contains the newly calculated server time offset</param>
        /// <param name="onError">Callback indicating failure, contains the error message</param>
        public static async void GetServerTime(int trips = 30, Action<long> onSuccess = null, Action<string> onError = null)
        {
            const string commandDescription = "Get Server Time";
            
            OnCommandStart?.Invoke();
            TryLogVerbose(commandDescription);
            
            if (CheckConnectionKey(commandDescription, onError)) {
                OnCommandEnd?.Invoke();
                return;
            }

            var offsetAggregate = 0L;
            for (var i = 0; i < trips; i++) {
                var startTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();

                var response = await GetAsync(GetUrl("getServerTime"));
                var responseJson = (JSONObject)JSONNode.Parse(response);
                TryLogResponse(commandDescription, responseJson);

                if (responseJson["error"] != null) {
                    TryLogError(commandDescription, responseJson["error"], onError);
                    OnCommandEnd?.Invoke();
                    return;
                }

                var endTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                var rtd = endTime - startTime;
                var estimatedServerTime = long.Parse(responseJson["serverTime"]) + rtd / 2;
                var offset = estimatedServerTime - endTime;
                offsetAggregate += offset;
            }

            offsetAggregate /= trips;
            ServerTimeOffset = offsetAggregate;
            if ((int) LogMode >= (int) HandyLogMode.Verbose) {
                Debug.Log($"<color=blue>Calculated server offset as {ServerTimeOffset} milliseconds</color>");
            }
            
            onSuccess?.Invoke(ServerTimeOffset);
            OnCommandEnd?.Invoke();
        }

        /// <summary>
        /// Sends a CSV control file to the Handy for sync playback. This file needs to be hosted at a static URL. To get a static URL, use the PatternToURL function.
        /// </summary>
        /// <param name="url">The URL the CSV file is hosted at</param>
        /// <param name="fileName">Optional filename parameter. If the filename and fileSize match what is already loaded on the Handy, loading will be skipped</param>
        /// <param name="fileSize">Optional filesize parameter (in bytes). If the filename and fileSize match what is already loaded on the Handy, loading will be skipped</param>
        /// <param name="onSuccess">Callback indicating success, indicates that the Handy is ready for sync playback</param>
        /// <param name="onError">Callback indicating failure, contains the error message</param>
        public static void SyncPrepare(string url, string fileName = "", int fileSize = -1, Action onSuccess = null, Action<string> onError = null)
        {
            DoCommand(
                "Sync Prepare", 
                async () => {
                    var urlParams = new Dictionary<string, string>
                    {
                        {"url", url}
                    };
                    if (!string.IsNullOrEmpty(fileName)) urlParams.Add("name", fileName);
                    if (fileSize > 0) urlParams.Add("size", fileSize.ToString());
                    urlParams.Add("timeout", "30000"); //30 seconds - to account for larger files
                    
                    var response = await GetAsync(GetUrl("syncPrepare", urlParams));
                    return response;
                }, 
                responseJson => {
                    //This command sets the Handy mode to Sync automatically
                    if (Mode != HandyMode.Sync) {
                        Mode = HandyMode.Sync;
                        OnStatusChanged?.Invoke(Mode);
                    }

                    onSuccess?.Invoke();
                }, 
                onError
            );
        }

        /// <summary>
        /// Begins playback of the last loaded sync file
        /// </summary>
        /// <param name="time">Playback time to start from, in milliseconds from the beginning of the file</param>
        /// <param name="onSuccess">Callback indicating success, contains whether the device is now playing back a file (should be true), and also contains the sync offset value as previously set</param>
        /// <param name="onError">Callback indicating failure, contains the error message</param>
        public static void SyncPlay(int time = 0, Action<HandyPlayingData> onSuccess = null, Action<string> onError = null)
        {
            //Ensure we're in sync mode before sync playing
            EnforceMode(HandyMode.Sync, () =>
            {
                DoCommand(
                    "Sync Play",
                    async () =>
                    {
                        var urlParams = new Dictionary<string, string>
                        {
                            {"play", "true"}
                        };
                        if (time > 0) urlParams.Add("time", time.ToString());
                        if (ServerTimeOffset != 0) {
                            var estimatedServerTime = DateTimeOffset.Now.ToUnixTimeMilliseconds() + ServerTimeOffset;
                            urlParams.Add("serverTime", estimatedServerTime.ToString());
                        }

                        var response = await GetAsync(GetUrl("syncPlay", urlParams));
                        return response;
                    },
                    responseJson =>
                    {
                        onSuccess?.Invoke(new HandyPlayingData()
                        {
                            Playing = responseJson["playing"].AsBool,
                            SetOffset = responseJson["setOffset"].AsInt
                        });
                    },
                    onError
                );
            }, onError);
        }
        
        /// <summary>
        /// Pauses playback of the last loaded sync file (if it is already playing)
        /// </summary>
        /// <param name="onSuccess">Callback indicating success, contains whether the device is now playing back a file (should be false)</param>
        /// <param name="onError">Callback indicating failure, contains the error message</param>
        public static void SyncPause(Action<HandyPlayingData> onSuccess = null, Action<string> onError = null)
        {
            DoCommand(
                "Sync Pause", 
                async () => {
                    var urlParams = new Dictionary<string, string>
                    {
                        {"play", "false"}
                    };
                    if (ServerTimeOffset != 0) {
                        var estimatedServerTime = DateTimeOffset.Now.ToUnixTimeMilliseconds() + ServerTimeOffset;
                        urlParams.Add("serverTime", estimatedServerTime.ToString());
                    }

                    var response = await GetAsync(GetUrl("syncPlay", urlParams));
                    return response;
                }, 
                responseJson => {
                    onSuccess?.Invoke(new HandyPlayingData()
                    {
                        Playing = responseJson["playing"].AsBool
                    });
                }, 
                onError
            );
        }

        /// <summary>
        /// Sets the sync offset value - this is to tweak the playback time of the Handy sync playback to better match on-screen visuals, and must be done by the end-user
        /// </summary>
        /// <param name="offset">The new offset time in milliseconds (can be negative)</param>
        /// <param name="onSuccess">Callback indicating success, contains the newly set sync offset value</param>
        /// <param name="onError">Callback indicating failure, contains the error message</param>
        public static void SyncOffset(int offset, Action<HandyPlayingData> onSuccess = null, Action<string> onError = null)
        {
            DoCommand(
                "Sync Offset",
                async () =>
                {
                    var urlParams = new Dictionary<string, string>
                    {
                        {"offset", offset.ToString()}
                    };
                    var response = await GetAsync(GetUrl("syncOffset", urlParams));
                    return response;
                },
                responseJson =>
                {
                    onSuccess?.Invoke(new HandyPlayingData()
                    {
                        SetOffset = responseJson["offset"].AsInt
                    });
                },
                onError
            );
        }

        /// <summary>
        /// Converts a list of time/position pairs to a CSV file hosted on Handy's servers, ready to be loaded onto a Handy using PrepareSync
        /// </summary>
        /// <param name="patternData">The data to be converted.
        /// The x coordinate represents the time in milliseconds from the beginning of the file
        /// The y coordinate represents the position as a percentage of the current stroke value that the Handy should be at, at the given time value</param>
        /// <param name="onSuccess">Callback indicating success, contains the URL of the newly created CSV file</param>
        /// <param name="onError">Callback indicating failure, contains the error message</param>
        public static async void PatternToUrl(Vector2Int[] patternData, Action<string> onSuccess = null, Action<string> onError = null)
        {
            const string commandDescription = "Pattern to URL";
            OnCommandStart?.Invoke();
            TryLogVerbose(commandDescription);

            if (patternData == null || patternData.Length == 0) {
                TryLogError(commandDescription, "No pattern data provided", onError);
                OnCommandEnd?.Invoke();
                return;
            }

            try {
                var csv = "#{\"type\":\"handy\"}";
                foreach (var item in patternData) {
                    csv += $"\n{item.x},{item.y}";
                }

                var bytes = Encoding.ASCII.GetBytes(csv);

                var response = await PostAsync(
                    "https://www.handyfeeling.com/api/sync/upload", 
                    $"UnityGenerated_{DateTimeOffset.Now.ToUnixTimeMilliseconds()}.csv", 
                    new MemoryStream(bytes)
                );
                var responseJson = JSONNode.Parse(response);
                TryLogResponse(commandDescription, responseJson);

                var url = responseJson["url"] == null ? "" : (string) responseJson["url"];
                onSuccess?.Invoke(url);
                OnCommandEnd?.Invoke();
            } catch (Exception e) {
                TryLogError(commandDescription, "Unexpected error: " + e.Message, onError);
                OnCommandEnd?.Invoke();
            }
        }

        /// <summary>
        /// Converts a funscript to a CSV file hosted on Handy's servers, ready to be loaded onto a Handy using PrepareSync
        /// </summary>
        /// <param name="funscript">The funscript to be loaded, as a string</param>
        /// <param name="fileName">Optional filename, one will be generated if left off</param>
        /// <param name="onSuccess">Callback indicating success, contains the URL of the newly created CSV file</param>
        /// <param name="onError">Callback indicating failure, contains the error message</param>
        public static async void FunscriptToUrl(string funscript, string fileName = "", Action<string> onSuccess = null, Action<string> onError = null)
        {
            const string commandDescription = "Funscript to URL";
            OnCommandStart?.Invoke();
            TryLogVerbose(commandDescription);

            if (string.IsNullOrEmpty(funscript)) {
                TryLogError(commandDescription, "No funscript provided", onError);
                OnCommandEnd?.Invoke();
                return;
            }

            try {
                var bytes = Encoding.ASCII.GetBytes(funscript);

                var response = await PostAsync(
                    "https://www.handyfeeling.com/api/sync/upload", 
                    string.IsNullOrEmpty(fileName)
                        ? $"UnityGenerated_{DateTimeOffset.Now.ToUnixTimeMilliseconds()}.funscript"
                        : fileName, 
                    new MemoryStream(bytes)
                );
                var responseJson = JSONNode.Parse(response);
                TryLogResponse(commandDescription, responseJson);

                var url = responseJson["url"] == null ? "" : (string) responseJson["url"];
                onSuccess?.Invoke(url);
                OnCommandEnd?.Invoke();
            } catch (Exception e) {
                TryLogError(commandDescription, "Unexpected error: " + e.Message, onError);
                OnCommandEnd?.Invoke();
            }
        }

        /// <summary>
        /// Uploads a CSV file to Handy's servers, ready to be loaded onto a Handy using PrepareSync
        /// </summary>
        /// <param name="csv">The CSV to be loaded, as a string. Each line should be in the format [time (ms)],[position (%)]</param>
        /// <param name="fileName">Optional filename, one will be generated if left off</param>
        /// <param name="onSuccess">Callback indicating success, contains the URL of the newly uploaded CSV file</param>
        /// <param name="onError">Callback indicating failure, contains the error message</param>
        public static async void CsvToUrl(string csv, string fileName = "", Action<string> onSuccess = null, Action<string> onError = null)
        {
            const string commandDescription = "CSV to URL";
            OnCommandStart?.Invoke();
            TryLogVerbose(commandDescription);

            if (string.IsNullOrEmpty(csv)) {
                TryLogError(commandDescription, "No CSV provided", onError);
                OnCommandEnd?.Invoke();
                return;
            }

            try {
                var bytes = Encoding.ASCII.GetBytes(csv);

                var response = await PostAsync(
                    "https://www.handyfeeling.com/api/sync/upload", 
                    string.IsNullOrEmpty(fileName)
                        ? $"UnityGenerated_{DateTimeOffset.Now.ToUnixTimeMilliseconds()}.csv"
                        : fileName, 
                    new MemoryStream(bytes)
                );
                var responseJson = JSONNode.Parse(response);
                TryLogResponse(commandDescription, responseJson);

                var url = responseJson["url"] == null ? "" : (string) responseJson["url"];
                onSuccess?.Invoke(url);
                OnCommandEnd?.Invoke();
            } catch (Exception e) {
                TryLogError(commandDescription, "Unexpected error: " + e.Message, onError);
                OnCommandEnd?.Invoke();
            }
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


        private static void EnforceMode(HandyMode mode, Action command, Action<string> onError)
        {
            if (Mode != mode) {
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
            if (responseJson["success"] == null) error = "Invalid response";
            if (!responseJson["success"].AsBool) error = responseJson["error"] == null 
                ? "Unknown error" 
                : (string)responseJson["error"];

            if (string.IsNullOrEmpty(error)) {
                return false;
            }

            TryLogError(commandDescription, error, onError);
            return true;
        }

        private static async Task<string> GetAsync(string uri)
        {
            var response = await Client.GetAsync(uri);
            try {
                response.EnsureSuccessStatusCode();
            } catch (HttpRequestException e) {
                return "{success: false, error: " + e.Message + "}";
            }

            return await response.Content.ReadAsStringAsync();
        }

        private static async Task<string> PostAsync(string uri, string fileName, Stream file)
        {
            var content = new MultipartFormDataContent
            {
                {
                    new StreamContent(file), "syncFile", fileName
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

            return url;
        }
    }
}