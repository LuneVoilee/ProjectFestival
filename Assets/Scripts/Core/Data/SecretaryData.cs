#region

using System.Collections.Generic;
using Core.Reactive;

#endregion

namespace Core.Data
{
    public class SecretaryData : BasePanelData
    {
        public int ID { get; }
        public IReadOnlyList<string> Lines { get; }

        public ReactiveValue<int> CurrentIndex { get; } = new(-1);

        public bool IsFinished => CurrentIndex.Value >= Lines.Count;

        public string CurrentLine => IsFinished ? "" : Lines[CurrentIndex.Value];

        public SecretaryData(int id, List<string> lines)
        {
            ID = id;
            Lines = lines;
        }

        public void Next() => CurrentIndex.Value++;
    }
}