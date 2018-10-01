using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace EFCoreWebApi.Data
{
    public class ExampleContext : DbContext
    {
        public ExampleContext(DbContextOptions<ExampleContext> options) : base(options) { }

        public DbSet<Blog> Blogs { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<Tag> Tags { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Blog>().HasData(new Blog
            {
                BlogId = 1,
                Name = "Seed Blog",
                Url = "http://sample.com"
            });

            modelBuilder.Entity<Post>().HasData(new Post
            {
                BlogId = 1,
                PostId = 1,
                DateCreated = DateTime.Now,
                Title = "First Post",
                Content = "This is the first post!"
            });

            modelBuilder.Entity<Tag>().HasKey(c => new { c.BlogId, c.Name });
            modelBuilder.Entity<Tag>().HasData(
                new Tag { BlogId = 1, Name = "Tag 1" },
                new Tag { BlogId = 1, Name = "Tag 2" }
            );
        }
    }
}
