#region

using Core.Reactive;

#endregion

namespace Core.Data
{
    public class SettingsData : BasePanelData
    {
        public ReactiveValue<float> CharacterMovement;
        public ReactiveValue<float> CharacterZoom;
        public ReactiveValue<float> Difficulty;
        public ReactiveValue<float> Volume;
    }
}