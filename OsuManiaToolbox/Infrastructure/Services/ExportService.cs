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
                _logger.Error("�ļ�������Ϊ��");
                return false;
            }
            
            if (!beatmaps.Any())
            {
                _logger.Info("û�пɵ���������");
                return false;
            }
            
            var file = fileName + ".csv";
            if (File.Exists(file))
            {
                _logger.Warning($"�ļ� {file} �Ѵ��ڣ��Զ�����");
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
            
            _logger.Info($"�ѵ��� {table.Rows.Count} ��������Ϣ�� {file}");

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
                _logger.Error("�ղؼ����Ʋ���Ϊ��");
                return false;
            }
            
            if (!beatmapsMd5.Any())
            {
                _logger.Info("û�з�������������");
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
                _logger.Warning($"�ղؼ� {collection.Name} �Ѵ��ڣ��Զ�����");
                _collectionDb.Remove(collection.Name);
            }
            
            _collectionDb.Add(collection);
            _logger.Info($"�����ղؼ� {collection.Name}������ {collection.Count} ������");
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