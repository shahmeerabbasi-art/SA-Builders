using System.ComponentModel.DataAnnotations;
using SA_Builders.Models;

namespace SA_Builders.DTOs
{
    public class CreateProjectDto
    {
        [Required, MaxLength(150)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(200)]
        public string Location { get; set; } = string.Empty;

        public double CoveredAreaSqFt { get; set; }

        [Required]
        public ConstructionType ConstructionType { get; set; }

        public string? Description { get; set; }

        public bool ShowOnPublicPortfolio { get; set; } = true;
    }

    public class UpdateProjectDto
    {
        [Required, MaxLength(150)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(200)]
        public string Location { get; set; } = string.Empty;

        public double CoveredAreaSqFt { get; set; }

        [Required]
        public ConstructionType ConstructionType { get; set; }

        public string? Description { get; set; }

        [Required]
        public ProjectStatus Status { get; set; }

        public bool ShowOnPublicPortfolio { get; set; }
    }

    public class CreateCertificationDto
    {
        [Required, MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        [Required, MaxLength(150)]
        public string IssuingBody { get; set; } = string.Empty;

        public bool DisplayOnHome { get; set; } = true;

        public int SortOrder { get; set; } = 0;
    }
}