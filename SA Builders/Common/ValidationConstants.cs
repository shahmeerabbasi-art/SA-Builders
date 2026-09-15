namespace SA_Builders.Common
{
    // Centralized so every DTO/model uses the same limits — change a number
    // here once instead of hunting through a dozen files.
    public static class ValidationConstants
    {
        public const int NameMinLength = 2;
        public const int NameMaxLength = 150;

        public const int EmailMaxLength = 254; // RFC 5321 max email length

        public const int PasswordMinLength = 8;
        public const int PasswordMaxLength = 100;

        public const int ShortTextMaxLength = 200;
        public const int LongTextMaxLength = 2000;

        public const int UrlMaxLength = 500;

        public const double MinCoveredAreaSqFt = 50;
        public const double MaxCoveredAreaSqFt = 1_000_000;

        public const double MinCostImpact = 0;
        public const double MaxCostImpact = 100_000_000;

        public const int MinRating = 1;
        public const int MaxRating = 5;
    }
}