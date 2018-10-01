using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace EFCoreWebApi.Data
{
    public class Post
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PostId { get; set; }

        public string Title { get; set; }
        public DateTimeOffset DateCreated { get; set; }
        public string Content { get; set; }

        public int BlogId { get; set; }
        public Blog Blog { get; set; }
    }
}
