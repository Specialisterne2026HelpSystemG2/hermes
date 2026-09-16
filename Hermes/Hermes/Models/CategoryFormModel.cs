using System.ComponentModel.DataAnnotations;

namespace Hermes.Models;

public class CategoryFormModel
{
    public int? Id { get; set; }

    [Required(ErrorMessage = "A name is required.")]
    [StringLength(80, MinimumLength = 2,
        ErrorMessage = "The name must be between {2} and {1} characters.")]
    public string Name { get; set; } = string.Empty;
}
