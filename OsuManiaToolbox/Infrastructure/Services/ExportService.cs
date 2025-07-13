using CsvHelper;
using OsuManiaToolbox.Core;
using OsuManiaToolbox.Core.Services;
using OsuParsers.Database.Objects;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;

namespace OsuManiaToolbox.Infrastructure.Services;

public class ExportService : IExportService
{
    private readonly ICollectionDbService _collectionDb;
    private readonly ILogger _logger;
    private readonly ITableService _tableService;

    public ExportService(ICollectionDbService collectionDb, ILogService logService, ITableService tableService)
    {
        _collectionDb = collectionDb;
        _logger = logService.GetLogger(this);
        _tableService = tableService;
    }

    public bool ExportToCsv(IEnumerable<BeatmapData> beatmaps, string fileName)
    {
        try
        {
            if (string.IsNullOrEmpty(fileName))
            {
                _logger.Error("文件名不能为空");
                return false;
            }
            
            if (!beatmaps.Any())
            {
                _logger.Info("没有可导出的谱面");
                return false;
            }
            
            var file = fileName + ".csv";
            if (File.Exists(file))
            {
                _logger.Warning($"文件 {file} 已存在，自动覆盖");
                File.Delete(file);
            }

            var table = _tableService.Create(beatmaps);
            using (var stream = File.OpenWrite(file))
            using (var writer = new StreamWriter(stream, Encoding.UTF8))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                foreach (DataColumn column in table.Columns)
                {
                    csv.WriteField(column.ColumnName);
                }
                csv.NextRecord();

                foreach (DataRow row in table.Rows)
                {
                    for (int i = 0; i < table.Columns.Count; i++)
                    {
                        csv.WriteField(row[i]);
                    }
                    csv.NextRecord();
                }
            }
            
            _logger.Info($"已导出 {table.Rows.Count} 行谱面信息到 {file}");

            var absolutePath = Path.GetFullPath(file);
            var process = new Process();
            process.StartInfo.FileName = absolutePath;
            process.StartInfo.UseShellExecute = true;
            process.Start();
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.Exception(ex);
            return false;
        }
    }

    public bool CreateCollection(IEnumerable<string> beatmapsMd5, string collectionName)
    {
        try
        {
            if (string.IsNullOrEmpty(collectionName))
            {
                _logger.Error("收藏夹名称不能为空");
                return false;
            }
            
            if (!beatmapsMd5.Any())
            {
                _logger.Info("没有符合条件的谱面");
                return false;
            }
            
            var collection = new Collection
            {
                Name = collectionName,
            };
            collection.MD5Hashes.AddRange(beatmapsMd5);
            collection.Count = collection.MD5Hashes.Count;

            if (_collectionDb.Index.ContainsKey(collection.Name))
            {
                _logger.Warning($"收藏夹 {collection.Name} 已存在，自动覆盖");
                _collectionDb.Remove(collection.Name);
            }
            
            _collectionDb.Add(collection);
            _logger.Info($"创建收藏夹 {collection.Name}，包含 {collection.Count} 张谱面");
            _collectionDb.Save();
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.Exception(ex);
            return false;
        }
    }
}