namespace SA_Builders.Models
{
    public class Certification
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string IssuingBody { get; set; } = string.Empty;
        public string? DocumentUrl { get; set; }
        public string? IconUrl { get; set; }
        public bool DisplayOnHome { get; set; } = true;
        public int SortOrder { get; set; } = 0;
    }
}