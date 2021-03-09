using Microsoft.AspNetCore.Authentication.Certificate;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;

namespace API
{
    internal class ValidadorCertificado
    {
        internal bool Validar(X509Certificate2 clientCertificate)
        {
            File.WriteAllText(@"c:\temp\certificado\teste.txt", clientCertificate.Thumbprint);
            //var certificadoParaComparar = new X509Certificate2(@"C:\temp\certificado\client_test.pfx", "19372846");
           var certificadoParaComparar = CarregaCertificados(clientCertificate);

            if (certificadoParaComparar.Thumbprint == clientCertificate.Thumbprint)
            {
                return true;

            }
            return false;
        }

        internal void GravaErro(CertificateAuthenticationFailedContext context)
        {
            File.WriteAllText(@"c:\temp\certificado\teste_erro.txt", JsonConvert.SerializeObject(context));
            Console.WriteLine(context);
        }

        internal X509Certificate2 CarregaCertificados(X509Certificate2 clientCertificate)
        {
            var store = new X509Store(StoreName.My, StoreLocation.LocalMachine);
            store.Open(OpenFlags.OpenExistingOnly | OpenFlags.ReadWrite);

            var certificates = new X509Certificate2Collection(store.Certificates);

           return  certificates.Find(X509FindType.FindByIssuerName, clientCertificate.IssuerName, true)[0];
        }
    }
}