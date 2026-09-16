using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Hermes.Common;

public static class EnumExtensions
{
    /// <summary>
    /// Returns the [Display(Name = "...")] text for an enum value, or the value's
    /// own name when the attribute is absent.
    /// </summary>
    public static string GetDisplayName(this Enum value)
    {
        var member = value.GetType()
            .GetMember(value.ToString())
            .FirstOrDefault();

        return member?.GetCustomAttribute<DisplayAttribute>()?.Name ?? value.ToString();
    }
}
