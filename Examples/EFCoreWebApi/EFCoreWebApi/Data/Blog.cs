using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace EFCoreWebApi.Data
{
    public class Blog
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int BlogId { get; set; }

        public string Name { get; set; }
        public string Url { get; set; }

        public List<Tag> Tags { get; set; }
        public List<Post> Posts { get; set; }
    }
}
