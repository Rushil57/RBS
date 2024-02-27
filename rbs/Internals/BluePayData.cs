using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
namespace plweb.Internals
{
    public class BluePayData
    {
        public string AccountID { get; set; }
        public string SecretKey { get; set; }
        public string Mode { get; set; }
        public string PaymentsRedirectURL { get; set; }
    }
}
