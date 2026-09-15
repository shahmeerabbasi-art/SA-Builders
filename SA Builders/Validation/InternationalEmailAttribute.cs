using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text.RegularExpressions;

namespace SA_Builders.Validation
{
    // The built-in [EmailAddress] attribute is looser than most people expect
    // (it accepts things like "a@b") and doesn't handle internationalized
    // domain names (e.g. a client's domain in Arabic/Chinese/etc script)
    // consistently. This attribute:
    //   1. Requires a realistic local-part@domain.tld shape
    //   2. Converts internationalized domains to Punycode before checking,
    //      so a real-world international domain isn't wrongly rejected
    //   3. Requires a real top-level domain (at least 2 letters, no numbers)
    public class InternationalEmailAttribute : ValidationAttribute
    {
        // local-part@domain-labels.tld
        // local-part: letters, digits, and . _ % + - (no leading/trailing/double dots)
        // domain: one or more labels separated by dots, each label alphanumeric/hyphen
        // tld: at least 2 letters (supports long TLDs like .construction, .international)
        private static readonly Regex Pattern = new(
            @"^(?!\.)(?!.*\.\.)[A-Za-z0-9._%+\-]+(?<!\.)@[A-Za-z0-9]([A-Za-z0-9\-]{0,61}[A-Za-z0-9])?(\.[A-Za-z0-9]([A-Za-z0-9\-]{0,61}[A-Za-z0-9])?)*\.[A-Za-z]{2,}$",
            RegexOptions.Compiled);

        public InternationalEmailAttribute()
        {
            ErrorMessage = "Enter a valid email address (e.g. name@company.com).";
        }

        public override bool IsValid(object? value)
        {
            if (value is not string email || string.IsNullOrWhiteSpace(email))
                return false;

            email = email.Trim();

            // Reject obviously malformed input before touching IdnMapping,
            // which throws on some invalid strings instead of failing gracefully.
            var atIndex = email.IndexOf('@');
            if (atIndex <= 0 || atIndex == email.Length - 1)
                return false;

            var localPart = email[..atIndex];
            var domainPart = email[(atIndex + 1)..];

            // Convert international domain (e.g. "münchen.de") to ASCII
            // Punycode ("xn--mnchen-3ya.de") so the regex below — which only
            // needs to understand ASCII — can validate it correctly.
            string asciiDomain;
            try
            {
                asciiDomain = new IdnMapping().GetAscii(domainPart);
            }
            catch (ArgumentException)
            {
                return false; // not a valid domain at all
            }

            var normalizedEmail = $"{localPart}@{asciiDomain}";
            return Pattern.IsMatch(normalizedEmail);
        }
    }
}