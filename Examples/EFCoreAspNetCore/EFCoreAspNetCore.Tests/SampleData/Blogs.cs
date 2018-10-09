using EFCoreAspNetCore.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace EFCoreAspNetCore.Tests.SampleData
{
    public static class Blogs
    {
        public static Blog Generic()
        {
            return new Blog
            {
                Name = $"Blog " + Generator.Id<string>(),
                Url = $"Http://www.{Generator.Id<string>()}.org",
                Tags = new List<Tag>
                {
                    new Tag { Name = "A" },
                    new Tag { Name = "B" },
                    new Tag { Name = "C" }
                }
            };
        }
    }
}
