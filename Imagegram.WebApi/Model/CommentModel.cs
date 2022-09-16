using System.Text.Json.Serialization;

namespace Imagegram.WebApi.Model
{
    public class CommentModel
    {
        public string CommentId { get; set; }
        public string Content { get; set; }
        public string CreatedDt { get; set; }
        public bool IsDeleted { get; set; }
    }
}
