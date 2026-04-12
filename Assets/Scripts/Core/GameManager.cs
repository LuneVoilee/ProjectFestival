#region

using Core.Reactive;
using Tool;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

#endregion

namespace Core
{
    public class GameManager : PersistentSingletonMono<GameManager>
    {
        private bool m_bPaused;

        private float m_LastTimeScale = 1f;

        private ReactiveValue<float> m_TimeScale;

        private ReactiveValue<bool> m_StartGame;

        private ReactiveValue<bool> m_EndGame;

        protected override void Awake()
        {
            base.Awake();

            m_StartGame = new ReactiveValue<bool>();

            m_EndGame = new ReactiveValue<bool>();

            m_TimeScale = new ReactiveValue<float>(1f);

            GPEvents.GetTimeScale = () => m_TimeScale;

            GPEvents.GetMainMenuData = () => (m_StartGame, m_EndGame);
        }

        private void OnEnable()
        {
            m_TimeScale.Bind(InternalSetTimeScale);

            UIEvents.OnHopeStartGameAction += StartGame;
            UIEvents.OnHopeEndGameAction += QuitGame;

            UIEvents.OnHopeChangeTimeScaleAction += HandleChangeTimeScale;

            UIEvents.OnGoBackToMainMenuAction += GoBackToMainMenu;
        }

        private void OnDisable()
        {
            m_TimeScale?.Unbind(InternalSetTimeScale);

            UIEvents.OnHopeStartGameAction -= StartGame;
            UIEvents.OnHopeEndGameAction -= QuitGame;

            UIEvents.OnHopeChangeTimeScaleAction -= HandleChangeTimeScale;

            UIEvents.OnGoBackToMainMenuAction -= GoBackToMainMenu;
        }

        private void StartGame()
        {
            m_StartGame.Value = true;

            SceneManager.LoadSceneAsync("1");
        }

        private void GoBackToMainMenu()
        {
            m_StartGame.Value = false;
            m_EndGame.Value = false;

            SceneManager.LoadSceneAsync("0");
        }

        private void HandleChangeTimeScale(float v)
        {
            m_TimeScale.Value = v;
        }

        public void PauseGame(bool isPause)
        {
            m_TimeScale.Value = isPause ? 0f : m_LastTimeScale;
        }

        private void InternalSetTimeScale(float oldScale, float newScale)
        {
            m_LastTimeScale = oldScale;
            Time.timeScale = newScale;
        }

        public void QuitGame()
        {
            m_EndGame.Value = true;

#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}