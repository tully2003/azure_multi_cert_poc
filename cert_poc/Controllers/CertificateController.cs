using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using cert_poc.Models;
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;
using Dapper;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography;
using System.Text;

namespace cert_poc.Controllers
{
    public class CertificateController : Controller
    {
        private static Random random = new Random();
        private readonly IConfiguration configuration;

        public CertificateController(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public IActionResult Index()
        {
            string connectionString = configuration.GetConnectionString("Default");

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                var certificates = connection.Query<Certificate>(@"
SELECT 
    Name,
    CertificateData,
    CreatedDate
FROM Certificates");

                string stringToEncrypt = RandomString(32);
                var vm = new CertificatesViewModel
                {
                    StringToEncrypt = stringToEncrypt,
                    Certificates = certificates.Select(c => 
                    {
                        var cert = new X509Certificate2(c.CertificateData, (string)null, X509KeyStorageFlags.Exportable | X509KeyStorageFlags.MachineKeySet);

                        return new CertificatesViewModel.CertificateViewModel
                        {
                            Name = c.Name,
                            EncryptedString = Encrypt(cert, stringToEncrypt),
                            CreatedDate = c.CreatedDate
                        };
                    })
                };

                return View(vm);
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        private static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private static string Encrypt(X509Certificate2 certificate, string stringToEncrypt)
        {
            using (RSA rsa = certificate.GetRSAPublicKey())
            {
                byte[] encryptedBytes = rsa.Encrypt(Encoding.UTF8.GetBytes(stringToEncrypt), RSAEncryptionPadding.OaepSHA256);
                return Encoding.UTF8.GetString(encryptedBytes);
            }
        }

        private static string Decrypt(X509Certificate2 certificate, string stringToDecrypt)
        {
            using (RSA rsa = certificate.GetRSAPrivateKey())
            {
                byte[] decryptedBytes = rsa.Decrypt(Encoding.UTF8.GetBytes(stringToDecrypt), RSAEncryptionPadding.OaepSHA256);
                return Encoding.UTF8.GetString(decryptedBytes);
            }
        }

        private class Certificate
        {
            public string Name { get; private set; }

            public byte[] CertificateData { get; private set; }

            public DateTimeOffset CreatedDate { get; set; }
        }
    }
}
