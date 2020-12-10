using Customer.Data;
using Customer.Repository;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Customer.OrderFacade;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Logging;
using Polly;
using System.Runtime.Serialization;
using Microsoft.AspNetCore.Authentication;

namespace Customer.AccountAPI
{
    public class Startup
    {
        public Startup(IConfiguration configuration, Microsoft.AspNetCore.Hosting.IHostingEnvironment env)
        {
            Configuration = configuration;
            Env = env;
        }

        public IConfiguration Configuration { get; }

        private Microsoft.AspNetCore.Hosting.IHostingEnvironment Env { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
            services.AddAuthentication("Bearer")
            .AddJwtBearer("Bearer", options =>
            {
                options.Authority = getAuthority();
                options.Audience = "customer_account_api";
            });
            services.AddAuthorization(OptionsBuilderConfigurationExtensions =>
            {
                OptionsBuilderConfigurationExtensions.AddPolicy("CustomerOnly", builder =>
                {
                    builder.RequireClaim("role", "Customer");
                });
            });

            services.AddControllers();
            services.AddAutoMapper(typeof(Startup));
            services.AddDbContext<CustomerDb>(options => options.UseSqlServer(
                 Configuration.GetConnectionString("CustomerAccount"), optionsBuilder =>
                 {
                     optionsBuilder.EnableRetryOnFailure(10, TimeSpan.FromSeconds(10), null);
                 }));
            services.AddScoped<ICustomerRepository, CustomerRepository>();
            if (Env.IsDevelopment())
            {
                services.AddScoped<IOrderFacade, FakeOrderFacade>();
            }
            else
            {
                services.AddScoped<IOrderFacade, OrderFacade.OrderFacade>();
            }
            string customerOrderingClientName = "CustomerOrderingAPI";
            services.AddHttpClient(customerOrderingClientName, client =>
            {
                client.BaseAddress = new Uri(getUri(customerOrderingClientName));
            })
                    .AddTransientHttpErrorPolicy(p => p.OrResult(
                        msg => msg.StatusCode == System.Net.HttpStatusCode.NotFound)
                    .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))))
                    .AddTransientHttpErrorPolicy(p => p.CircuitBreakerAsync(5, TimeSpan.FromSeconds(30)));
        }

        private string getUri(string clientName)
        {
            if (Env.IsDevelopment())
            {
               if (clientName.Equals("CustomerOrderingAPI"))
                {
                    return "http://localhost:50448";
                }
            }
            else
            {
                if (clientName.Equals("CustomerOrderingAPI"))
                {
                    return "https://customerorderingthamco.azurewebsites.net";
                }
            }
            return "";
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        private string getAuthority()
        {
            if (Env.IsDevelopment())
            {
                return "https://localhost:43389";
            }
            return "https://thamcocustomerauth.azurewebsites.net/";
        }
    }
}
