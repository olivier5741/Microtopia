using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.Swagger;

//using Swashbuckle.AspNetCore.Swagger;

namespace Microtopia.Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();

            services.AddMediatR(typeof(Startup));

            services.AddSwaggerGen(c => { c.SwaggerDoc("v1", new Info {Title = "Emergency API", Version = "v1"}); });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment()) app.UseDeveloperExceptionPage();


            app.UseSwagger(c => { c.RouteTemplate = "api-docs/{documentName}/swagger.json"; });

            app.UseReDoc(c =>
            {
                c.RoutePrefix = "api-docs";
                c.SpecUrl = "v1/swagger.json";
            });

            app.UseMvc();
        }
    }
}