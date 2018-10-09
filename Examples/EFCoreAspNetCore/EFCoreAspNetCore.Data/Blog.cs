using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EFCoreAspNetCore.Data
{
    public class Blog
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int BlogId { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; }

        [Required, MaxLength(150)]
        public string Url { get; set; }

        public List<Tag> Tags { get; set; } = new List<Tag>();
        public List<Post> Posts { get; set; } = new List<Post>();

        public int? PromotedPostId { get; set; }
        public Post PromotedPost { get; set; }
    }
}
