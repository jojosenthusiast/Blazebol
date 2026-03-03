using System;
using System.Text.RegularExpressions;

namespace ControlTorneosFutbol.Helpers
{
    public static class ValidationHelper
    {
        public static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                var regex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
                return regex.IsMatch(email);
            }
            catch
            {
                return false;
            }
        }

        public static bool IsValidPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                return false;

            // Mínimo 8 caracteres, al menos una mayúscula, una minúscula y un número
            var regex = new Regex(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,}$");
            return regex.IsMatch(password);
        }

        public static bool IsValidPhoneNumber(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return true; // Opcional

            var regex = new Regex(@"^\+?[\d\s\-\(\)]{7,15}$");
            return regex.IsMatch(phone);
        }

        public static bool IsValidColorHex(string color)
        {
            if (string.IsNullOrWhiteSpace(color))
                return true; // Opcional

            var regex = new Regex(@"^#[0-9A-Fa-f]{6}$");
            return regex.IsMatch(color);
        }

        public static bool IsValidDateRange(DateTime startDate, DateTime endDate)
        {
            return endDate > startDate;
        }

        public static bool IsValidAge(DateTime birthDate, int minAge = 0, int maxAge = 150)
        {
            var age = DateTime.Today.Year - birthDate.Year;
            if (birthDate.Date > DateTime.Today.AddYears(-age))
                age--;

            return age >= minAge && age <= maxAge;
        }
    }
}