using Imagegram.WebApi.Model;
using Imagegram.WebApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace Imagegram.WebApi.Controllers;

[Route("api/[controller]")]
public class PostController : ControllerBase
{
    private readonly IPostService postService;
    public PostController(IPostService postService)
    {
        this.postService = postService;
    }

    [HttpPost("Post/Create")]
    public async Task<IActionResult> CreatePost([FromBody] CreatePostRequest request)
    {
        try
        {
            var result = await Task.Run(() => postService.CreatePostAsync(request));
            return Ok(result);
        }
        catch (Exception)
        {
            return BadRequest();
        }
    }

    [HttpPost("Post/Get")]
    public async Task<IActionResult> GetPost([FromBody] GetPostRequest request)
    {
        try
        {
            var result = await Task.Run(() => postService.GetPostAsync(request));
            return Ok(result);
        }
        catch (Exception)
        {
            return BadRequest();
        }
    }

    [HttpPost("Comment/Get")]
    public async Task<IActionResult> GetComment([FromBody] GetCommentRequest request)
    {
        try
        {
            var result = await Task.Run(() => postService.GetCommentAsync(request));
            return Ok(result);
        }
        catch (Exception)
        {
            return BadRequest();
        }
    }

    [HttpPost("Comment/Create")]
    public async Task<IActionResult> CreateComment([FromBody] CreateCommentRequest request)
    {
        try
        {
            var result = await Task.Run(() => postService.CreateCommentAsync(request));
            return Ok(result);
        }
        catch (Exception)
        {
            return BadRequest();
        }
    }

    [HttpPost("Comment/Delete")]
    public async Task<IActionResult> DeleteComment([FromBody] DeleteCommentRequest request)
    {
        try
        {
            var result = await Task.Run(() => postService.DeleteCommentAsync(request));
            return Ok(result);
        }
        catch (Exception)
        {
            return BadRequest();
        }
    }
}