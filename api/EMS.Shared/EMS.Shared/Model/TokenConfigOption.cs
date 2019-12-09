using System;
using System.Collections.Generic;
using System.Text;

namespace EMS.Shared.Model
{
    public class TokenConfigOption
    {
        public string Issuer { get; set; }
        public string Audience { get; set; }
        public string SecretKey { get; set; }


        public override string ToString()
            => $"\nIssuer: {Issuer}, Audience: {Audience}, SecretKey: {SecretKey}";
    }
}
