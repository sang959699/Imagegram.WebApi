namespace Imagegram.WebApi.Model
{
    public class GetCommentRequest
    {
        public string PostId { get; set; }
    }
    public class GetCommentResponse : BaseResponse
    {
        public GetCommentResponse(int status, CommentModel[] comments = null) : base(status)
        {
            this.Comments = comments;
        }

        public CommentModel[] Comments { get; set; }
    }
}
