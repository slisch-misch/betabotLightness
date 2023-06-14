/*
 * Модель парсинга отчёта по api-fns для метода check
 * 
 */

using System.Text;

namespace scoring_counter_agent_bot.Parser.Check;

public class ItemCheck
{
    public ФНСЮЛ ЮЛ { get; set; }
    public ФНСИП ИП { get; set; }
}

public class Parse_check_fns
{
    public List<ItemCheck> items { get; set; }
}

public class ФНСЮЛ
{
    public string ОГРН { get; set; }
    public string ИНН { get; set; }
    public Позитив Позитив { get; set; }
    public Негатив Негатив { get; set; }

    public string GetText()
    {
        var text = new StringBuilder();
        if (Позитив.Текст != null)
            text.Append("✅Позитивные качества: " + " \n" +
                        Позитив.Текст + " \n");
        if (Негатив.Текст != null)
            text.Append("❌Негативные качества: " + " \n" +
                        Негатив.Текст + " \n");
        return text.ToString();
    }

    public string[] GetParams()
    {
        string[] param =
        {
            ОГРН,
            ИНН,
            Позитив.Текст,
            Негатив.Текст
        };
        return param;
    }
}

public class ФНСИП
{
    public string ОГРНИП { get; set; }
    public string ИННФЛ { get; set; }
    public Позитив Позитив { get; set; }
    public Негатив Негатив { get; set; }

    public string GetText()
    {
        var text = new StringBuilder();
        if (Позитив.Текст != null)
            text.Append("✅Позитивные качества: " + " \n" +
                        Позитив.Текст + " \n");
        if (Негатив.Текст != null)
            text.Append("❌Негативные качества: " + " \n" +
                        Негатив.Текст + " \n");
        return text.ToString();
    }

    public string[] GetParams()
    {
        string[] param =
        {
            ОГРНИП,
            ИННФЛ,
            Позитив.Текст,
            Негатив.Текст
        };
        return param;
    }
}

public class Негатив
{
    public string Статус { get; set; }
    public string ИсклИзРеестраМСП { get; set; }
    public string РегНедавно { get; set; }
    public string ДисквРук { get; set; }
    public string ДисквРукДр { get; set; }
    public string ДисквРукДрБезИНН { get; set; }
    public string РеестрМассАдрес { get; set; }
    public string МассАдрес { get; set; }
    public string РешИзмАдрес { get; set; }
    public string НедостоверАдрес { get; set; }
    public string СменаРег { get; set; }
    public string РеестрМассРук { get; set; }
    public string МассРук { get; set; }
    public string МассРукБезИНН { get; set; }
    public string РукЛиквКомп { get; set; }
    public string РукЛиквКомпБезИНН { get; set; }
    public string НедостоверРук { get; set; }
    public string РеестрМассУчр { get; set; }
    public string РеестрМассРукУчр { get; set; }
    public string ДисквУчрДр { get; set; }
    public string ДисквУчрДрБезИНН { get; set; }
    public string УчрЛиквКомп { get; set; }
    public string УчрЛиквКомпБезИНН { get; set; }
    public string ОдноврСменаРукУчр { get; set; }
    public string СменаРукГод { get; set; }
    public string РукУчр1Комп { get; set; }
    public string Обременения { get; set; }
    public string НеПредостОтч { get; set; }
    public string ЗадолжНалог { get; set; }
    public string РешУмКап { get; set; }
    public string КолРаб { get; set; }
    public string БлокСчета { get; set; }
    public string Банкрот { get; set; }
    public string БанкротНамерение { get; set; }
    public string РискНалогПроверки { get; set; }
    public string НедоимкаНалог { get; set; }
    public string Текст { get; set; }
}

public class Позитив
{
    public string Лицензии { get; set; }
    public string Филиалы { get; set; }
    public string ДатаВклМСП { get; set; }
    public string КатСубМСП { get; set; }
    public string ПризНовМСП { get; set; }
    public string СведСоцПред { get; set; }
    public string ССЧР { get; set; }
    public string ДатаСост { get; set; }
    public string КапБолее50тыс { get; set; }
    public string Текст { get; set; }
    public string АдресМСП { get; set; }
}