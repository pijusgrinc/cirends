using System.ComponentModel.DataAnnotations;

namespace CirendsAPI.Attributes
{
    public class PositiveIdAttribute : ValidationAttribute
    {
        public override bool IsValid(object? value)
        {
            if (value == null)
                return false;

            if (value is int intValue)
                return intValue > 0;

            if (int.TryParse(value.ToString(), out int parsedValue))
                return parsedValue > 0;

            return false;
        }

        public override string FormatErrorMessage(string name)
        {
            return $"{name} must be a positive integer greater than 0.";
        }
    }
}