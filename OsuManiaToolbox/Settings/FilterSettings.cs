using CommunityToolkit.Mvvm.ComponentModel;
using DynamicExpresso;
using OsuManiaToolbox.ViewModels;
using OsuParsers.Database.Objects;
using System.Collections.ObjectModel;
using OsuParsers.Enums;

namespace OsuManiaToolbox.Settings;

public partial class FilterHistoryItem : ObservableObject
{
    [ObservableProperty]
    private string _expression = string.Empty;

    [ObservableProperty]
    private string _collectionName = string.Empty;

    [ObservableProperty]
    private string _csvName = string.Empty;
}

public class FilterSettings : ObservableObject
{
    public ObservableCollection<FilterHistoryItem> History { get; set; } = [
        new FilterHistoryItem{Expression = "status==R && SR()>5 && SR()<SR(EZ)", CollectionName = "PP"},
        new FilterHistoryItem{Expression = "status==R && SR(HT)>5 && SR(HT)<SR(HT&EZ)", CollectionName = "PP HT"},
        new FilterHistoryItem{Expression = "Acc>93 && Acc<96", CollectionName="93-96"},
        new FilterHistoryItem{Expression = "LN/(LN+N)>0.5", CollectionName="LN>50%"},
        new FilterHistoryItem{Expression = "AccDays>30", CollectionName="超过30天"},
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
