using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdminUAT.Dependencias.EmailModel
{
    public class EmailSenderOptions
    {
        public int Port { get; set; }
        public string Email { get; set; }
        public string Email2 { get; set; }
        public string Password { get; set; }
        public bool EnableSsl { get; set; }
        public string Host { get; set; }

        public int PortFiscalia { get; set; }
        public string EmailFiscalia { get; set; }
        public string PasswordFiscalia { get; set; }
        public bool EnableSslFiscalia { get; set; }
        public string HostFiscalia { get; set; }
    }
}
