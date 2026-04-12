#region

using System.Collections;
using TMPro;
using Core.Data;
using UnityEngine;

#endregion

namespace UI
{
    public class EconomyPanel : BasePanel<EconomyPanelData>
    {
        [Header("UI 引用")] public TMP_Text MoneyText;
        public TMP_Text HappinessText;
        public RectTransform MoneyIconTarget;
        public RectTransform HappinessIconTarget;

        [Header("动画参数")] public float AnimationDuration = 0.5f;
        public float ScaleFactor = 1.5f;
        public Color IncreaseColor = Color.green;
        public Color DecreaseColor = Color.red;

        private Color m_OriginalMoneyColor;
        private Color m_OriginalHappinessColor;
        private Vector3 m_OriginalMoneyScale;
        private Vector3 m_OriginalHappinessScale;
        private Coroutine m_MoneyAnimCoroutine;
        private Coroutine m_HappinessAnimCoroutine;

        private bool m_IsFirstMoneyBind = true;
        private bool m_IsFirstHappinessBind = true;

        protected void Awake()
        {
            if (MoneyText != null)
            {
                m_OriginalMoneyColor = MoneyText.color;
                m_OriginalMoneyScale = MoneyText.transform.localScale;
                MoneyText.text = "0";
            }

            if (HappinessText != null)
            {
                m_OriginalHappinessColor = HappinessText.color;
                m_OriginalHappinessScale = HappinessText.transform.localScale;
                HappinessText.text = "0";
            }
        }

        protected override void OnBind()
        {
            m_IsFirstMoneyBind = true;
            m_IsFirstHappinessBind = true;

            Data.Money.Bind(OnMoneyChanged);
            Data.Happiness.Bind(OnHappinessChanged);
        }

        protected override void OnUnbind()
        {
            Data.Money.Unbind(OnMoneyChanged);
            Data.Happiness.Unbind(OnHappinessChanged);
        }

        private void OnMoneyChanged(float oldValue, float newValue)
        {
            if (Mathf.Approximately(oldValue, newValue))
            {
                return;
            }

            var isIncrease = newValue > oldValue;

            if (m_MoneyAnimCoroutine != null)
            {
                StopCoroutine(m_MoneyAnimCoroutine);
                MoneyText.transform.localScale = m_OriginalMoneyScale;
                MoneyText.color = m_OriginalMoneyColor;
            }

            MoneyText.text = newValue.ToString("F0");

            if (m_IsFirstMoneyBind)
            {
                m_IsFirstMoneyBind = false;
                return;
            }

            m_MoneyAnimCoroutine = StartCoroutine(
                AnimateText(MoneyText, isIncrease, m_OriginalMoneyColor, m_OriginalMoneyScale));
        }

        private void OnHappinessChanged(float oldValue, float newValue)
        {
            if (Mathf.Approximately(oldValue, newValue))
            {
                return;
            }

            var isIncrease = newValue > oldValue;

            if (m_HappinessAnimCoroutine != null)
            {
                StopCoroutine(m_HappinessAnimCoroutine);
                HappinessText.transform.localScale = m_OriginalHappinessScale;
                HappinessText.color = m_OriginalHappinessColor;
            }

            HappinessText.text = newValue.ToString("F0");

            if (m_IsFirstHappinessBind)
            {
                m_IsFirstHappinessBind = false;
                return;
            }

            m_HappinessAnimCoroutine = StartCoroutine(
                AnimateText(HappinessText, isIncrease, m_OriginalHappinessColor,
                    m_OriginalHappinessScale));
        }

        private IEnumerator AnimateText
        (
            TMP_Text textElement, bool isIncrease,
            Color originalColor, Vector3 originalScale
        )
        {
            var targetColor = isIncrease ? IncreaseColor : DecreaseColor;
            var targetScale = originalScale * ScaleFactor;

            float t = 0;
            while (t < AnimationDuration / 2)
            {
                t += Time.deltaTime;
                var progress = t / (AnimationDuration / 2);
                textElement.transform.localScale =
                    Vector3.Lerp(originalScale, targetScale, progress);
                textElement.color = Color.Lerp(originalColor, targetColor, progress);
                yield return null;
            }

            t = 0;
            while (t < AnimationDuration / 2)
            {
                t += Time.deltaTime;
                var progress = t / (AnimationDuration / 2);
                textElement.transform.localScale =
                    Vector3.Lerp(targetScale, originalScale, progress);
                textElement.color = Color.Lerp(targetColor, originalColor, progress);
                yield return null;
            }

            textElement.transform.localScale = originalScale;
            textElement.color = originalColor;
        }
    }
}