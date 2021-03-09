using Microsoft.AspNetCore.Authentication.Certificate;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace API
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
            services.AddTransient<ValidadorCertificado,ValidadorCertificado>();

            services.AddAuthentication(
                    CertificateAuthenticationDefaults.AuthenticationScheme)
                    .AddCertificate(options =>
                    {
                        options.AllowedCertificateTypes = CertificateTypes.Chained;
                        options.RevocationMode = X509RevocationMode.NoCheck;

                        options.Events = new CertificateAuthenticationEvents
                        {
                            OnCertificateValidated = context =>
                            {
                                var validationService =
                                      context.HttpContext.RequestServices
                                          .GetRequiredService<ValidadorCertificado>();

                                if (!validationService.Validar(context.ClientCertificate)){
                                    context.Fail("certificado invalido.");
                                };

                                return Task.CompletedTask;
                            },
                            OnAuthenticationFailed = context =>
                            {
                                var validationService =
                                 context.HttpContext.RequestServices
                                     .GetRequiredService<ValidadorCertificado>();
                                var informacao = context;

                                validationService.GravaErro(informacao);

                                return Task.CompletedTask;
                            }
                        };
                    });

            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseAuthentication();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

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
