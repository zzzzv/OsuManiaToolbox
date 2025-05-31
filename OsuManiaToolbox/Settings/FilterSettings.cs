using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace OsuManiaToolbox.Settings;

public partial class FilterHistoryItem : ObservableObject
{
    [ObservableProperty]
    private string _expression = string.Empty;

    [ObservableProperty]
    private string _orderBy = string.Empty;

    [ObservableProperty]
    private int _skip = 0;

    [ObservableProperty]
    private int _take = 0;

    [ObservableProperty]
    private string _collectionName = string.Empty;

    [ObservableProperty]
    private string _csvName = string.Empty;
}

public class FilterSettings : ObservableObject
{
    public ObservableCollection<FilterHistoryItem> History { get; set; } = [
        new FilterHistoryItem{Expression = "status==R && SR()>6 && SR()-SR(EZ)<0.2", CollectionName = "PP"},
        new FilterHistoryItem{Expression = "status==R && SR(HT)>6 && SR(HT)-SR(HT|EZ)<0.2", CollectionName = "PP HT"},
        new FilterHistoryItem{Expression = "status==R && SR(DT)>6 && SR(DT)-SR(DT|EZ)<0.2", CollectionName = "PP DT"},
        new FilterHistoryItem{Expression = "AccDays>30", CollectionName="超过30天", OrderBy="-AccDays", Take=10},
        new FilterHistoryItem{Expression = "Acc>93 && Acc<96", CollectionName="93-96"},
        new FilterHistoryItem{Expression = "LN/(LN+N)>0.5", CollectionName="LN>50%"},
    ];

    public FilterHistoryItem MoveFirst(FilterHistoryItem item)
    {
        if (History.Contains(item))
        {
            History.Remove(item);
            History.Insert(0, item);
        }
        return item;
    }
}
