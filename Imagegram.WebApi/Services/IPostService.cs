using Imagegram.WebApi.Model;

namespace Imagegram.WebApi.Services
{
    public interface IPostService
    {
        Task<GetPostResponse> GetPostAsync(GetPostRequest request);
        Task<GetCommentResponse> GetCommentAsync(GetCommentRequest request);
        Task<CreateCommentResponse> CreateCommentAsync(CreateCommentRequest request);
        Task<DeleteCommentResponse> DeleteCommentAsync(DeleteCommentRequest request);
        Task<CreatePostResponse> CreatePostAsync(CreatePostRequest request);
    }
}