using System.Text;
using Newtonsoft.Json;

namespace scoring_counter_agent_bot.Parser.Egr;

public class ItemEgr
{
    public ЕгрИП ИП { get; set; }
    public ЕгрЮЛ ЮЛ { get; set; }
}

public class Parse_egr_fns
{
    public List<ItemEgr> items { get; set; }
}

public class ЕгрИП
{
    public string ФИОПолн { get; set; }
    public string ИННФЛ { get; set; }
    public string ОГРНИП { get; set; }
    public string ДатаРег { get; set; }
    public string ВидИП { get; set; }
    public string Пол { get; set; }
    public string ВидГражд { get; set; }
    public string ОКСМ { get; set; }
    public string Статус { get; set; }
    public string СтатусДата { get; set; }
    public string СпОбрЮЛ { get; set; }
    public string ДатаПрекр { get; set; }
    public НО НО { get; set; }
    public ПФ ПФ { get; set; }
    public ФСС ФСС { get; set; }
    public ОснВидДеят ОснВидДеят { get; set; }
    public List<СПВЗ> СПВЗ { get; set; }
    public История История { get; set; }
    public Контакты Контакты { get; set; }

    [JsonProperty("E-mail")] public string Email { get; set; }

    public Адрес Адрес { get; set; }


    public string GetContacts()
    {
        var text = new StringBuilder();
        text.Append("Контакты: \n");
        if (Контакты.Email != null) text.Append("✉️" + Контакты.Email.First() + "\n");
        if (Контакты.Сайт != null) text.Append("🕸" + Контакты.Сайт.First() + "\n");
        if (Адрес.АдресПолн != null) text.Append("🏢" + Адрес.АдресПолн + "\n");
        return text.ToString();
    }
}

public class ЕгрЮЛ
{
    public string НомТел { get; set; }
    public Адрес Адрес { get; set; }

    [JsonProperty("E-mail")] public string Email { get; set; }

    public string ИНН { get; set; }
    public string КПП { get; set; }
    public string ОГРН { get; set; }
    public string НаимСокрЮЛ { get; set; }
    public string НаимПолнЮЛ { get; set; }
    public string НаимСокрЮЛИн { get; set; }
    public string НаимПолнЮЛИн { get; set; }
    public string ДатаРег { get; set; }
    public string ОКОПФ { get; set; }
    public string КодОКОПФ { get; set; }
    public string Статус { get; set; }
    public string СтатусДата { get; set; }
    public string СпОбрЮЛ { get; set; }
    public string ДатаПрекр { get; set; }
    public string СпПрекрЮЛ { get; set; }
    public Контакты Контакты { get; set; }


    public НО НО { get; set; }
    public ПФ ПФ { get; set; }
    public ФСС ФСС { get; set; }
    public Капитал Капитал { get; set; }

    public Руководитель Руководитель { get; set; }


    public ОснВидДеят ОснВидДеят { get; set; }
    public List<ДопВидДеят> ДопВидДеят { get; set; }
    public List<СПВЗ> СПВЗ { get; set; }
    public ОткрСведения ОткрСведения { get; set; }
    public List<Участия> Участия { get; set; }
    public История История { get; set; }


    public string GetContacts()
    {
        var text = new StringBuilder();
        text.Append("Контакты: \n");
        if (Контакты.Телефон != null) text.Append("☎️" + Контакты.Телефон.First() + "\n");
        if (Контакты.Email != null) text.Append("✉️" + Контакты.Email.First() + "\n");
        if (Контакты.Сайт != null) text.Append("🕸" + Контакты.Сайт.First() + "\n");
        if (Адрес.АдресПолн != null) text.Append("🏢" + Адрес.АдресПолн + "\n");

        return text.ToString();
    }
}

public class Адрес
{
    public string КодРегион { get; set; }
    public string Индекс { get; set; }
    public string АдресПолн { get; set; }
    public string Дата { get; set; }
    public string ИдНомФИАС { get; set; }
}

public class Капитал
{
    public string ВидКап { get; set; }
    public string СумКап { get; set; }
    public string Дата { get; set; }
}

public class История
{
    public Адрес Адрес { get; set; }
}

public class НО
{
    public string Рег { get; set; }
    public string РегДата { get; set; }
    public string Аккр { get; set; }
    public string АккрДата { get; set; }
    public string ДатаПрекрАккр { get; set; }
    public string Учет { get; set; }
    public string УчетДата { get; set; }
}

public class ОснВидДеят
{
    public string Код { get; set; }
    public string Текст { get; set; }
}

public class ДопВидДеят
{
    public string Код { get; set; }
    public string Текст { get; set; }
}

public class ОткрСведения
{
    public string КолРаб { get; set; }
    public string СведСНР { get; set; }
    public string ПризнУчКГН { get; set; }
    public string СумДоход { get; set; }
    public string СумРасход { get; set; }
    public string Дата { get; set; }
}

public class ПФ
{
    public string РегНомПФ { get; set; }
    public string ДатаРегПФ { get; set; }
    public string КодПФ { get; set; }
}

public class СПВЗ
{
    public string Дата { get; set; }
    public string Текст { get; set; }
}

public class Руководитель
{
    public string ВидДолжн { get; set; }
    public string Должн { get; set; }
    public string ФИОПолн { get; set; }
    public string ИННФЛ { get; set; }
    public string Пол { get; set; }
    public string ВидГражд { get; set; }
    public string ОКСМ { get; set; }
    public string ОГРНИП { get; set; }
    public string ДатаНачДискв { get; set; }
    public string ДатаОкончДискв { get; set; }
    public string Дата { get; set; }
}

public class Участия
{
    public string ОГРН { get; set; }
    public string ИНН { get; set; }
    public string НаимСокрЮЛ { get; set; }
    public string Статус { get; set; }
    public string Процент { get; set; }
    public string СуммаУК { get; set; }
}

public class ФСС
{
    public string РегНомФСС { get; set; }
    public string ДатаРегФСС { get; set; }
    public string КодФСС { get; set; }
}

public class Контакты
{
    public string[] Телефон { get; set; }

    [JsonProperty("E-mail")] public string[] Email { get; set; }

    public string[] Сайт { get; set; }
}