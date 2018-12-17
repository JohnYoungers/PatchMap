using EF6AspNetWebApi.Data;
using EF6AspNetWebApi.Web.Tests.Contexts;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechTalk.SpecFlow;

namespace EF6AspNetWebApi.Web.Tests.Features.Blogs
{
    [Binding]
    public class BlogSteps
    {
        private readonly AppFeatureContext context;
        private readonly HttpContext httpContext;

        public BlogSteps(AppFeatureContext context, HttpContext httpContext)
        {
            this.context = context;
            this.httpContext = httpContext;
        }

        [Given(@"a new blog exists with placeholder blogId and first post placeholder postId")]
        public void GivenANewAgencyExistsForCompany()
        {
            using (var dbContext = Application.ServiceProvider.GetRequiredService<ExampleContext>())
            {
                var dbBlog = dbContext.Blogs.Add(EF6AspNetWebApi.Tests.SampleData.Blogs.Generic());
                dbBlog.Posts.Add(new Post { Title = "A", Content = "B", DateCreated = DateTimeOffset.Now });
                dbContext.SaveChanges();

                context.AddPlaceholderValue("blogId", dbBlog.BlogId.ToString());
                context.AddPlaceholderValue("postId", dbBlog.Posts[0].PostId.ToString());
            }

        }
    }
}
