using CertificateManager;
using CertificateManager.Models;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;

namespace GeradorCertificado
{
    class Program
    {
        public const string DNS = "localhost";
        private const string DIRETORIO= @"c:\temp\certificado\";
        private static string DIRETORIO_COMPLETO = Path.Combine(DIRETORIO, DNS);

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
                    CommonName = $"root_test_{DNS}",
                    Country = "BR"
                },
                new ValidityPeriod
                {
                    ValidFrom = DateTime.UtcNow,
                    ValidTo = DateTime.UtcNow.AddYears(10)
                },
                3, DNS);

            root.FriendlyName = $"root_test_{DNS} certificate";

            string password = "19372846";
            var importExportCertificate = serviceProvider.GetService<ImportExportCertificate>();

            var rootCertInPfxBtyes = importExportCertificate.ExportRootPfx(password, root);
            File.WriteAllBytes(Path.Combine(DIRETORIO_COMPLETO ,$"root_test_{DNS}.pfx"), rootCertInPfxBtyes);

            var intermediate = createClientServerAuthCerts
                    .NewIntermediateChainedCertificate(

                    new DistinguishedName
                    {
                        CommonName = $"intermediate_test_{DNS}",
                        Country = "BR"
                    },

                    new ValidityPeriod
                    {
                        ValidFrom = DateTime.UtcNow,
                        ValidTo = DateTime.UtcNow.AddYears(10)
                    },
                    2, DNS, root);

            intermediate.FriendlyName = $"intermediate_test_{DNS} certificate";

            importExportCertificate = serviceProvider.GetService<ImportExportCertificate>();

            var intermediateCertInPfxBtyes = importExportCertificate.ExportChainedCertificatePfx(password, intermediate, root);
            File.WriteAllBytes(Path.Combine(DIRETORIO_COMPLETO, $"intermediate_test_{DNS}.pfx"), intermediateCertInPfxBtyes);

            var client = createClientServerAuthCerts.NewClientChainedCertificate(
                new DistinguishedName 
                { 
                    CommonName = $"client_test_{DNS}", 
                    Country = "BR" 
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
            File.WriteAllBytes(Path.Combine(DIRETORIO_COMPLETO, $"client_test_{DNS}.pfx"), clientCertInPfxBtyes);

        }
    }
}
