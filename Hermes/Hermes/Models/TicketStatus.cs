using System.ComponentModel.DataAnnotations;

namespace Hermes.Models;

public enum TicketStatus
{
    [Display(Name = "Open")]
    Open = 1,

    [Display(Name = "In progress")]
    InProgress = 2,

    [Display(Name = "Resolved")]
    Resolved = 3,

    [Display(Name = "Closed")]
    Closed = 4
}
