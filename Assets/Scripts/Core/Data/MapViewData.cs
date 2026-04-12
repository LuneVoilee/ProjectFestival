#region

using Core.Reactive;

#endregion

namespace Core.Data
{
    public class MapViewData : BasePanelData
    {
        public ReactiveValue<int> CurrentMode { get; }

        public MapViewData(ReactiveValue<int> currentMode)
        {
            CurrentMode = currentMode;
        }
    }
}