using System;
using System.Threading.Tasks;
using EasyNetQ;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microtopia.Dto;
using ServiceStack.Data;
using ServiceStack.OrmLite;
using StructureMap;
using Swashbuckle.AspNetCore.Swagger;
using IServiceProvider = System.IServiceProvider;

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
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();

            services.AddSwaggerGen(c => { c.SwaggerDoc("v1", new Info {Title = "Emergency API", Version = "v1"}); });

            var db = new OrmLiteConnectionFactory(":memory:", SqliteDialect.Provider);
            using (var sess = db.Open())
                sess.CreateTable<Emergency>();

            services.AddSingleton<IDbConnectionFactory>(db);
            services.AddSingleton(RabbitHutch.CreateBus("host=localhost;username=guest;password=guest"));

            // Microsoft dependency injection does not support polymorphism
            services.AddMediatR(typeof(DummyCommandAndEventSink), typeof(Emergency));

            var container = new Container();

            container.Populate(services);

            var bus = container.GetInstance<IBus>();
            bus.Subscribe<AlarmElapsed>("Emergency", @event => { container.GetInstance<IMediator>().Publish(@event); });
            bus.Subscribe<ChannelMessageReceived>("Emergency",
                @event => { container.GetInstance<IMediator>().Publish(@event); });

            // mock outside service
            bus.SubscribeAsync<Alarm>("Alarm", async cmd =>
            {
                var timeSpan = cmd.Time - DateTime.Now;

                if (timeSpan < new TimeSpan(0))
                    timeSpan = new TimeSpan(0);

                await Task.Delay(timeSpan).ContinueWith(t => bus.Publish(new AlarmElapsed {Flow = cmd.Flow}));
            });

            bus.Subscribe<AlarmCancel>("Alarm", Console.WriteLine);

            bus.SubscribeAsync<ChannelMessage>("ChannelMessage",
                async cmd =>
                {
                    await Task.Delay(10000)
                        .ContinueWith(t => bus.Publish(new ChannelMessageReceived {Flow = cmd.Flow}));
                });


            return container.GetInstance<IServiceProvider>();
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