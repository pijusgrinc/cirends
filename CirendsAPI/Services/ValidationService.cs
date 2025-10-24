using CirendsAPI.Exceptions;

namespace CirendsAPI.Services
{
    public interface IValidationService
    {
        void ValidateId(int id, string parameterName = "ID");
        void ValidatePositiveId(int id, string parameterName = "ID");
    }

    public class ValidationService : IValidationService
    {
        public void ValidateId(int id, string parameterName = "ID")
        {
            if (id <= 0)
                throw new ArgumentException($"{parameterName} must be a positive integer greater than 0.");
        }

        public void ValidatePositiveId(int id, string parameterName = "ID")
        {
            if (id <= 0)
                throw new ArgumentException($"{parameterName} must be a positive integer greater than 0.");
        }
    }
}
