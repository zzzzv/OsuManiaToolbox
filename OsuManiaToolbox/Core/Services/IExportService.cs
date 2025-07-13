namespace OsuManiaToolbox.Core.Services;

public interface IExportService
{
    bool ExportToCsv(IEnumerable<BeatmapData> beatmaps, string fileName);
    
    bool CreateCollection(IEnumerable<string> beatmapsMd5, string collectionName);
}