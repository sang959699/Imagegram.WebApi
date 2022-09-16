namespace Imagegram.WebApi.Model
{
    public class CreatePostRequest
    {
        public string Caption { get; set; }
        public string ImageDataUri { get; set; }
    }
    public class CreatePostResponse : BaseResponse
    {
        public CreatePostResponse(int status, string message = null) : base(status, message)
        {
        }
    }
}
