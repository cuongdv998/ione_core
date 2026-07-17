using iOne.Report.Localization;
using iOne.ResDocuments;
using iOne.Master.ResDocuments;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;
using iOne.Report.Permissions;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Content;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using System.Text.RegularExpressions;
using System.Collections;
using ClosedXML.Report;
using System.Text;
using System.Text.Json;

using iOne.File.Conversion;

namespace iOne.Report.Certificates
{
    public class CertificateAppService : ApplicationService, ICertificateAppService
    {
        private readonly IResDocumentAppService _resDocumentAppService;
        private readonly IFileConversionAppService _fileConversionAppService;

        public CertificateAppService(
            IResDocumentAppService resDocumentAppService,
            IFileConversionAppService fileConversionAppService)
        {
            LocalizationResource = typeof(ReportResource);
            _resDocumentAppService = resDocumentAppService;
            _fileConversionAppService = fileConversionAppService;
        }

        [Authorize(CertificatePermissions.Generate)]
        public async Task<IRemoteStreamContent> GenerateCertificateAsync(Guid documentId, Dictionary<string, object> data)
        {
            try
            {
                // 1. Lấy thông tin file từ Guid
                var resDocument = await _resDocumentAppService.GetSingleFileAsync(documentId);
                var ext = Path.GetExtension(resDocument.FileName)?.ToLower();

                if (string.IsNullOrWhiteSpace(resDocument?.Url))
                {
                    throw new UserFriendlyException("Không tìm thấy đường dẫn tải file mẫu.");
                }

                // 2. Tải file mẫu về stream
                using var httpClient = new HttpClient();
                var bytes = await httpClient.GetByteArrayAsync(resDocument.Url!);

                // 3. Paste data vào file và trả về
                if (ext == ".docx")
                {
                    var memoryStream = new MemoryStream();
                    await memoryStream.WriteAsync(bytes, 0, bytes.Length);
                    memoryStream.Position = 0;

                    using (var wordDoc = WordprocessingDocument.Open(memoryStream, true))
                    {
                        ReplaceSingleFields(wordDoc, data);
                        FillMultipleTables(wordDoc, data);
                        wordDoc.MainDocumentPart.Document.Save();
                    }

                    memoryStream.Position = 0;
                    var wordBytes = memoryStream.ToArray();
                    var pdfBytes = await _fileConversionAppService.ConvertWordToPdfAsync(wordBytes, resDocument.FileName ?? "Certificate.docx");

                    var pdfStream = new MemoryStream(pdfBytes);
                    var pdfFileName = Path.ChangeExtension(resDocument.FileName ?? "Certificate.docx", ".pdf");

                    return new RemoteStreamContent(
                        pdfStream,
                        pdfFileName,
                        "application/pdf"
                    );
                }
                
                throw new UserFriendlyException($"Định dạng file không được hỗ trợ: {ext}");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Lỗi khi tạo chứng chỉ cho DocumentId {DocumentId}", documentId);
                throw new UserFriendlyException("Đã xảy ra lỗi trong quá trình tạo file chứng chỉ.");
            }
        }

        #region Word Helpers (OpenXML)

        private void ReplaceSingleFields(
            WordprocessingDocument wordDoc,
            Dictionary<string, object> data)
        {
            var regex = new Regex(@"\{\{\s*(?<key>[^}]+?)\s*\}\}");

            var paragraphs = wordDoc.MainDocumentPart.Document.Descendants<Paragraph>();

            foreach (var paragraph in paragraphs)
            {
                var texts = paragraph.Descendants<Text>().ToList();

                if (!texts.Any())
                    continue;

                var fullText = string.Concat(texts.Select(t => t.Text));

                if (!fullText.Contains("{{"))
                    continue;

                var replacedText = regex.Replace(fullText, match =>
                {
                    var key = match.Groups["key"].Value.Trim();

                    if (!data.TryGetValue(key, out var value))
                        return match.Value;

                    if (value is IEnumerable && value is not string)
                        return match.Value;

                    return value?.ToString() ?? string.Empty;
                });
                
                foreach (var text in texts)
                    text.Text = "";
                
                texts.First().Text = replacedText;
            }
        }

        private void FillMultipleTables(WordprocessingDocument wordDoc, Dictionary<string, object> data)
        {
            foreach (var entry in data)
            {
                if (entry.Value is not IEnumerable list || entry.Value is string) continue;
                var tableKey = entry.Key;
                var items = list.Cast<object>().ToList();
                FillTableByKey(wordDoc, tableKey, items);
            }
        }

        private void FillTableByKey(WordprocessingDocument wordDoc, string tableKey, List<object> items)
        {
            var body = wordDoc.MainDocumentPart.Document.Body;
            var table = body.Descendants<Table>().FirstOrDefault(t => t.InnerText.IndexOf($"{{{{#{tableKey}}}}}", StringComparison.OrdinalIgnoreCase) >= 0);
            if (table == null) return;

            RemoveRowContainsMarker(wordDoc, $"{{{{#{tableKey}}}}}");
            var templateRow = table.Descendants<TableRow>().FirstOrDefault(r => r.InnerText.Contains("{{"));
            if (templateRow == null) return;

            templateRow.Remove();
            int index = 1;

            var placeholderRegex = new Regex(@"\{\{\s*(?<key>[^}]+?)\s*\}\}", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            foreach (var item in items)
            {
                var newRow = (TableRow)templateRow.CloneNode(true);
                
                // Cố gắng parse item về dictionary nếu có thể
                Dictionary<string, object> itemDict = new();
                if (item is Dictionary<string, object> d) itemDict = d;
                else if (item is JsonElement je && je.ValueKind == JsonValueKind.Object)
                {
                    itemDict = JsonSerializer.Deserialize<Dictionary<string, object>>(je.GetRawText()) ?? new();
                }

                foreach (var text in newRow.Descendants<Text>())
                {
                    if (!text.Text.Contains("{{")) continue;

                    text.Text = placeholderRegex.Replace(text.Text, match =>
                    {
                        var key = match.Groups["key"].Value.Trim();
                        if (key.Equals("Index", StringComparison.OrdinalIgnoreCase)) return index.ToString();
                        
                        if (itemDict != null && itemDict.TryGetValue(key, out var value))
                            return value?.ToString() ?? string.Empty;
                        
                        return match.Value;
                    });
                }
                table.AppendChild(newRow);
                index++;
            }
        }

        private void RemoveRowContainsMarker(WordprocessingDocument wordDoc, string marker)
        {
            var rows = wordDoc.MainDocumentPart.Document.Body.Descendants<TableRow>()
                .Where(r => r.InnerText.IndexOf(marker, StringComparison.OrdinalIgnoreCase) >= 0)
                .ToList();
            foreach (var row in rows) row.Remove();
        }

        #endregion
    }
}
