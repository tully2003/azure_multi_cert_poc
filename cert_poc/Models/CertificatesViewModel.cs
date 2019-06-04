using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace cert_poc.Models
{
    public class CertificatesViewModel
    {
        public string StringToEncrypt { get; set; }

        public IEnumerable<CertificateViewModel> Certificates { get; set; }

        public class CertificateViewModel
        {
            public string Name { get; set; }

            public string EncryptedString { get; set; }

            public string DecryptedString { get; set; }

            public DateTimeOffset CreatedDate { get; set; }
        }
    }
}

