using System.ComponentModel.DataAnnotations;
using SA_Builders.Common;
using SA_Builders.Models;

namespace SA_Builders.DTOs
{
    // Used as form fields alongside the uploaded file itself —
    // see AdminDocumentsController.Upload for how this pairs with IFormFile.
    public class UploadDocumentDto
    {
        [Required, StringLength(ValidationConstants.ShortTextMaxLength, MinimumLength = 2)]
        public string FileName { get; set; } = string.Empty;

        [Required, EnumDataType(typeof(DocumentType))]
        public DocumentType DocumentType { get; set; }
    }
}