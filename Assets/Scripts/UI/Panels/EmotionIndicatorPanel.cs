#region

using System.Collections;
using UnityEngine;
using UnityEngine.UI;

#endregion

namespace UI
{
    public class EmotionIndicatorPanel : BasePanel
    {
        public Image EmotionImage;
        public Sprite HappySprite;
        public Sprite UnhappySprite;
        public Vector3 PositionOffset = new(0, 2.5f, 0);
        public float Lifetime = 2f;
        public float FadeOutDuration = 0.5f;

        private Transform m_TargetToFollow;
        private RectTransform m_Rect;
        private Camera m_MainCamera;

        private void Awake()
        {
            m_Rect = transform as RectTransform;
            m_MainCamera = Camera.main;
        }

        public void Init(Transform target, bool isHappy)
        {
            m_TargetToFollow = target;
            EmotionImage.sprite = isHappy ? HappySprite : UnhappySprite;
            StartCoroutine(FadeOutAndDestroy());
        }

        private void LateUpdate()
        {
            if (!m_TargetToFollow)
            {
                Destroy(gameObject);
                return;
            }

            if (!m_MainCamera)
                return;

            var screenPos =
                m_MainCamera.WorldToScreenPoint(m_TargetToFollow.position + PositionOffset);
            m_Rect.position = screenPos;
        }

        private IEnumerator FadeOutAndDestroy()
        {
            yield return new WaitForSeconds(Lifetime - FadeOutDuration);

            var timer = 0f;
            var startColor = EmotionImage.color;

            while (timer < FadeOutDuration)
            {
                timer += Time.deltaTime;
                EmotionImage.color = new Color(
                    startColor.r,
                    startColor.g,
                    startColor.b,
                    Mathf.Lerp(startColor.a, 0f, timer / FadeOutDuration)
                );
                yield return null;
            }

            Destroy(gameObject);
        }
    }
}