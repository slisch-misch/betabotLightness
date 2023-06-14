using System.ComponentModel.DataAnnotations;

namespace betabotLightness.Models.Enums;
public enum Tariff
{
    [Display]
    None = 0,
    Light,
    Standart,
    Max,
    All
}