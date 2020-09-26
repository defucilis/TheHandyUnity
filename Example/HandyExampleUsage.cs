using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Defucilis.TheHandyUnity
{
    public class HandyExampleUsage : MonoBehaviour
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
                transform.Find("Body/Middle/SetMode").GetComponent<Dropdown>().value = (int) newStatus;
            };

            //Load connection key from PlayerPrefs, if it exists (and display in the UI)
            _connectionKeyInputField = transform.Find("Body/Left/ConnectionKeyInput").GetComponent<InputField>();
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
                        $"Mode: {data.Status}\n" +
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
            transform.Find("Body/Middle/SetMode/Set").GetComponent<Button>().onClick.AddListener(() =>
            {
                SetError("");
                HandyConnection.SetMode((HandyStatus)transform.Find("Body/Middle/SetMode").GetComponent<Dropdown>().value, 
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
        }

        private void SetError(string error)
        {
            if (_errorText == null) _errorText = transform.Find("Footer/ErrorText").GetComponent<Text>();
            _errorText.text = error;
        }
    }
}