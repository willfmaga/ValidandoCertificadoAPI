using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace API
{
    public class Program
    {

        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();

                    var cert = CarregaCertificado();

                    webBuilder.ConfigureKestrel(o =>
                    {
                        o.ConfigureHttpsDefaults(o =>
                        {
                            o.ServerCertificate = cert;
                            o.ClientCertificateMode =
                            ClientCertificateMode.RequireCertificate;

                        });
                    });

                });

        internal static X509Certificate2 CarregaCertificado()
        {
            var store = new X509Store(StoreName.Root, StoreLocation.LocalMachine);
            store.Open(OpenFlags.OpenExistingOnly | OpenFlags.ReadWrite);

            var certificates = new X509Certificate2Collection(store.Certificates);

            //certificates.Find(X509FindType.FindByIssuerName, clientCertificate.IssuerName, true)[0];
            return certificates.Cast<X509Certificate2>().Where(x => x.Subject.Contains("root")).FirstOrDefault();
        }
    }
}
