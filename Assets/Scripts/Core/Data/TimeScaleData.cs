#region

using Core.Reactive;

#endregion

namespace Core.Data
{
    public class TimeScaleData : BasePanelData
    {
        public ReactiveValue<float> TimeScale;

        public TimeScaleData(ReactiveValue<float> timeScale)
        {
            TimeScale = timeScale;
        }
    }
}