using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AngleSharp;
using AngleSharp.Html.Parser;
using Newtonsoft.Json;

namespace unloadSchedule.Classes
{
    public class ParseSchedule
    {
        public async Task<string> ParseScheduleDay(string filepath)
        {
            byte[] fileBytes;

            using (var fileStream = File.OpenRead($"{filepath}\\hg.htm"))
            {
                fileBytes = new byte[fileStream.Length];
                await fileStream.ReadAsync(fileBytes, 0, (int)fileStream.Length);
            }

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            var encoding = Encoding.GetEncoding(1251);
            var parser = new HtmlParser();
            var memoryStream = new MemoryStream(fileBytes);
            var document = await parser.ParseDocumentAsync(memoryStream);

            return document.QuerySelector("li.zgr")?.TextContent.Trim() ?? string.Empty;
        }
    }
}
