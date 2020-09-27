using UnityEngine;
using UnityEngine.UI;

namespace Defucilis.TheHandyUnity
{
    /// <summary>
    /// This class contains a simple of example of how to use this API wrapper.
    /// For a more complete example, see the UI Example scene
    /// </summary>
    public class HandyKeyboardExampleUI : MonoBehaviour
    {
        public KeyCode SpeedUpKey = KeyCode.RightArrow;
        public KeyCode SpeedDownKey = KeyCode.LeftArrow;
        public KeyCode StrokeUpKey = KeyCode.UpArrow;
        public KeyCode StrokeDownKey = KeyCode.DownArrow;
        public KeyCode StartStopKey = KeyCode.Space;
        [Space(10)]
        public HandyLogMode LogMode = HandyLogMode.Verbose;

        private LoadingWidget _loadingWidget;
        private GameObject _inputBlocker;
        private InputField _connectionKeyInputField;
        private Button _connectButton;
        private InputField _modeInputField;
        private Slider _speedSlider;
        private Slider _strokeSlider;
        private Text _errorText;

        public void Awake()
        {
            //Normally, this would be left off, but you can enable more logs to get a better idea of what's going on
            HandyConnection.LogMode = LogMode;
            
            //Tell the loading widget to show whenever we start a command, and hide whenever we end one
            //We also show a full-screen low-opacity image to indicate that no more commands can be input during this time
            _inputBlocker = transform.Find("Blocker").gameObject;
            _inputBlocker.GetComponent<Image>().enabled = true;
            _inputBlocker.SetActive(false);
            HandyConnection.OnCommandStart += ShowLoader;
            HandyConnection.OnCommandEnd += HideLoader;

            //Update the mode slider whenever the Handy's mode changes
            HandyConnection.OnStatusChanged += SetModeDisplay;

            //Load connection key from PlayerPrefs, if it exists (and display in the UI)
            _connectionKeyInputField = transform.Find("Body/ConnectionKey").GetComponent<InputField>();
            if (PlayerPrefs.HasKey("ConnectionKey")) {
                _connectionKeyInputField.text = PlayerPrefs.GetString("ConnectionKey");
                HandyConnection.ConnectionKey = _connectionKeyInputField.text;
            }
        }

        public void Update()
        {
            //Change speed
            if (Input.GetKeyDown(SpeedUpKey)) {
                HandyConnection.StepSpeedUp(data => SetSpeedDisplay(data.PercentageValue), SetError);
            }
            if (Input.GetKeyDown(SpeedDownKey)) {
                HandyConnection.StepSpeedDown(data => SetSpeedDisplay(data.PercentageValue), SetError);
            }
            
            //Change stroke
            if (Input.GetKeyDown(StrokeUpKey)) {
                HandyConnection.StepStrokeUp(data => SetStrokeDisplay(data.PercentageValue), SetError);
            }
            if (Input.GetKeyDown(StrokeDownKey)) {
                HandyConnection.StepStrokeDown(data => SetStrokeDisplay(data.PercentageValue), SetError);
            }

            //Start / Stop
            if (Input.GetKeyDown(StartStopKey)) {
                HandyConnection.ToggleMode(HandyMode.Automatic, null, SetError);
            }
        }

        //When 'connect' is clicked, get the current status of the Handy and update the UI to show that status
        public void ConnectToHandy()
        {
            var connectionKey = _connectionKeyInputField.text;
            HandyConnection.GetSettings(data =>
            {
                SetModeDisplay(data.Mode);
                SetSpeedDisplay(data.Speed);
                SetStrokeDisplay(data.Stroke);
            }, SetError);
        }

        private void ShowLoader()
        {
            if (_loadingWidget == null) _loadingWidget = transform.Find("Header/LoadingWidget").GetComponent<LoadingWidget>();
            if (_inputBlocker == null) _inputBlocker = transform.Find("Blocker").gameObject;
            _loadingWidget.Hide();
            _inputBlocker.SetActive(true);
        }

        private void HideLoader()
        {
            if (_loadingWidget == null) _loadingWidget = transform.Find("Header/LoadingWidget").GetComponent<LoadingWidget>();
            if (_inputBlocker == null) _inputBlocker = transform.Find("Blocker").gameObject;
            _loadingWidget.Hide();
            _inputBlocker.SetActive(false);
        }

        private void SetModeDisplay(HandyMode mode)
        {
            if(_modeInputField == null) _modeInputField = transform.Find("Body/Mode").GetComponent<InputField>();
            _modeInputField.text = mode.ToString();
        }

        private void SetSpeedDisplay(float speed)
        {
            if(_speedSlider == null) _speedSlider = transform.Find("Body/Speed").GetComponent<Slider>();
            _speedSlider.value = speed;
        }

        private void SetStrokeDisplay(float stroke)
        {
            if(_strokeSlider == null) _strokeSlider = transform.Find("Body/Stroke").GetComponent<Slider>();
            _strokeSlider.value = stroke;
        }

        private void SetError(string error)
        {
            if (_errorText == null) _errorText = transform.Find("Footer/ErrorText").GetComponent<Text>();
            _errorText.text = error;
        }
    }
}