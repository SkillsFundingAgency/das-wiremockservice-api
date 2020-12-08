using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using System;
using System.Text.Json.Serialization;

namespace SFA.DAS.WireMockServiceWeb
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
            services.Configure<ApiStubOptions>(Configuration.GetSection(ApiStubOptions.ConfigSection));

            services
                .AddControllers()
                .AddJsonOptions(options =>
                    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "SFA.DAS.WireMockServiceWeb", Version = "v1" });
            });

            ConfigureWireMockService(services);

            services.AddSingleton<IDataRepository, DataRepository>();
        }

        private void ConfigureWireMockService(IServiceCollection services)
        {
            var wireMockServerBaseUrl = Configuration.GetValue<string>("WireMockServiceApiBaseUrl");
            var httpClient = new WireMockHttpClient
            {
                BaseAddress = new Uri(wireMockServerBaseUrl)
            };
            services.AddSingleton(httpClient);
            services.AddSingleton<IWireMockHttpService, WireMockHttpService>();
        }


        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseDeveloperExceptionPage();
            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "SFA.DAS.WireMockServiceWeb v1"));

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

        }
    }
}