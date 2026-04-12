#region

using Core.Reactive;

#endregion

namespace Core.Data
{
    public class LevelUpData : BasePanelData
    {
        public ReactiveValue<int> CurrentLevel { get; }

        public LevelUpData(ReactiveValue<int> level)
        {
            CurrentLevel = level;
        }
    }
}