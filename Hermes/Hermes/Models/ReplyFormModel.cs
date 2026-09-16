using System.ComponentModel.DataAnnotations;

namespace Hermes.Models;

public class ReplyFormModel
{
    [Required(ErrorMessage = "Write a reply before sending.")]
    [StringLength(4000, MinimumLength = 2,
        ErrorMessage = "The reply must be between {2} and {1} characters.")]
    public string Body { get; set; } = string.Empty;
}
