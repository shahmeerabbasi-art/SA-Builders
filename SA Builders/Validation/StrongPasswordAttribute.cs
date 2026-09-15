using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace SA_Builders.Validation
{
    // Applies to BOTH admin and client passwords — same rule everywhere a
    // password is created or changed, so there's one standard across the
    // whole app instead of different rules for different account types.
    //
    // Requires: 1 uppercase, 1 lowercase, 1 digit, 1 special character,
    // no whitespace anywhere (whitespace in passwords causes copy-paste
    // bugs and inconsistent behavior across keyboards/devices — common
    // pain point for an international user base typing on different
    // OS/keyboard layouts).
    public class StrongPasswordAttribute : ValidationAttribute
    {
        private static readonly Regex HasUpper = new(@"[A-Z]", RegexOptions.Compiled);
        private static readonly Regex HasLower = new(@"[a-z]", RegexOptions.Compiled);
        private static readonly Regex HasDigit = new(@"\d", RegexOptions.Compiled);
        private static readonly Regex HasSpecial = new(@"[^a-zA-Z0-9\s]", RegexOptions.Compiled);
        private static readonly Regex HasWhitespace = new(@"\s", RegexOptions.Compiled);

        public override bool IsValid(object? value)
        {
            if (value is not string password || string.IsNullOrEmpty(password))
            {
                ErrorMessage = "Password is required.";
                return false;
            }

            if (HasWhitespace.IsMatch(password))
            {
                ErrorMessage = "Password cannot contain spaces.";
                return false;
            }

            if (!HasUpper.IsMatch(password))
            {
                ErrorMessage = "Password must contain at least one uppercase letter.";
                return false;
            }

            if (!HasLower.IsMatch(password))
            {
                ErrorMessage = "Password must contain at least one lowercase letter.";
                return false;
            }

            if (!HasDigit.IsMatch(password))
            {
                ErrorMessage = "Password must contain at least one number.";
                return false;
            }

            if (!HasSpecial.IsMatch(password))
            {
                ErrorMessage = "Password must contain at least one special character.";
                return false;
            }

            return true;
        }
    }
}