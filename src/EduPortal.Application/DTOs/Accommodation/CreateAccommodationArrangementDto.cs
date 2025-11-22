using System.ComponentModel.DataAnnotations;

namespace EduPortal.Application.DTOs.Accommodation;

public class CreateAccommodationArrangementDto
{
    [Required]
    public int ProgramId { get; set; }

    [Required]
    public int Type { get; set; } // AccommodationType enum

    [MaxLength(300)]
    public string? Name { get; set; }

    [MaxLength(500)]
    public string? Address { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public decimal? MonthlyCost { get; set; }

    public decimal? SecurityDeposit { get; set; }

    [MaxLength(100)]
    public string? ContactPerson { get; set; }

    [MaxLength(50)]
    public string? ContactPhone { get; set; }

    [MaxLength(100)]
    public string? ContactEmail { get; set; }

    [MaxLength(2000)]
    public string? Notes { get; set; }
}
