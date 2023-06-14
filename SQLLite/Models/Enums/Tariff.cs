using System.ComponentModel.DataAnnotations;

namespace betabotLightness.Models.Enums;
public enum Tariff
{
    None = 0,
    [Display(Name = "Лайт")]
    Light,
    [Display(Name = "Стандарт")]
    Standart,
    [Display(Name = "Макс")]
    Max,
    All
}