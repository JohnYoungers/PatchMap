using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EFCoreWebApi.Data
{
    public class Tag
    {
        public string Name { get; set; }

        public int BlogId { get; set; }
        public Blog Blog { get; set; }
    }
}
