using System.ComponentModel.DataAnnotations;

namespace Hermes.Models;

public class TicketFormModel
{
    public int? Id { get; set; }

    [Required(ErrorMessage = "A title is required.")]
    [StringLength(200, MinimumLength = 5,
        ErrorMessage = "The title must be between {2} and {1} characters.")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "A description is required.")]
    [StringLength(4000, MinimumLength = 10,
        ErrorMessage = "The description must be between {2} and {1} characters.")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "A category is required.")]
    [Range(1, int.MaxValue, ErrorMessage = "A category is required.")]
    public int? CategoryId { get; set; }

    [Required(ErrorMessage = "A priority is required.")]
    public TicketPriority? Priority { get; set; } = TicketPriority.Normal;

    public string? AssignedToId { get; set; }
}
