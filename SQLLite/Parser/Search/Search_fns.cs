/*
 * Модель парсинга отчёта по api-fns для метода search
 * 
 */

using System.Text;

namespace scoring_counter_agent_bot.Parser.Search;

// Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
public class ItemSearch
{
    public ЮЛ ЮЛ { get; set; }
    public ИП ИП { get; set; }
}

public class Parse_search_fns
{
    public List<ItemSearch> items { get; set; }
    public int Count { get; set; }
}

public class ИП
{
    public string ИНН { get; set; }
    public string ОГРН { get; set; }
    public string ФИОПолн { get; set; }
    public string ДатаРег { get; set; }
    public string Статус { get; set; }
    public string ДатаПрекр { get; set; }
    public string АдресПолн { get; set; }
    public string ОснВидДеят { get; set; }
    public string ГдеНайдено { get; set; }

    public string GetText()
    {
        var text = new StringBuilder();
        if (ФИОПолн != null) text.Append("👔" + ФИОПолн + "\n");
        text.Append("Основная информация:" + "\n");
        if (ИНН != null) text.Append("<b>ИНН:</b> " + ИНН + "\n");
        if (ОГРН != null) text.Append("<b>ОГРН:</b> " + ОГРН + "\n");
        //if (this.ДатаРег != null) text.Append("<b>Дата регистрации:</b> " + this.ДатаРег + "\n");
        if (Статус != null) text.Append("<b>Статус:</b> " + Статус + "\n");
        if (ОснВидДеят != null) text.Append("<b>Основной вид деятельности:</b> " + ОснВидДеят + "\n");

        return text.ToString();
    }
}

public class ЮЛ
{
    public string ИНН { get; set; }
    public string ОГРН { get; set; }
    public string НаимСокрЮЛ { get; set; }
    public string НаимПолнЮЛ { get; set; }
    public string ДатаРег { get; set; }
    public string Статус { get; set; }
    public string ДатаПрекр { get; set; }
    public string АдресПолн { get; set; }
    public string ОснВидДеят { get; set; }
    public string ГдеНайдено { get; set; }

    public string GetText()
    {
        var text = new StringBuilder();
        if (НаимПолнЮЛ != null) text.Append("🛕" + НаимПолнЮЛ + "\n");
        text.Append("Основная информация:" + "\n");
        if (ИНН != null) text.Append("<b>ИНН:</b> " + ИНН + "\n");
        if (ОГРН != null) text.Append("<b>ОГРН:</b> " + ОГРН + "\n");
        //if (this.ДатаРег != null) text.Append("<b>Дата регистрации:</b> " + this.ДатаРег + "\n");
        if (Статус != null) text.Append("<b>Статус:</b> " + Статус + "\n");
        if (ОснВидДеят != null) text.Append("<b>Основной вид деятельности:</b> " + ОснВидДеят + "\n");

        return text.ToString();
    }
}