using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class ExtendedObservableCollection<T> : ObservableCollection<T>
{
    public ExtendedObservableCollection()
    {
    }


    public void AddRange(IEnumerable<T> range)
    {
        var rangeList = range as IList<T> ?? range.ToList();
        if (rangeList.Count == 0) { return; }
        if (rangeList.Count == 1)
        {
            Add(rangeList[0]);
            return;
        }
        foreach (var item in rangeList)
        {
            Items.Add(item);
        }
    }

    public void Reset(IEnumerable<T> range)
    {
        ClearItems();
        AddRange(range);
    }
}
