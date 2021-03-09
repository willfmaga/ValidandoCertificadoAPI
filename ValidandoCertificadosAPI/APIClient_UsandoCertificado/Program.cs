using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;

namespace APIClient_UsandoCertificado
{
    class Program
    {
        static void Main(string[] args)
        {
            var cert = new X509Certificate2(@"C:\temp\certificado\client_test.pfx", "19372846");
            var handler = new HttpClientHandler();
            handler.ClientCertificates.Add(cert);
            var client = new System.Net.Http.HttpClient(handler);

            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri("https://localhost/API/weatherforecast"),
                Method = HttpMethod.Get,
            };
            var response = client.SendAsync(request);

            if (response.Result.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var responseContent = response.Result.Content.ReadAsStringAsync();
                var data = JsonConvert.SerializeObject(responseContent);
                return;
            }


            throw new ApplicationException($"Status code: {response.Result.StatusCode}, Error: {response.Result.ReasonPhrase}");
        }
    }
}
