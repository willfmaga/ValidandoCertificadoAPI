using Microsoft.AspNetCore.Authentication.Certificate;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

namespace API
{
    internal class ValidadorCertificado
    {
        internal bool Validar(X509Certificate2 clientCertificate)
        {
            File.WriteAllText(@"c:\temp\certificado\teste.txt", clientCertificate.Thumbprint);

            var listOfValidThumbprints = new List<string>
            {
                "3E242D3509937FE9AE2B5C29669AF802100B0057" //localhost
            };

            if (listOfValidThumbprints.Contains(clientCertificate.Thumbprint.ToLower()) ||  
                listOfValidThumbprints.Contains(clientCertificate.Thumbprint))
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

        internal X509Certificate2 CarregaCertificado(X509Certificate2 clientCertificate)
        {
            var store = new X509Store(StoreName.My, StoreLocation.LocalMachine);
            store.Open(OpenFlags.OpenExistingOnly | OpenFlags.ReadWrite);

            var certificates = new X509Certificate2Collection(store.Certificates);

            return certificates.Cast<X509Certificate2>().Where(x => x.Subject == clientCertificate.Subject).FirstOrDefault();
        }
    }
}