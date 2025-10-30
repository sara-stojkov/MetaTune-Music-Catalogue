namespace Core.Utils
{
    public static class Validator
    {
        /// <summary>
        /// Function validates that a passed string is not null, empty or 
        /// just whitespace.
        /// </summary>
        /// <param name="stringToValidate"></param>
        /// <returns>True if the string is valid, false othervise</returns>
        public static bool IsValidString(string? stringToValidate)
        {
            return !string.IsNullOrWhiteSpace(stringToValidate);
        }
        /// <summary>
        /// Function validates that a passed email is not null, empty
        /// and is a valid email address.
        /// </summary>
        /// <param name="emailToValidate"></param>
        /// <returns>True if the email is valid, false othervise</returns>
        public static bool IsValidEmail(string? emailToValidate)
        {
            if (string.IsNullOrWhiteSpace(emailToValidate)) return false;
            try
            {
                // Fix: Use 'emailToValidate!' to assert non-null after IsValidString check
                var addr = new System.Net.Mail.MailAddress(emailToValidate);
                if (addr.Address != emailToValidate.Trim())
                    return false;
            }
            catch
            {
                return false;
            }
            return true;
        }
        /// <summary>
        /// Function validates if the password is not null, is suffitient lenght,
        /// has uppercase, lowercase, digits and special characters.
        /// </summary>
        /// <param name="passwordToValidate"></param>
        /// <returns>True if the password is valid, false othervise</returns>
        public static bool IsValidPassword(string? passwordToValidate)
        {
            if (string.IsNullOrWhiteSpace(passwordToValidate)) return false;
            const int MIN_PASSWORD_LENGHT = 8;
            if (passwordToValidate.Length < MIN_PASSWORD_LENGHT) return false;

            // Regex: At least one upper, one lower, one digit, one special, min length 8
            System.Text.RegularExpressions.Regex regex = new(
                @"^(?=.*[A-Z])(?=.*[a-z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,}$"
            );
            return regex.IsMatch(passwordToValidate);
        }

        /// <summary>
        /// Function validates if the phone number is not null and if it is in the 
        /// valid format - +xxxxxxxxxxxx or just xxxxxxxxxx
        /// </summary>
        /// <param name="phoneNumberToValidate"></param>
        /// <returns>True if the phone number is valid, false othervise</returns>
        public static bool IsValidPhoneNumber(string? phoneNumberToValidate)
        {
            if (string.IsNullOrWhiteSpace(phoneNumberToValidate)) return false;

            // International format: + followed by 11-15 digits (e.g. +381649781191)
            // Serbian format: 0 followed by 9 digits (e.g. 0649171191)
            var internationalPattern = @"^\+[\d]{11,15}$";
            var serbianPattern = @"^0\d{9}$";

            return System.Text.RegularExpressions.Regex.IsMatch(phoneNumberToValidate, internationalPattern)
                || System.Text.RegularExpressions.Regex.IsMatch(phoneNumberToValidate, serbianPattern);
        }

        /// <summary>
        /// Function validates that a date is not in the past
        /// </summary>
        /// <param name="dateToValidate">Date to validate</param>
        /// <returns>True if the date is today or in the future, false otherwise</returns>
        public static bool IsValidFutureDate(DateTime dateToValidate)
        {
            return dateToValidate >= DateTime.Today;
        }

        /// <summary>
        /// Function validates that a date is within a reasonable future range
        /// </summary>
        /// <param name="dateToValidate">Date to validate</param>
        /// <param name="maxYearsInFuture">Maximum years in the future (default: 10)</param>
        /// <returns>True if the date is within the valid future range</returns>
        public static bool IsValidFutureDate(DateTime dateToValidate, int maxYearsInFuture = 10)
        {
            var maxDate = DateTime.Today.AddYears(maxYearsInFuture);
            return dateToValidate >= DateTime.Today && dateToValidate <= maxDate;
        }

        /// <summary>
        /// Function validates that a date is not null and is within a reasonable range
        /// </summary>
        /// <param name="dateToValidate">Date to validate</param>
        /// <param name="minDate">Minimum allowed date (optional)</param>
        /// <param name="maxDate">Maximum allowed date (optional)</param>
        /// <returns>True if the date is valid</returns>
        public static bool IsValidDate(DateTime dateToValidate, DateTime? minDate = null, DateTime? maxDate = null)
        {
            minDate ??= DateTime.Today;
            maxDate ??= DateTime.Today.AddYears(10);

            return dateToValidate >= minDate && dateToValidate <= maxDate;
        }

        /// <summary>
        /// Function validates that an integer is within a specified range
        /// </summary>
        /// <param name="numberToValidate">Number to validate</param>
        /// <param name="min">Minimum value (inclusive)</param>
        /// <param name="max">Maximum value (inclusive)</param>
        /// <returns>True if the number is within range, false otherwise</returns>
        public static bool IsValidIntegerRange(int numberToValidate, int min, int max)
        {
            return numberToValidate >= min && numberToValidate <= max;
        }

        /// <summary>
        /// Function that checks that a date passed is in the past.
        /// </summary>
        /// <param name="dateOnly"></param>
        /// <returns>True if the date is in the past, false otherwise</returns>
        public static bool IsInThePast(DateOnly dateOnly)
        {
            return dateOnly < DateOnly.FromDateTime(DateTime.Today);
        }
    }
}
