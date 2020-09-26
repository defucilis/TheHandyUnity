using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Defucilis.TheHandyUnity
{
    [RequireComponent(typeof(CanvasGroup))]
    public class LoadingWidget : MonoBehaviour
    {
        [Range(0f, 2500f)] public float SpinRateAmplitude = 2000f;
        [Range(1f, 10f)] public float SpinRatePeriod = 6f;
        [Range(0f, 1f)] public float TargetAlpha = 0f;
        [Range(2f, 16f)] public float AlphaLerpRate = 12f;

        private CanvasGroup _canvasGroup;
        private float _alpha;
        private float _t;
        
        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            _canvasGroup.alpha = _alpha;
        }

        private void Update()
        {
            _alpha = Mathf.Lerp(_alpha, TargetAlpha, Time.deltaTime * AlphaLerpRate);
            _canvasGroup.alpha = _alpha;
            
            transform.Rotate(Vector3.up, Time.deltaTime * SpinRateAmplitude * Mathf.Sin((_t * Mathf.PI * 2f) / SpinRatePeriod));
            _t += Time.deltaTime;
        }

        public void Show()
        {
            _t = 0f;
            TargetAlpha = 1f;
        }

        public void Hide()
        {
            TargetAlpha = 0f;
        }
    }
}