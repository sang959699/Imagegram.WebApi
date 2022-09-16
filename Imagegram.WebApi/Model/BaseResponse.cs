namespace Imagegram.WebApi.Model
{
    public class BaseResponse
    {
        public BaseResponse(int status, string message = null)
        {
            Status = status;
            if (string.IsNullOrWhiteSpace(message))
            {
                message = status switch
                {
                    1 => ApplicationConstants.ResponseMessage.Success,
                    2 => ApplicationConstants.ResponseMessage.Failed,
                    _ => ApplicationConstants.ResponseMessage.Pending,
                };
            }
            Message = message;
        }
        public int Status { get; set; }
        public string Message { get; set; }
    }
}
