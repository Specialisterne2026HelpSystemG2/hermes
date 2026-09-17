using System.ComponentModel.DataAnnotations;

namespace Hermes.Models;

/// <summary>
/// Modelled as an enum to keep the prototype free of an extra table. If departments
/// ever need to be managed at runtime this becomes an entity with a foreign key.
/// </summary>
public enum DepartmentEnum
{
    [Display(Name = "Information Technology")]
    InformationTechnology = 1,

    [Display(Name = "Human Resources")]
    HumanResources = 2,

    [Display(Name = "Finance")]
    Finance = 3,

    [Display(Name = "Sales")]
    Sales = 4,

    [Display(Name = "Operations")]
    Operations = 5,

    [Display(Name = "Legal")]
    Legal = 6
}
