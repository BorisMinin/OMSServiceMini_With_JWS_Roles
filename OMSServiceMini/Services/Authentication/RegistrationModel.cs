using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OMSServiceMini.Services.Authentication
{
    public class RegistrationModel
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
    }
}