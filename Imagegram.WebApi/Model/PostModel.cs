namespace Imagegram.WebApi.Model
{
    public class PostModel
    {
        public string PostId { get; set; }
        public string Caption { get; set; }
        public CommentModel[] Comments { get; set; }
        public string ImageUrl { get; set; }
        public string CreateDt { get; set; }
        public bool IsDeleted { get; set; }
        public string DeletedDt { get; set; }
    }
}
