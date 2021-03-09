using CertificateManager;
using CertificateManager.Models;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;

namespace GeradorCertificado
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            GeraCertificado();
        }

        static void GeraCertificado()
        {
            if (!Directory.Exists(@"c:\temp\certificado\"))
            {
                Directory.CreateDirectory(@"c:\temp\certificado\");
            }

            var serviceProvider = new ServiceCollection()
                    .AddCertificateManager()
                    .BuildServiceProvider();

            var createClientServerAuthCerts =
                serviceProvider.GetService<CreateCertificatesClientServerAuth>();

            var root = createClientServerAuthCerts.NewRootCertificate(
                new DistinguishedName
                {
                    CommonName = "root_test",
                    Country = "CH"
                },
                new ValidityPeriod
                {
                    ValidFrom = DateTime.UtcNow,
                    ValidTo = DateTime.UtcNow.AddYears(10)
                },
                3, "localhost");

            root.FriendlyName = "root_test certificate";

            string password = "19372846";
            var importExportCertificate = serviceProvider.GetService<ImportExportCertificate>();

            var rootCertInPfxBtyes = importExportCertificate.ExportRootPfx(password, root);
            File.WriteAllBytes(@"c:\temp\certificado\root_test.pfx", rootCertInPfxBtyes);

            var intermediate = createClientServerAuthCerts
                    .NewIntermediateChainedCertificate(

                    new DistinguishedName
                    {
                        CommonName = "intermediate_test",
                        Country = "CH"
                    },

                    new ValidityPeriod
                    {
                        ValidFrom = DateTime.UtcNow,
                        ValidTo = DateTime.UtcNow.AddYears(10)
                    },
                    2, "localhost", root);

            intermediate.FriendlyName = "intermediate_test certificate";

            importExportCertificate = serviceProvider.GetService<ImportExportCertificate>();

            var intermediateCertInPfxBtyes = importExportCertificate.ExportChainedCertificatePfx(password, intermediate, root);
            File.WriteAllBytes(@"c:\temp\certificado\intermediate_test.pfx", intermediateCertInPfxBtyes);

            var client = createClientServerAuthCerts.NewClientChainedCertificate(
                new DistinguishedName { CommonName = "client_test", Country = "CH" },
                new ValidityPeriod { ValidFrom = DateTime.UtcNow, ValidTo = DateTime.UtcNow.AddYears(10) },
                "localhost", intermediate);
            client.FriendlyName = "client certificate";

            importExportCertificate = serviceProvider.GetService<ImportExportCertificate>();

            var clientCertInPfxBtyes = importExportCertificate.ExportChainedCertificatePfx(password, client, intermediate);
            File.WriteAllBytes(@"c:\temp\certificado\client_test.pfx", clientCertInPfxBtyes);

        }
    }
}
