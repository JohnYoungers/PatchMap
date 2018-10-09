using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EF6AspNetWebApi.Data
{
    public class Tag
    {
        [Required, MaxLength(100)]
        public string Name { get; set; }

        public int BlogId { get; set; }
        [ForeignKey(nameof(BlogId))]
        public Blog Blog { get; set; }
    }
}
