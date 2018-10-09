using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EF6AspNetWebApi.Data
{
    public class ExampleContext : DbContext
    {
        public DbSet<Blog> Blogs { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<Tag> Tags { get; set; }

        public ExampleContext() : base(@"Server=(localdb)\mssqllocaldb;Database=EF6;Trusted_Connection=True;ConnectRetryCount=0")
        {
            Database.SetInitializer(new ExampleContextInitializer());

            Database.Log = (s => Debug.WriteLine(s));
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Blog>().HasMany(c => c.Posts).WithRequired(c => c.Blog);
            modelBuilder.Entity<Tag>().HasKey(c => new { c.BlogId, c.Name });

            //ExampleContextMetadata.BuildMetadata(this);
        }
    }

    public class ExampleContextInitializer : DropCreateDatabaseIfModelChanges<ExampleContext>
    {
        protected override void Seed(ExampleContext context)
        {
            context.Blogs.Add(new Blog
            {
                BlogId = 1,
                Name = "Seed Blog",
                Url = "http://sample.com"
            });

            context.Posts.AddRange(new[]
            {
                new Post
                {
                    BlogId = 1,
                    PostId = 1,
                    DateCreated = new DateTimeOffset(new DateTime(2018, 6, 1)),
                    Title = "First Post",
                    Content = "This is the first post!"
                }, new Post
                {
                    BlogId = 1,
                    PostId = 2,
                    DateCreated = new DateTimeOffset(new DateTime(2018, 7, 1)),
                    Title = "Second Post",
                    Content = "This is the second post!"
                }
            });

            context.Tags.AddRange(new[]
            {
                new Tag { BlogId = 1, Name = "Tag 1" },
                new Tag { BlogId = 1, Name = "Tag 2" }
            });

            context.SaveChanges();

            var dbBlog = context.Blogs.FirstOrDefault(b => b.BlogId == 1);
            dbBlog.PromotedPostId = 1;

            var dbPost = context.Posts.FirstOrDefault(p => p.BlogId == 1 && p.PostId == 1);
            dbPost.UpdatedPostId = 2;
            dbPost.UpdatedAsOfDate = new DateTimeOffset(new DateTime(2018, 10, 1));
            context.SaveChanges();

            base.Seed(context);
        }
    }
}
