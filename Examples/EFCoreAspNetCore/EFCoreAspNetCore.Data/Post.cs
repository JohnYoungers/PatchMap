using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EFCoreAspNetCore.Data
{
    public class Post
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PostId { get; set; }

        [Required, MaxLength(150)]
        public string Title { get; set; }
        [Required]
        public DateTimeOffset DateCreated { get; set; }
        [Required]
        public string Content { get; set; }

        //Instead of editing this post, we're forcing them to create a new one and redirect this to the one
        public DateTimeOffset? UpdatedAsOfDate { get; set; }
        public int? UpdatedPostId { get; set; }
        public Post UpdatedPost { get; set; }

        public int BlogId { get; set; }
        public Blog Blog { get; set; }
    }
}
