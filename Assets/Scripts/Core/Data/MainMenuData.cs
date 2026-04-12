#region

using Core.Reactive;

#endregion

namespace Core.Data
{
    public class MainMenuData : BasePanelData
    {
        public ReactiveValue<bool> StartGame { get; }
        public ReactiveValue<bool> EndGame { get; }

        public MainMenuData(ReactiveValue<bool> s, ReactiveValue<bool> e)
        {
            StartGame = s;
            EndGame = e;
        }
    }
}