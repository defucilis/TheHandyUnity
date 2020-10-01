using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace Defucilis.TheHandyUnity
{
    /// <summary>
    /// This class contains examples of how to use every function in the API
    /// For ease of exploration, all functions have been made accessible through the Unity UI
    /// </summary>
    public class HandyExampleUsageUI : MonoBehaviour
    {
        public HandyLogMode LogMode = HandyLogMode.Verbose;

        private InputField _connectionKeyInputField;
        private Text _errorText;

        public void Awake()
        {
            //Normally, this would be left off, but you can enable more logs to get a better idea of what's going on
            HandyConnection.LogMode = LogMode;
            
            //Tell the loading widget to show whenever we start a command, and hide whenever we end one
            //We also show a full-screen low-opacity image to indicate that no more commands can be input during this time
            var loader = transform.Find("Header/LoadingWidget").GetComponent<LoadingWidget>();
            var blocker = transform.Find("Blocker").gameObject;
            blocker.GetComponent<Image>().enabled = true;
            blocker.SetActive(false);
            HandyConnection.OnCommandStart += () =>
            {
                loader.Show();
                blocker.SetActive(true);
            };
            HandyConnection.OnCommandEnd += () =>
            {
                loader.Hide();
                blocker.SetActive(false);
            };

            //Sometimes the Handy changes its own mode when necessary
            //(for example, entering automatic mode when you set the stroke speed)
            //For these situations, we want to subscribe to the OnStatusChanged event to make sure we update the UI
            HandyConnection.OnStatusChanged += newStatus =>
            {
                transform.Find("Body/Middle/ControlMode/Dropdown").GetComponent<Dropdown>().value = (int) newStatus;
            };

            //Load connection key from PlayerPrefs, if it exists (and display in the UI)
            _connectionKeyInputField = transform.Find("Body/Left/ConnectionKeyInput").GetComponent<InputField>();
            _connectionKeyInputField.onValueChanged.AddListener(newValue => HandyConnection.ConnectionKey = newValue);
            if (PlayerPrefs.HasKey("ConnectionKey")) {
                _connectionKeyInputField.text = PlayerPrefs.GetString("ConnectionKey");
                HandyConnection.ConnectionKey = _connectionKeyInputField.text;
            }
            
            
            
            
            //These three operations show how to get data out of the Handy
            //This is the best way to determine whether it's connected or not - if the onSuccess callback fires,
            //that means the Handy responded and is connected

            //Get Handy version
            transform.Find("Body/Left/GetVersion/Button").GetComponent<Button>().onClick.AddListener(() =>
            {
                SetError("");

                HandyConnection.GetVersion(data =>
                {
                    transform.Find("Body/Left/GetVersion").GetComponent<InputField>().text =
                        $"Current Version: {data.CurrentVersion}\n" +
                        $"Latest Version: {data.LatestVersion}";
                }, SetError);
            });
            
            //Get Handy settings
            transform.Find("Body/Left/GetSettings/Button").GetComponent<Button>().onClick.AddListener(() =>
            {
                SetError("");

                HandyConnection.GetSettings(data =>
                {
                    transform.Find("Body/Left/GetSettings").GetComponent<InputField>().text =
                        $"Mode: {data.Mode}\n" +
                        $"Position: {data.CurrentPosition}\n" +
                        $"Speed: {data.Speed}\n" +
                        $"Stroke: {data.Stroke}";
                }, SetError);
            });
            
            //Get Handy status
            transform.Find("Body/Left/GetStatus/Button").GetComponent<Button>().onClick.AddListener(() =>
            {
                SetError("");

                HandyConnection.GetStatus(data =>
                {
                    transform.Find("Body/Left/GetStatus").GetComponent<InputField>().text = $"Mode: {data}";
                }, SetError);
            });
            
            
            
            
            
            
            //These operations are for controlling the Handy directly, setting speed, stroke, etc
            
            //Set Handy mode
            transform.Find("Body/Middle/ControlMode/Set").GetComponent<Button>().onClick.AddListener(() =>
            {
                SetError("");
                HandyConnection.SetMode((HandyMode)transform.Find("Body/Middle/ControlMode/Dropdown").GetComponent<Dropdown>().value, 
                    null, SetError);
            });
            
            //Toggle Handy mode
            transform.Find("Body/Middle/ControlMode/Toggle").GetComponent<Button>().onClick.AddListener(() =>
            {
                SetError("");
                HandyConnection.ToggleMode((HandyMode)transform.Find("Body/Middle/ControlMode/Dropdown").GetComponent<Dropdown>().value, 
                    null, SetError);
            });
            
            //Set Handy speed
            var speedPercent = transform.Find("Body/Middle/SetSpeedPercent").GetComponent<Slider>();
            var speedPercentNumber = transform.Find("Body/Middle/SetSpeedPercent/InputField").GetComponent<InputField>();
            var speedMm = transform.Find("Body/Middle/SetSpeedMm").GetComponent<Slider>();
            var speedMmNumber = transform.Find("Body/Middle/SetSpeedMm/InputField").GetComponent<InputField>();
            
            //As percentage
            speedPercent.onValueChanged.AddListener(newValue => speedPercentNumber.text = newValue.ToString("0"));
            transform.Find("Body/Middle/SetSpeedPercent/Set").GetComponent<Button>().onClick.AddListener(() =>
            {
                SetError("");
                HandyConnection.SetSpeedPercent((int)speedPercent.value, null, SetError);
            });
            
            //As mm
            speedMm.onValueChanged.AddListener(newValue => speedMmNumber.text = newValue.ToString("0"));
            transform.Find("Body/Middle/SetSpeedMm/Set").GetComponent<Button>().onClick.AddListener(() =>
            {
                SetError("");
                HandyConnection.SetSpeed((int)speedMm.value, null, SetError);
            });
            
            //Step Handy speed up and down
            transform.Find("Body/Middle/StepSpeed/Up").GetComponent<Button>().onClick.AddListener(() =>
            {
                SetError("");
                HandyConnection.StepSpeedUp(data =>
                {
                    speedPercent.value = data.PercentageValue;
                    speedPercentNumber.text = data.PercentageValue.ToString("0");
                    speedMm.value = data.RawValue;
                    speedMmNumber.text = data.RawValue.ToString("0");
                }, SetError);
            });
            transform.Find("Body/Middle/StepSpeed/Down").GetComponent<Button>().onClick.AddListener(() =>
            {
                SetError("");
                HandyConnection.StepSpeedDown(data => {
                    speedPercent.value = data.PercentageValue;
                    speedPercentNumber.text = data.PercentageValue.ToString("0");
                    speedMm.value = data.RawValue;
                    speedMmNumber.text = data.RawValue.ToString("0");
                }, SetError);
            });
            
            //Set Handy stroke (as percentage)
            var strokePercent = transform.Find("Body/Middle/SetStrokePercent").GetComponent<Slider>();
            var strokePercentNumber = transform.Find("Body/Middle/SetStrokePercent/InputField").GetComponent<InputField>();
            strokePercent.onValueChanged.AddListener(newValue => strokePercentNumber.text = newValue.ToString("0"));
            transform.Find("Body/Middle/SetStrokePercent/Set").GetComponent<Button>().onClick.AddListener(() =>
            {
                SetError("");
                HandyConnection.SetStrokePercent((int)strokePercent.value, null, SetError);
            });
            
            //Set Handy stroke (as mm)
            var strokeMm = transform.Find("Body/Middle/SetStrokeMm").GetComponent<Slider>();
            var strokeMmNumber = transform.Find("Body/Middle/SetStrokeMm/InputField").GetComponent<InputField>();
            strokeMm.onValueChanged.AddListener(newValue => strokeMmNumber.text = newValue.ToString("0"));
            transform.Find("Body/Middle/SetStrokeMm/Set").GetComponent<Button>().onClick.AddListener(() =>
            {
                SetError("");
                HandyConnection.SetStroke((int)strokeMm.value, null, SetError);
            });
            
            //Step Handy stroke up and down
            transform.Find("Body/Middle/StepStroke/Up").GetComponent<Button>().onClick.AddListener(() =>
            {
                SetError("");
                HandyConnection.StepStrokeUp(data => {
                    strokePercent.value = data.PercentageValue;
                    strokePercentNumber.text = data.PercentageValue.ToString("0");
                    strokeMm.value = data.RawValue;
                    strokeMmNumber.text = data.RawValue.ToString("0");
                }, SetError);
            });
            transform.Find("Body/Middle/StepStroke/Down").GetComponent<Button>().onClick.AddListener(() =>
            {
                SetError("");
                HandyConnection.StepStrokeDown(data => {
                    strokePercent.value = data.PercentageValue;
                    strokePercentNumber.text = data.PercentageValue.ToString("0");
                    strokeMm.value = data.RawValue;
                    strokeMmNumber.text = data.RawValue.ToString("0");
                }, SetError);
            });
            
            
            
            
            //These operations are for synchronizing the Handy with funscripts, CSV, or other sequenced stroke patterns
            
            //Get Server Time
            transform.Find("Body/Left/GetServerTime/Load").GetComponent<Button>().onClick.AddListener(() => {
                SetError("");
                HandyConnection.GetServerTime(30, data => {
                    transform.Find("Body/Left/GetServerTime").GetComponent<InputField>().text = data.ToString("0");
                }, SetError);
            });
            
            //Sync Prepare (with pre-existing CSV url)
            //To test, try the example CSV from the API docs:
            //        https://sweettecheu.s3.eu-central-1.amazonaws.com/scripts/admin/dataset.csv
            transform.Find("Body/Right/SyncPrepare/Send").GetComponent<Button>().onClick.AddListener(() => {
                SetError("");
                HandyConnection.SyncPrepare(transform.Find("Body/Right/SyncPrepare").GetComponent<InputField>().text,
                    "", -1, null, SetError);
            });
            
            //Sync Play/Pause
            transform.Find("Body/Right/SyncPlayback/Play").GetComponent<Button>().onClick.AddListener(() => {
                SetError("");
                HandyConnection.SyncPlay(0, null, SetError);
            });
            transform.Find("Body/Right/SyncPlayback/Pause").GetComponent<Button>().onClick.AddListener(() => {
                SetError("");
                HandyConnection.SyncPause(null, SetError);
            });
            
            //Sync Offset
            var syncOffset = transform.Find("Body/Right/SyncOffset").GetComponent<Slider>();
            var syncOffsetValue = transform.Find("Body/Right/SyncOffset/InputField").GetComponent<InputField>();
            syncOffset.onValueChanged.AddListener(newValue => syncOffsetValue.text = newValue.ToString("0"));
            transform.Find("Body/Right/SyncOffset/Set").GetComponent<Button>().onClick.AddListener(() =>
            {
                SetError("");
                HandyConnection.SyncOffset((int)syncOffset.value, null, SetError);
            });
            
            //Generate a CSV file on the handyfeeling.com servers from a generated list of position/time pairs
            transform.Find("Body/Right/CreatePattern/Generate").GetComponent<Button>().onClick.AddListener(() => {
                SetError("");
                HandyConnection.PatternToUrl(GetPattern(
                    transform.Find("Body/Right/CreatePattern/TypeDropdown").GetComponent<Dropdown>().value,
                    (int)transform.Find("Body/Right/CreatePattern/RepeatsSlider").GetComponent<Slider>().value,
                    (int)transform.Find("Body/Right/CreatePattern/SpeedSlider").GetComponent<Slider>().value
                ), url => transform.Find("Body/Right/CreatePattern/Url").GetComponent<InputField>().text = url,
                    SetError);
            });
            var repeats = transform.Find("Body/Right/CreatePattern/RepeatsSlider").GetComponent<Slider>();
            var repeatsValue = transform.Find("Body/Right/CreatePattern/RepeatsSlider/InputField").GetComponent<InputField>();
            repeats.onValueChanged.AddListener(newValue => repeatsValue.text = newValue.ToString("0"));
            var speed = transform.Find("Body/Right/CreatePattern/SpeedSlider").GetComponent<Slider>();
            var speedValue = transform.Find("Body/Right/CreatePattern/SpeedSlider/InputField").GetComponent<InputField>();
            speed.onValueChanged.AddListener(newValue => speedValue.text = newValue.ToString("0"));
            
            //Upload a .funscript or .csv file to the handyfeeling.com servers
            transform.Find("Body/Right/UploadFile/Upload").GetComponent<Button>().onClick.AddListener(() =>  {
                var path = transform.Find("Body/Right/UploadFile").GetComponent<InputField>().text;
                var fileContents = "";
                var fileName = "";
                var extension = "";
                try {
                    var file = new FileInfo(path);
                    fileName = file.Name;
                    extension = file.Extension;
                    var reader = new StreamReader(path);
                    fileContents = reader.ReadToEnd();
                    reader.Close();
                } catch (Exception e) {
                    SetError("Failed to read file - " + e.Message);
                    return;
                }

                if (extension == ".csv") {
                    HandyConnection.CsvToUrl(
                        fileContents,
                        fileName,
                        url => transform.Find("Body/Right/CreatePattern/Url").GetComponent<InputField>().text = url,
                        SetError
                    );
                } else if (extension == ".funscript") {
                    HandyConnection.FunscriptToUrl(
                        fileContents,
                        fileName,
                        url => transform.Find("Body/Right/CreatePattern/Url").GetComponent<InputField>().text = url,
                        SetError
                    );
                } else {
                    SetError("Invalid file selected");
                }
            });
        }

        //This is an example showing how to generate patterns to be converted into CSV for playback
        //It just creates repeating strokes that match a particular tempo and pattern
        //You can create any kind of pattern you like, so long as it comes as an array of Vector2Ints
        //The X value is the time, in milliseconds, from the beginning of the pattern
        //The Y value is the stroke position. Note that this is multiplied by the stroke setting on the Handy!
        //So if someone has Stroke set to 60% on their Handy, a value of 50 in your pattern will end up being position = 30%
        private Vector2Int[] GetPattern(int patternType, int repeats, int bpm)
        {
            var commandList = new List<Vector2Int>();

            //length of a bar in milliseconds
            var barTime = (int)(4000f * (60f / bpm));
            
            for (var i = 0; i < repeats; i++) {
                var startTime = i * barTime;
                switch (patternType) {
                    case 0: //slow
                        commandList.Add(new Vector2Int((int)(startTime + barTime * (0f / 4f)), 0));
                        commandList.Add(new Vector2Int((int)(startTime + barTime * (1f / 4f)), 100));
                        commandList.Add(new Vector2Int((int)(startTime + barTime * (2f / 4f)), 0));
                        commandList.Add(new Vector2Int((int)(startTime + barTime * (3f / 4f)), 100));
                        break;
                    case 1: //123 slow
                        commandList.Add(new Vector2Int((int)(startTime + barTime * (0f / 8f)), 0));
                        commandList.Add(new Vector2Int((int)(startTime + barTime * (1f / 8f)), 100));
                        commandList.Add(new Vector2Int((int)(startTime + barTime * (2f / 8f)), 0));
                        commandList.Add(new Vector2Int((int)(startTime + barTime * (3f / 8f)), 100));
                        commandList.Add(new Vector2Int((int)(startTime + barTime * (4f / 8f)), 0));
                        commandList.Add(new Vector2Int((int)(startTime + barTime * (5f / 8f)), 100));
                        
                        commandList.Add(new Vector2Int((int)(startTime + barTime * (7f / 8f)), 100));
                        break;
                    case 2: //normal
                        commandList.Add(new Vector2Int((int)(startTime + barTime * (0f / 8f)), 0));
                        commandList.Add(new Vector2Int((int)(startTime + barTime * (1f / 8f)), 100));
                        commandList.Add(new Vector2Int((int)(startTime + barTime * (2f / 8f)), 0));
                        commandList.Add(new Vector2Int((int)(startTime + barTime * (3f / 8f)), 100));
                        commandList.Add(new Vector2Int((int)(startTime + barTime * (4f / 8f)), 0));
                        commandList.Add(new Vector2Int((int)(startTime + barTime * (5f / 8f)), 100));
                        commandList.Add(new Vector2Int((int)(startTime + barTime * (6f / 8f)), 0));
                        commandList.Add(new Vector2Int((int)(startTime + barTime * (7f / 8f)), 100));
                        break;
                    case 3: //heartbeat
                        commandList.Add(new Vector2Int((int)(startTime + barTime * (0f / 16f)), 0));
                        commandList.Add(new Vector2Int((int)(startTime + barTime * (1f / 16f)), 100));
                        commandList.Add(new Vector2Int((int)(startTime + barTime * (2f / 16f)), 0));
                        commandList.Add(new Vector2Int((int)(startTime + barTime * (3f / 16f)), 100));
                        
                        commandList.Add(new Vector2Int((int)(startTime + barTime * (7f / 16f)), 100));
                        
                        commandList.Add(new Vector2Int((int)(startTime + barTime * (8f / 16f)), 0));
                        commandList.Add(new Vector2Int((int)(startTime + barTime * (9f / 16f)), 100));
                        commandList.Add(new Vector2Int((int)(startTime + barTime * (10f / 16f)), 0));
                        commandList.Add(new Vector2Int((int)(startTime + barTime * (11f / 16f)), 100));
                        
                        commandList.Add(new Vector2Int((int)(startTime + barTime * (15f / 16f)), 100));
                        break;
                    case 4: //123 Fast
                        commandList.Add(new Vector2Int((int)(startTime + barTime * (0f / 16f)), 0));
                        commandList.Add(new Vector2Int((int)(startTime + barTime * (1f / 16f)), 100));
                        commandList.Add(new Vector2Int((int)(startTime + barTime * (2f / 16f)), 0));
                        commandList.Add(new Vector2Int((int)(startTime + barTime * (3f / 16f)), 100));
                        commandList.Add(new Vector2Int((int)(startTime + barTime * (4f / 16f)), 0));
                        commandList.Add(new Vector2Int((int)(startTime + barTime * (5f / 16f)), 100));
                        
                        commandList.Add(new Vector2Int((int)(startTime + barTime * (7f / 16f)), 100));
                        
                        commandList.Add(new Vector2Int((int)(startTime + barTime * (8f / 16f)), 0));
                        commandList.Add(new Vector2Int((int)(startTime + barTime * (9f / 16f)), 100));
                        commandList.Add(new Vector2Int((int)(startTime + barTime * (10f / 16f)), 0));
                        commandList.Add(new Vector2Int((int)(startTime + barTime * (11f / 16f)), 100));
                        commandList.Add(new Vector2Int((int)(startTime + barTime * (12f / 16f)), 0));
                        commandList.Add(new Vector2Int((int)(startTime + barTime * (13f / 16f)), 100));
                        
                        commandList.Add(new Vector2Int((int)(startTime + barTime * (15f / 16f)), 100));
                        break;
                    case 5: //1234567
                        commandList.Add(new Vector2Int((int)(startTime + barTime * (0f / 16f)), 0));
                        commandList.Add(new Vector2Int((int)(startTime + barTime * (1f / 16f)), 100));
                        commandList.Add(new Vector2Int((int)(startTime + barTime * (2f / 16f)), 0));
                        commandList.Add(new Vector2Int((int)(startTime + barTime * (3f / 16f)), 100));
                        commandList.Add(new Vector2Int((int)(startTime + barTime * (4f / 16f)), 0));
                        commandList.Add(new Vector2Int((int)(startTime + barTime * (5f / 16f)), 100));
                        commandList.Add(new Vector2Int((int)(startTime + barTime * (6f / 16f)), 0));
                        commandList.Add(new Vector2Int((int)(startTime + barTime * (7f / 16f)), 100));
                        commandList.Add(new Vector2Int((int)(startTime + barTime * (8f / 16f)), 0));
                        commandList.Add(new Vector2Int((int)(startTime + barTime * (9f / 16f)), 100));
                        commandList.Add(new Vector2Int((int)(startTime + barTime * (10f / 16f)), 0));
                        commandList.Add(new Vector2Int((int)(startTime + barTime * (11f / 16f)), 100));
                        commandList.Add(new Vector2Int((int)(startTime + barTime * (12f / 16f)), 0));
                        commandList.Add(new Vector2Int((int)(startTime + barTime * (13f / 16f)), 100));
                        
                        commandList.Add(new Vector2Int((int)(startTime + barTime * (15f / 16f)), 100));
                        break;
                    case 6: //double time
                        commandList.Add(new Vector2Int((int)(startTime + barTime * (0f / 16f)), 0));
                        commandList.Add(new Vector2Int((int)(startTime + barTime * (1f / 16f)), 100));
                        commandList.Add(new Vector2Int((int)(startTime + barTime * (2f / 16f)), 0));
                        commandList.Add(new Vector2Int((int)(startTime + barTime * (3f / 16f)), 100));
                        commandList.Add(new Vector2Int((int)(startTime + barTime * (4f / 16f)), 0));
                        commandList.Add(new Vector2Int((int)(startTime + barTime * (5f / 16f)), 100));
                        commandList.Add(new Vector2Int((int)(startTime + barTime * (6f / 16f)), 0));
                        commandList.Add(new Vector2Int((int)(startTime + barTime * (7f / 16f)), 100));
                        commandList.Add(new Vector2Int((int)(startTime + barTime * (8f / 16f)), 0));
                        commandList.Add(new Vector2Int((int)(startTime + barTime * (9f / 16f)), 100));
                        commandList.Add(new Vector2Int((int)(startTime + barTime * (10f / 16f)), 0));
                        commandList.Add(new Vector2Int((int)(startTime + barTime * (11f / 16f)), 100));
                        commandList.Add(new Vector2Int((int)(startTime + barTime * (12f / 16f)), 0));
                        commandList.Add(new Vector2Int((int)(startTime + barTime * (13f / 16f)), 100));
                        commandList.Add(new Vector2Int((int)(startTime + barTime * (14f / 16f)), 0));
                        commandList.Add(new Vector2Int((int)(startTime + barTime * (15f / 16f)), 100));
                        break;
                    default:
                        throw new FormatException("Unexpected value " + patternType + " for pattern type!");
                }
            }

            return commandList.ToArray();
        }

        private void SetError(string error)
        {
            if (_errorText == null) _errorText = transform.Find("Footer/ErrorText").GetComponent<Text>();
            _errorText.text = error;
        }
    }
}