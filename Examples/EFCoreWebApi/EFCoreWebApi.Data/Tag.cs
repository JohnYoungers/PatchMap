using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EFCoreWebApi.Data
{
    public class Tag
    {
        [Required, MaxLength(100)]
        public string Name { get; set; }

        public int BlogId { get; set; }
        public Blog Blog { get; set; }
    }
}
