using System;
namespace Authentication_Service.Responses
{
	public class TokenValidationResponse : BaseApiResponse
	{
        public bool IsValidToken { get; set; }

        public TokenValidationResponse()
        {
            // Default constructor without arguments
        }

        public TokenValidationResponse(string defaultMessage, int statusCode, bool isValidToken, string errorMessage)
        {
            DefaultMessage = defaultMessage;
            StatusCode = statusCode;
            IsValidToken = isValidToken;
            ErrorMessage = errorMessage;
        }
    }
}

