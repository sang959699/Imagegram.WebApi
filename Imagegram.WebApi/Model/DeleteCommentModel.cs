namespace Imagegram.WebApi.Model
{
    public class DeleteCommentRequest
    {
        public string PostId { get; set; }
        public string CommentId { get; set; }
    }
    public class DeleteCommentResponse : BaseResponse
    {
        public DeleteCommentResponse(int status, string message = null) : base(status, message)
        {
        }
    }
}
