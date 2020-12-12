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
using Microsoft.AspNetCore.Authorization;
using IdentityModel.Client;

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

            services.AddAuthentication()
                .AddJwtBearer("CustomerAuth", options =>
                {
                    options.Authority = Configuration.GetValue<string>("CustomerAuthServerUrl");
                    options.Audience = "customer_account_api";
                })
                .AddJwtBearer("StaffAuth", options =>
                    {
                        options.Authority = Configuration.GetValue<string>("StaffAuthServerUrl");
                        options.Audience = "customer_account_api";
                    });

            services.AddAuthorization(OptionsBuilderConfigurationExtensions =>
            {
                OptionsBuilderConfigurationExtensions.DefaultPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .AddAuthenticationSchemes("CustomerAuth", "StaffAuth")
                .Build();

                OptionsBuilderConfigurationExtensions.AddPolicy("CustomerOnly", new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .AddAuthenticationSchemes("CustomerAuth")
                    .RequireClaim("role", "Customer")
                    .Build());
                OptionsBuilderConfigurationExtensions.AddPolicy("StaffOnly", new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .AddAuthenticationSchemes("StaffAuth")
                    .RequireClaim("role", "Staff")
                    .Build());
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
            
            services.AddScoped<ProtocolResponse, DiscoveryDocumentResponse>();


            services.AddHttpClient("CustomerOrderingAPI", client =>
            {
                client.BaseAddress = new Uri(Configuration.GetValue<string>("CustomerOrderingUrl"));
            })
                    .AddTransientHttpErrorPolicy(p => p.OrResult(
                        msg => msg.StatusCode == System.Net.HttpStatusCode.NotFound)
                    .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))))
                    .AddTransientHttpErrorPolicy(p => p.CircuitBreakerAsync(5, TimeSpan.FromSeconds(30)));

/*            services.AddHttpClient("StaffAuthServer", client =>
            {
                client.BaseAddress = new Uri(Configuration.GetValue<string>("StaffAuthServerUrl"));
            })
                    .AddTransientHttpErrorPolicy(p => p.OrResult(
                        msg => msg.StatusCode == System.Net.HttpStatusCode.NotFound)
                    .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))))
                    .AddTransientHttpErrorPolicy(p => p.CircuitBreakerAsync(5, TimeSpan.FromSeconds(30)));*/
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
    }
}
