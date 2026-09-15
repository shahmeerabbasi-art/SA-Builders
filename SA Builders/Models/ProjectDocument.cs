using System.ComponentModel.DataAnnotations;
using SA_Builders.Common;

namespace SA_Builders.Models
{
    public enum DocumentType
    {
        Contract,
        Blueprint,
        NOC,           // No Objection Certificate
        FloorPlan,
        Other
    }

    public class ProjectDocument
    {
        public int Id { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "ProjectId must reference a valid project.")]
        public int ProjectId { get; set; }
        public Project? Project { get; set; }

        [Required, StringLength(ValidationConstants.ShortTextMaxLength, MinimumLength = 2)]
        public string FileName { get; set; } = string.Empty;

        [Required, StringLength(ValidationConstants.UrlMaxLength)]
        public string FileUrl { get; set; } = string.Empty;

        [Required, EnumDataType(typeof(DocumentType))]
        public DocumentType DocumentType { get; set; }

        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        [Range(1, int.MaxValue, ErrorMessage = "UploadedByUserId must reference a valid user.")]
        public int UploadedByUserId { get; set; }
    }
}