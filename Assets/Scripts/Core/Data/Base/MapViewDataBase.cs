using Core.Reactive;

namespace Core.Data
{
    public class MapViewDataBase : BasePanelData
    {
        public ReactiveValue<bool> IsVisible { get; } = new(false);
    }
}
