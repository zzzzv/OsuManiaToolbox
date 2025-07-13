using System.Data;
using System.Windows.Controls;

namespace OsuManiaToolbox.Core.Services;

public interface ITableService
{
    DataTable Create(IEnumerable<BeatmapData> beatmaps);
    void SetupColumns(DataGrid dataGridView);
}