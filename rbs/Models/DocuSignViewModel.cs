using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static DocuSign.eSign.Client.Auth.OAuth;

namespace plweb.Models
{
    public class DocuSignViewModel
    {
        public string AuthToken { get; set; }

        public UserInfo UserInfo { get; set; }
    }
}
