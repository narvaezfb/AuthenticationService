using System;
namespace Authentication_Service.Responses
{
    public abstract class BaseApiResponse
    {
        public string? DefaultMessage { get; set; }
        public int StatusCode { get; set; }
        public string? ErrorMessage { get; set; }
    }
}

