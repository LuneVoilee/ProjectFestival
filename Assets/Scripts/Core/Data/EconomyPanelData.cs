#region

using Core.Reactive;

#endregion

namespace Core.Data
{
    public class EconomyPanelData : BasePanelData
    {
        public ReactiveValue<float> Money { get; }

        public ReactiveValue<float> Happiness { get; }


        public EconomyPanelData(ReactiveValue<float> money, ReactiveValue<float> happiness)
        {
            Money = money;
            Happiness = happiness;
        }
    }
}