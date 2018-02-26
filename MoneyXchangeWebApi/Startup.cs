using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Cors;
using MoneyXchangeWebApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace MoneyXchangeWebApi
{
    public class Startup
    {
        private readonly AppSettings _mySettings;

        public Startup(IConfiguration configuration, IOptions<AppSettings> settings)
        {
            Configuration = configuration;
            _mySettings = settings.Value;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<ApplicationDbContext>(options => options.UseInMemoryDatabase("testDB"));
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes("secretkey132456belatrix")
                    ),
                    ClockSkew = TimeSpan.Zero

                }
            );

            services.AddMvc();

            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy",
                    builder =>
                    builder.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .WithExposedHeaders("content-disposition")
                    .AllowAnyHeader()
                    .AllowCredentials()
                    .SetPreflightMaxAge(TimeSpan.FromSeconds(3600))
                );
            });
            services.Configure<AppSettings>(Configuration.GetSection("AppSettings"));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ApplicationDbContext context)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseCors("CorsPolicy");
            app.UseAuthentication();
            app.UseMvc();
            
            if (!context.References.Any())
            {
                context.References.AddRange(new List<References>()
                {
                    new References(){ Symbol = "USD"}
                });

                context.SaveChanges();
            }

            if (!context.Exchanges.Any())
            {
                context.Exchanges.AddRange(new List<Exchanges>()
                {
                    new Exchanges(){ Symbol = "EUR"},
                    new Exchanges(){ Symbol = "CAD"},
                    new Exchanges(){ Symbol = "GBP"},
                    new Exchanges(){ Symbol = "AUD"},
                    new Exchanges(){ Symbol = "NZD"},
                    new Exchanges(){ Symbol = "XAU"}
                });

                context.SaveChanges();
            }

        }
    }
}
