#region

using System.Collections.Generic;
using Core;
using Core.Data;
using UnityEngine;
using UnityEngine.UI;

#endregion

namespace UI
{
    public class TimeScalePanel : BasePanel<TimeScaleData>
    {
        //0倍速
        public Button Scale1Btn;

        //0.5倍速
        public Button Scale2Btn;

        //1倍速
        public Button Scale3Btn;

        //2倍速
        public Button Scale4Btn;

        //4倍速
        public Button Scale5Btn;

        private Dictionary<float, Button> m_Dict;

        private void Awake()
        {
            m_Dict = new Dictionary<float, Button>
            {
                { 0f, Scale1Btn },
                { 0.5f, Scale2Btn },
                { 1f, Scale3Btn },
                { 2f, Scale4Btn },
                { 4f, Scale5Btn }
            };
        }

        private void OnEnable()
        {
            Scale1Btn.onClick.AddListener(() =>
            {
                UIEvents.OnHopeChangeTimeScaleAction?.Invoke(0f);
            });
            Scale2Btn.onClick.AddListener(() =>
            {
                UIEvents.OnHopeChangeTimeScaleAction?.Invoke(0.5f);
            });
            Scale3Btn.onClick.AddListener(() =>
            {
                UIEvents.OnHopeChangeTimeScaleAction?.Invoke(1f);
            });
            Scale4Btn.onClick.AddListener(() =>
            {
                UIEvents.OnHopeChangeTimeScaleAction?.Invoke(2f);
            });
            Scale5Btn.onClick.AddListener(() =>
            {
                UIEvents.OnHopeChangeTimeScaleAction?.Invoke(4f);
            });
        }

        private void OnDisable()
        {
            Scale1Btn.onClick.RemoveAllListeners();
            Scale2Btn.onClick.RemoveAllListeners();
            Scale3Btn.onClick.RemoveAllListeners();
            Scale4Btn.onClick.RemoveAllListeners();
            Scale5Btn.onClick.RemoveAllListeners();
        }

        protected override void OnBind()
        {
            Data.TimeScale.Bind(HandleHightLight);
        }

        protected override void OnUnbind()
        {
            Data.TimeScale.Unbind(HandleHightLight);
        }

        private void HandleHightLight(float old, float now)
        {
            SetHightLight(m_Dict[old], false);
            SetHightLight(m_Dict[now], true);
        }

        private void SetHightLight(Button btn, bool isHighLight)
        {
            btn.image.color = isHighLight ? Color.yellow : Color.white;
        }
    }
}