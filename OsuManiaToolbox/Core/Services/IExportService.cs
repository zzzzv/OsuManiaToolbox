namespace OsuManiaToolbox.Core.Services;

public interface IExportService
{
    bool ExportToCsv(IEnumerable<BeatmapData> beatmaps, string fileName, TableCreator tableCreator);
    
    bool CreateCollection(IEnumerable<string> beatmapsMd5, string collectionName);
}