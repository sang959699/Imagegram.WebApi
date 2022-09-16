using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Imagegram.WebApi.Helper;
using Imagegram.WebApi.Model;
using System.Globalization;
using System.Text.Json;

namespace Imagegram.WebApi.Services
{
    public class PostService : IPostService
    {
        private readonly IAmazonDynamoDB dynamoDB;
        private readonly IAmazonS3 s3Client;
        public PostService(IAmazonDynamoDB dynamoDB, IAmazonS3 s3Client)
        {
            this.dynamoDB = dynamoDB;
            this.s3Client = s3Client;
        }
        public async Task<CreatePostResponse> CreatePostAsync(CreatePostRequest request)
        {
            return await Task.Run(() => CreatePost(request));
        }
        private async Task<CreatePostResponse> CreatePost(CreatePostRequest request)
        {
            try
            {
                var postId = Guid.NewGuid().ToString();
                var splitedImageData = request.ImageDataUri.Split(';');
                var mimeType = splitedImageData[0].Replace("data:", "");
                var extension = "jpg";
                if (mimeType == "image/x-ms-bmp") extension = "bmp";
                else if (mimeType == "image/jpeg") extension = "jpg";
                else if (mimeType == "image/png") extension = "png";
                if (!ApplicationConstants.Config.AllowedFileFormat.Split(";").Contains(extension)) throw new Exception("Only support .jpg, .png, .bmp");
                using (var stream = new MemoryStream(Convert.FromBase64String(splitedImageData[1].Split(',')[1])))
                {
                    if (stream.Capacity > ApplicationConstants.Config.SizeRestriction) throw new Exception("Image must be smaller than 100MB");
                    var s3Request = new PutObjectRequest
                    {
                        BucketName = ApplicationConstants.Config.BucketName,
                        InputStream = stream,
                        ContentType = mimeType,
                        Key = $"{postId}.{extension}"
                    };
                    await s3Client.PutObjectAsync(s3Request);

                    using (var originStream = new MemoryStream(Convert.FromBase64String(splitedImageData[1].Split(',')[1])))
                    {
                        var resizedImage = ImageHelper.ResizeImage(originStream, 600, 600);

                        s3Request = new PutObjectRequest
                        {
                            BucketName = ApplicationConstants.Config.BucketName,
                            InputStream = resizedImage,
                            ContentType = "image/jpg",
                            Key = $"{postId}_resized.jpg"
                        };
                        await s3Client.PutObjectAsync(s3Request);
                    }
                }

                var createRecord = new PutItemRequest
                {
                    TableName = ApplicationConstants.TableName.Post,
                    Item = new Dictionary<string, AttributeValue>
                    {
                        { "PostId", new AttributeValue{ S = postId }},
                        { "Caption", new AttributeValue{ S = request.Caption }},
                        { "ImageUrl", new AttributeValue{ S = $"{ApplicationConstants.Config.S3BucketUrl}/{postId}_resized.jpg" }},
                        { "CreatedDt", new AttributeValue{ S = DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture) }},
                    }
                };
                var result = await dynamoDB.PutItemAsync(createRecord);
                return new CreatePostResponse(ApplicationConstants.ResponseStatus.Success);
            }
            catch (Exception ex)
            {
                return new CreatePostResponse(ApplicationConstants.ResponseStatus.Failed, ex.Message);
            }
        }
        public async Task<GetPostResponse> GetPostAsync(GetPostRequest request)
        {
            return await Task.Run(() => GetPost(request));
        }
        private async Task<GetPostResponse> GetPost(GetPostRequest request)
        {
            try
            {
                var posts = new List<PostModel>();
                var scanRecord = new ScanRequest
                {
                    TableName = ApplicationConstants.TableName.Post
                };
                var scanItemResult = await dynamoDB.ScanAsync(scanRecord);
                foreach (Dictionary<string, AttributeValue> item in scanItemResult.Items)
                {
                    item.TryGetValue("PostId", out var postId);
                    item.TryGetValue("Caption", out var caption);
                    item.TryGetValue("Comments", out var comments);
                    item.TryGetValue("CreatedDt", out var createdDt);
                    item.TryGetValue("ImageUrl", out var imageUrl);
                    posts.Add(new PostModel
                    {
                        PostId = postId?.S,
                        Caption = caption?.S,
                        CreateDt = createdDt?.S,
                        ImageUrl = imageUrl?.S,
                        Comments = JsonSerializer.Deserialize<CommentModel[]>(comments?.S ?? "[]")
                    });
                }
                var cursor = String.Empty;
                if (posts.Count > 0)
                {
                    posts = posts.OrderByDescending(o => o.Comments.Length).ThenBy(o => o.CreateDt).ToList();
                    var skipPosition = posts.FindIndex(f => f.PostId == request.Cursor);
                    if (skipPosition != 0) skipPosition++;
                    posts = posts.Skip(skipPosition).Take(request.Limit).ToList();
                    if (posts.Count > 0) cursor = posts.Last().PostId;
                }

                return new GetPostResponse(ApplicationConstants.ResponseStatus.Success, posts.ToArray(), cursor);
            }
            catch
            {
                return new GetPostResponse(ApplicationConstants.ResponseStatus.Failed);
            }
        }

        public async Task<GetCommentResponse> GetCommentAsync(GetCommentRequest request)
        {
            return await Task.Run(() => GetComment(request));
        }

        private async Task<GetCommentResponse> GetComment(GetCommentRequest request)
        {
            try
            {
                var getRecord = new GetItemRequest
                {
                    TableName = ApplicationConstants.TableName.Post,
                    Key = new Dictionary<string, AttributeValue>
                    {
                        { "PostId", new AttributeValue { S = request.PostId }}
                    }
                };
                var getItemResult = await dynamoDB.GetItemAsync(getRecord);
                getItemResult.Item.TryGetValue("Comments", out var commentStr);

                var commentModel = Array.Empty<CommentModel>();
                if (commentStr != null)
                {
                    var comments = JsonSerializer.Deserialize<List<CommentModel>>(commentStr?.S);
                    commentModel = comments.Where(w => !w.IsDeleted).ToArray();
                }
                
                return new GetCommentResponse(ApplicationConstants.ResponseStatus.Success, commentModel);
            }
            catch
            {
                return new GetCommentResponse(ApplicationConstants.ResponseStatus.Failed);
            }
        }

        public async Task<CreateCommentResponse> CreateCommentAsync(CreateCommentRequest request)
        {
            return await Task.Run(() => CreateComment(request));
        }

        private async Task<CreateCommentResponse> CreateComment(CreateCommentRequest request)
        {
            try
            {
                var getCommentResult = await GetComment(new GetCommentRequest { PostId = request.PostId });

                if (getCommentResult.Status != ApplicationConstants.ResponseStatus.Success) throw new Exception("Failed to retrieve Comments");

                var comments = getCommentResult.Comments.ToList();

                comments.Add(new CommentModel
                {
                    CommentId = Guid.NewGuid().ToString(),
                    Content = request.Comment,
                    CreatedDt = DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture),
                });

                var updateRecord = new UpdateItemRequest
                {
                    TableName = ApplicationConstants.TableName.Post,
                    Key = new Dictionary<string, AttributeValue>
                    {
                        { "PostId", new AttributeValue { S = request.PostId }}
                    },
                    ExpressionAttributeNames = new Dictionary<string, string>()
                    {
                        { "#C", "Comments" },
                    },
                    ExpressionAttributeValues = new Dictionary<string, AttributeValue>()
                    {
                        { ":comments", new AttributeValue { S = JsonSerializer.Serialize(comments) } },
                    },
                    UpdateExpression = "SET #C = :comments"
                };

                var response = await dynamoDB.UpdateItemAsync(updateRecord);

                return new CreateCommentResponse(ApplicationConstants.ResponseStatus.Success);
            }
            catch
            {
                return new CreateCommentResponse(ApplicationConstants.ResponseStatus.Failed);
            }
        }

        public async Task<DeleteCommentResponse> DeleteCommentAsync(DeleteCommentRequest request)
        {
            return await Task.Run(() => DeleteComment(request));
        }

        private async Task<DeleteCommentResponse> DeleteComment(DeleteCommentRequest request)
        {
            try
            {
                var getRecord = new GetItemRequest
                {
                    TableName = ApplicationConstants.TableName.Post,
                    Key = new Dictionary<string, AttributeValue>
                    {
                        { "PostId", new AttributeValue { S = request.PostId }}
                    }
                };
                var getItemResult = await dynamoDB.GetItemAsync(getRecord);
                getItemResult.Item.TryGetValue("Comments", out var commentStr);

                List<CommentModel> comments = null;
                if (commentStr != null)
                {
                    comments = JsonSerializer.Deserialize<List<CommentModel>>(commentStr?.S);
                    if (!comments.Any(a => a.CommentId == request.CommentId)) throw new Exception("Failed to retrieve comment");
                    comments.Single(w => w.CommentId == request.CommentId).IsDeleted = true;
                }

                var updateRecord = new UpdateItemRequest
                {
                    TableName = ApplicationConstants.TableName.Post,
                    Key = new Dictionary<string, AttributeValue>
                    {
                        { "PostId", new AttributeValue { S = request.PostId }}
                    },
                    ExpressionAttributeNames = new Dictionary<string, string>()
                    {
                        { "#C", "Comments" }
                    },
                    ExpressionAttributeValues = new Dictionary<string, AttributeValue>()
                    {
                        { ":comments", new AttributeValue { S = JsonSerializer.Serialize(comments) } }
                    },
                    UpdateExpression = "SET #C = :comments"
                };

                var response = await dynamoDB.UpdateItemAsync(updateRecord);

                return new DeleteCommentResponse(ApplicationConstants.ResponseStatus.Success);
            }
            catch
            {
                return new DeleteCommentResponse(ApplicationConstants.ResponseStatus.Failed);
            }
        }
    }
}
