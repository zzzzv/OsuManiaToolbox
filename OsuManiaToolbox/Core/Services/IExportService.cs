using OsuParsers.Database.Objects;

namespace OsuManiaToolbox.Core.Services;

public interface IExportService
{
    bool ExportToCsv(IEnumerable<DbBeatmap> beatmaps, string fileName);
    
    bool CreateCollection(IEnumerable<DbBeatmap> beatmaps, string collectionName);
}