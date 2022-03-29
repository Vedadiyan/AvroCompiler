using System.Net;
using System.Text.RegularExpressions;
using AvroCompiler.Core.Storage;

namespace AvroCompiler.Core.Parser;

public class PreProcessors
{
    public static async Task<string> UrlImportAsync(string avroIdlStr)
    {
        string tempAvroIdStr = avroIdlStr;
        HttpClient httpClient = new HttpClient();
        Regex importPattern = new Regex(@"import((\W)*)(idl|protocol|schema)((\W)*)url((\W)*)\(([^\)]+)\)");
        Regex urlFunctionPattern = new Regex(@"url((\W)*)\(([^\)]+)\)");
        Regex doubleQuotePattern = new Regex(@"""(.*?[^\\])""");
        MatchCollection importMatches = importPattern.Matches(avroIdlStr);
        foreach (Match importMatch in importMatches)
        {
            string[] parts = importMatch.Value.Split();
            MatchCollection urlFunctionMatches = urlFunctionPattern.Matches(importMatch.Value);
            if (urlFunctionMatches.Count != 1)
            {
                throw new ArgumentException();
            }
            Match urlFunctionMatch = urlFunctionMatches.FirstOrDefault()!;
            MatchCollection urlMatches = doubleQuotePattern.Matches(urlFunctionMatch.Value);
            if (urlMatches.Count != 1)
            {
                throw new ArgumentException();
            }
            Match urlMatch = urlMatches.FirstOrDefault()!;
            string url = urlMatch.Value.Substring(1, urlMatch.Value.Length - 2);
            HttpResponseMessage httpResponseMessage = await httpClient.GetAsync(url);
            if (httpResponseMessage.StatusCode == HttpStatusCode.OK)
            {
                string tempFileName = TempFiles.Current.Value.GetTempFileName();
                using (StreamWriter sw = new StreamWriter(tempFileName))
                {
                    string content = await httpResponseMessage.Content.ReadAsStringAsync();
                    content = await PreProcessors.UrlImportAsync(content);
                    await sw.WriteLineAsync(content);
                }
                tempAvroIdStr = tempAvroIdStr.Replace(importMatch.Value, @$"import {parts[1]} ""{tempFileName.Replace("\\", "/")}""");
            }
            else
            {
                throw new WebException();
            }
        }
        return tempAvroIdStr;
    }
}