namespace Imagegram.WebApi.Model
{
    public class CreateCommentRequest
    {
        public string PostId { get; set; }
        public string Comment { get; set; }
    }
    public class CreateCommentResponse : BaseResponse
    {
        public CreateCommentResponse(int status, string message = null) : base(status, message)
        {
        }
    }
}
