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
            var reader = new StreamReader($"{filepath}\\hg.htm", Encoding.GetEncoding(1251));
            string html = await reader.ReadToEndAsync();

            var parser = new HtmlParser();
            var document = await parser.ParseDocumentAsync(html);

            return document.QuerySelector("li.zgr")?.TextContent.Trim() ?? string.Empty;
        }
    }
}
