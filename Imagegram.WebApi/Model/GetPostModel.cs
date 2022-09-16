using System.Text.Json.Serialization;

namespace Imagegram.WebApi.Model
{
    public class GetPostRequest
    {
        public int Limit { get; set; }
        public string Cursor { get; set; }
    }
    public class GetPostResponse : BaseResponse
    {

        public GetPostResponse(int status, PostModel[] posts = null, string cursor = null) : base(status)
        {
            Posts = posts;
            Cursor = cursor;
        }
        public PostModel[] Posts { get; set; }
        public string Cursor { get; set; }
    }
}
