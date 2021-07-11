using System.ComponentModel.DataAnnotations;

namespace fr4nc3.com.tasks.DTO
{
    /// <summary>
    /// Error response class 
    /// </summary>
    public class ErrorResponse
    {
        /// <summary>
        /// error number from defined list
        /// </summary>
        public int errorNumber { get; set; }
        /// <summary>
        /// parameterName which created the error
        /// </summary>
        [StringLength(1024)]
        public string parameterName { get; set; }
        /// <summary>
        /// the value of the parameter that failed 
        /// </summary>
        [StringLength(2048)]
        public string parameterValue { get; set; }
        /// <summary>
        /// errr description
        /// </summary>
        [StringLength(1024)]
        public string errorDescription { get; set; }

        /// <summary>
        /// Converts an error number inside an encoded error description, to the standard error number
        /// </summary>
        /// <param name="encodedErrorDescription">The error description</param>
        /// <returns>The decoded error number</returns>
        public static int GetErrorNumberFromDescription(string encodedErrorDescription)
        {
            if (int.TryParse(encodedErrorDescription, out int errorNumber))
            {
                return errorNumber;
            }
            return 0;
        }

        /// <summary>
        /// Converts an error number inside an encoded error description, to the standard error response
        /// </summary>
        /// <param name="encodedErrorDescription">The error description</param>
        /// <returns>The decoded error message and number</returns>
        public static (string decodedErrorMessage, int decodedErrorNumber) GetErrorMessage(string encodedErrorDescription)
        {

            int errorNumber = GetErrorNumberFromDescription(encodedErrorDescription);
            // list of valid errors
            switch (errorNumber)
            {
                case 1:
                    {
                        return ("The entity already exists", errorNumber);
                    }
                case 2:
                    {
                        return ("The parameter value is too large", errorNumber);
                    }
                case 3:
                    {
                        return ("The parameter is required", errorNumber);
                    }
                case 4:
                    {
                        return ("The maximum number of entities have been created. No further entities can be created at this time.", errorNumber);
                    }
                case 5:
                    {
                        return ("The entity could not be found", errorNumber);
                    }
                case 6:
                    {
                        return ("The parameter value is too small", errorNumber);
                    }
                case 7:
                    {
                        return ("The parameter value is not valid", errorNumber);
                    }
                case 8:
                    {
                        return ("The entity could not be inserted", errorNumber);
                    }
                case 9:
                    {
                        return ("The entity could not be updated", errorNumber);
                    }
                case 10:
                    {
                        return ("The entity could not be deleted", errorNumber);
                    }
                case 11:
                    {
                        return ("Internal server error ", errorNumber);
                    }
                default:
                    {
                        return ($"Raw Error: {encodedErrorDescription}", errorNumber);
                    }

            }
        }

    }
}
