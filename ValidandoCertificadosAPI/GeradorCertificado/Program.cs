using CertificateManager;
using CertificateManager.Models;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;

namespace GeradorCertificado
{
    class Program
    {
        public const string DNS = "localhost"; //"de-vw-dv-ap-179.repom.com.br";
        private const string DIRETORIO= @"c:\temp\certificado\";
        private static string DIRETORIO_COMPLETO = Path.Combine(DIRETORIO, DNS);
        private const string COUNTRY = "BR";
        private const string password = "123";

        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            GeraCertificado();
        }

        static void GeraCertificado()
        {
            if (!Directory.Exists(DIRETORIO_COMPLETO))
            {
                Directory.CreateDirectory(DIRETORIO_COMPLETO);
            }

            var serviceProvider = new ServiceCollection()
                    .AddCertificateManager()
                    .BuildServiceProvider();

            var createClientServerAuthCerts =
                serviceProvider.GetService<CreateCertificatesClientServerAuth>();

            var root = createClientServerAuthCerts.NewRootCertificate(
                new DistinguishedName
                {
                    CommonName = $"root_{DNS}",
                    Country = COUNTRY 
                },
                new ValidityPeriod
                {
                    ValidFrom = DateTime.UtcNow,
                    ValidTo = DateTime.UtcNow.AddYears(10)
                },
                3, DNS);

            root.FriendlyName = $"root_{DNS} certificate";

            var importExportCertificate = serviceProvider.GetService<ImportExportCertificate>();

            var rootCertInPfxBtyes = importExportCertificate.ExportRootPfx(password, root);
            File.WriteAllBytes(Path.Combine(DIRETORIO_COMPLETO ,$"root_{DNS}.pfx"), rootCertInPfxBtyes);

            var intermediate = createClientServerAuthCerts
                    .NewIntermediateChainedCertificate(

                    new DistinguishedName
                    {
                        CommonName = $"intermediate_{DNS}",
                        Country = COUNTRY
                    },

                    new ValidityPeriod
                    {
                        ValidFrom = DateTime.UtcNow,
                        ValidTo = DateTime.UtcNow.AddYears(10)
                    },
                    2, DNS, root);

            intermediate.FriendlyName = $"intermediate_{DNS} certificate";

            importExportCertificate = serviceProvider.GetService<ImportExportCertificate>();

            var intermediateCertInPfxBtyes = importExportCertificate.ExportChainedCertificatePfx(password, intermediate, root);
            File.WriteAllBytes(Path.Combine(DIRETORIO_COMPLETO, $"intermediate_{DNS}.pfx"), intermediateCertInPfxBtyes);

            var client = createClientServerAuthCerts.NewClientChainedCertificate(
                new DistinguishedName 
                { 
                    CommonName = $"client_{DNS}", 
                    Country = COUNTRY  
                },
                new ValidityPeriod 
                { 
                    ValidFrom = DateTime.UtcNow, 
                    ValidTo = DateTime.UtcNow.AddYears(10) 
                },
                DNS, intermediate);
            client.FriendlyName = $"client_{DNS} certificate";

            importExportCertificate = serviceProvider.GetService<ImportExportCertificate>();

            var clientCertInPfxBtyes = importExportCertificate.ExportChainedCertificatePfx(password, client, intermediate);
            File.WriteAllBytes(Path.Combine(DIRETORIO_COMPLETO, $"client_{DNS}.pfx"), clientCertInPfxBtyes);

        }
    }
}
