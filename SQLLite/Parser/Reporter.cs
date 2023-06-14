using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using scoring_counter_agent_bot.DB.Entity;
using scoring_counter_agent_bot.DB.Repository;
using scoring_counter_agent_bot.Parser.Check;
using scoring_counter_agent_bot.Parser.Egr;
using scoring_counter_agent_bot.Parser.Search;
using Telegram.Bot.Types.Enums;

namespace scoring_counter_agent_bot.Parser;

/* Пока что это класс для хранения методов парсинга и подведения результатов обработки отчётов
 * в будущем отсюда будет строиться модель нашего html-отчёта
 * 
 */
internal class Reporter
{
    ///НАСТРОЙКИ ТАКИХ ВЕЩЕЙЙ ДОЛЖНЫ БЫТЬ ВЫНЕСЕНЫ В ФАЙЛ А ВООБЩЕ ЖЕЛАТЕЛЬНО КУДА-ТО В SECRET MANAGER!!!!
    private static readonly string hashCodeFNS = "7d94224ada019dbff5390249379b9ffe51a8547a";
    //private static string hashCodeZakupki = "780b3539a3011ef47648904e252aa577cee237a8";


    public async Task<CommandResponse> GetResultChecking(string inn)
    {
        var mass = Enumerable.Range(1, 50).Select(x => x.ToString()).ToArray();
        var res = await Task.WhenAll(GetSearchFNS(inn), GetCheckFNS(inn), GetEgrFNS(inn), GetBuchOtchFNS(inn));
        //var presentedSite = await CreateHtmlReport(mass);
        var commandResponse = new CommandResponse
        {
            TextMessage = string.Join("\n\n", res),
            ParseMode = ParseMode.Html,
            //Payload = presentedSite
        };
        using var rep = new Repository();            
        return commandResponse;
    }

    private async Task<string> GetRequestFNS(string method, string inn)
    {
        var url = "https://api-fns.ru/api/{0}={1}&key={2}";
        var urlResult = string.Format(url, method, inn, hashCodeFNS);
        var client = new HttpClient();
        var result = await client.GetStringAsync(urlResult);
        return result;
    }

    public async Task<string> GetEgrFNS(string inn)
    {
        var result = await GetRequestFNS(RequestHelper.Egr, inn);
        var egrFNS = JsonConvert.DeserializeObject<Parse_egr_fns>(result);
        string fnsEgrResult;
        if (egrFNS?.items.Any() == true && egrFNS?.items[0]?.ЮЛ != null)
            fnsEgrResult = egrFNS.items[0].ЮЛ.GetContacts();
        else if (egrFNS?.items.Any() == true && egrFNS?.items[0]?.ИП != null)
            fnsEgrResult = egrFNS.items[0].ИП.GetContacts();
        else
            fnsEgrResult = "Нет данных";
        return fnsEgrResult;
    }

    public async Task<string> GetCheckFNS(string inn)
    {
        var result = await GetRequestFNS(RequestHelper.Check, inn);
        var checkFNS = JsonConvert.DeserializeObject<Parse_check_fns>(result);
        string fnsCheckResult;
        if (checkFNS?.items.Any() == true && checkFNS?.items[0]?.ЮЛ != null)
            fnsCheckResult = checkFNS.items.First().ЮЛ.GetText();
        else if (checkFNS?.items.Any() == true && checkFNS?.items[0]?.ИП != null)
            fnsCheckResult = checkFNS.items[0].ИП.GetText();
        else
            fnsCheckResult = "Нет данных";
        return "Краткая сводка ФНС: \n" + fnsCheckResult;
    }

    public async Task<byte[]> CreateHtmlReport(string[] fields)
    {
        var text = await File.ReadAllTextAsync(@"Report\index.html");
        var header = text.Substring(HtmlReportHelper.StartHeader, HtmlReportHelper.EndHeader);
        text = text.Remove(HtmlReportHelper.StartHeader, HtmlReportHelper.EndHeader);
        var site = string.Format(text, fields);
        header += site;

        await File.WriteAllTextAsync("TEST.html", header);
        return Encoding.UTF8.GetBytes(header);
    }

    public async Task<string> GetSearchFNS(string inn)
    {
        var result = await GetRequestFNS(RequestHelper.Search, inn);
        var searchFNS = JsonConvert.DeserializeObject<Parse_search_fns>(result);
        var fnsSearchResult = string.Empty;
        if (searchFNS?.items.Any() == true && searchFNS?.items[0]?.ЮЛ != null)
            fnsSearchResult = searchFNS.items[0].ЮЛ.GetText();
        else if (searchFNS?.items.Any() == true && searchFNS?.items[0]?.ИП != null)
            fnsSearchResult = searchFNS.items[0].ИП.GetText();
        else
            fnsSearchResult = "Нет данных";
        return fnsSearchResult;
    }

    public async Task<string> GetBuchOtchFNS(string inn)
    {
        var url = "https://api-fns.ru/api/bo?req={0}&key={1}";
        var urlResult = string.Format(url, inn, hashCodeFNS);
        var client = new HttpClient();
        var response = await client.GetAsync(urlResult);
        var json = await response.Content.ReadAsStringAsync();
        var localResult = new StringBuilder();

        if (json == "[]") return localResult.Append("Нет данных").ToString();

        var jobj = JObject.Parse(json);

        var innResult = jobj[inn];
        if (!innResult.HasValues)
            return localResult.Append("Нет данных").ToString();

        var result = innResult.ToObject<Dictionary<string, Dictionary<string, double>>>();


        double? koefLiquid = null; // 1200/1500 если в районе 1.5-2 то норма
        double? koefAutonomy = null; // 1300/1600 если >=0.5 то норма
        double? rentSales = null; // 2200/2110 0.01-0.05 - норма
        double? rentActivity = null; // 2400/1600 >0.1 - норма
        double? koefFastLiquid = null; // (1230 + 1240 + 1250)/(1510 + 1520 + 1550) норма 0.8-1
        double? koefAbsLiquid = null; // (1240 + 1250)/ (1510 + 1520 + 1540 + 1550) норма 0.2-0.5

        if (result.Any())
        {
            var lastYear = result.Keys.Last();
            var dictRows = result[lastYear];
            localResult.Append("Расчёт коэффициентов: \n");

            if (dictRows.TryGetValue("1200", out var srt1200) && dictRows.TryGetValue("1500", out var str1500))
                koefLiquid = srt1200 / str1500; //(double)
            else
                localResult.Append("Коэффициент текущей ликвидности: Нет данных \n");

            if (dictRows.TryGetValue("1300", out var str1300) && dictRows.TryGetValue("1600", out var str1600))
                koefAutonomy = str1300 / str1600;
            else
                localResult.Append("Коэффициент автономии: Нет данных \n");

            if (dictRows.TryGetValue("2200", out var str2200) && dictRows.TryGetValue("2200", out var str2110))
                rentSales = str2200 / str2110;
            else
                localResult.Append("Рентабельность продаж: Нет данных \n");

            if (dictRows.TryGetValue("2400", out var str2400) && dictRows.TryGetValue("1600", out str1600))
                rentActivity = str2200 / str1600;
            else
                localResult.Append("Рентабельность активов: Нет данных \n");

            if (dictRows.TryGetValue("1230", out var str1230) && dictRows.TryGetValue("1240", out var str1240) &&
                dictRows.TryGetValue("1250", out var str1250) && dictRows.TryGetValue("1510", out var str1510) &&
                dictRows.TryGetValue("1520", out var str1520) &&
                dictRows.TryGetValue("1550", out var str1550))
                koefFastLiquid = (str1230 + str1240 + str1240) / (str1510 + str1520 + str1550);

            if (dictRows.TryGetValue("1240", out str1240) && dictRows.TryGetValue("1250", out str1250) &&
                dictRows.TryGetValue("1510", out str1510) && dictRows.TryGetValue("1520", out str1520) &&
                dictRows.TryGetValue("1540", out var str1540) &&
                dictRows.TryGetValue("1550", out str1550))
                koefAbsLiquid = (str1240 + str1250) / (str1510 + str1520 + str1540 + str1550);
        }

        if (koefLiquid < 2.0 && koefLiquid > 1.5)
            localResult.Append("📈Коэффициент текущей ликвидности (в норме): " + $"{koefLiquid:0.##}" + "\n");
        else if (koefLiquid != null)
            localResult.Append("📉Коэффициент текущей ликвидности (не в норме): " + $"{koefLiquid:0.##}" + "\n");
        if (koefAutonomy > 0.5)
            localResult.Append($"📈Коэффициент автономии (в норме): {koefAutonomy:0.##} \n");
        else if (koefAutonomy != null)
            localResult.Append("📉Коэффициент автономии (не в норме): " + $"{koefAutonomy:0.##}" + "\n");

        if (rentSales < 0.05 && rentSales > 0.01)
            localResult.Append("📈Рентабельность продаж (в норме): " + $"{rentSales:0.##}" + "\n");
        else if (rentSales != null)
            localResult.Append("📉Рентабельность продаж (не в норме): " + $"{rentSales:0.##}" + "\n");

        if (rentActivity > 0.1)
            localResult.Append("📈Рентабельность активов (в норме): " + $"{rentActivity:0.##}" + "\n");
        else if (rentActivity != null)
            localResult.Append("📉Рентабельность активов (не в норме): " + $"{rentActivity:0.##}" + "\n");

        if (koefFastLiquid < 1.0 && koefFastLiquid > 0.8)
            localResult.Append("📈Коэффициент быстрой ликвидности (в норме): " +
                               string.Format("{0:0.##}", koefFastLiquid) + "\n");
        else if (koefFastLiquid != null)
            localResult.Append("📉Коэффициент быстрой ликвидности (не в норме): " +
                               string.Format("{0:0.##}", koefFastLiquid) + "\n");

        if (koefAbsLiquid < 0.5 && koefAbsLiquid > 0.2)
            localResult.Append("📈Коэффициент абсолютной ликвидности (в норме): " +
                               string.Format("{0:0.##}", koefAbsLiquid) + "\n");
        else if (koefAbsLiquid != null)
            localResult.Append("📉Коэффициент ликвидности (не в норме): " + string.Format("{0:0.##}", koefAbsLiquid) +
                               "\n");

        return localResult.ToString();
    }
}