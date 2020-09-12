using System;
using System.Collections.Generic;
using System.Text;

namespace ExpressAD
{
    public class UserPrincipal
    {
        public string Description { get; set; }
        public string DisplayName { get; set; }
        public string DistinguishedName { get; set; }
        public string EmailAddress { get; set; }
        public string[] GivenNames { get; set; }
        public string UserPrincipalName { get; set; }
        public Guid Guid { get; set; }
        public string Name { get; set; }
        public string SamAccountName { get; set; }
        public string Sid { get; set; }
        public string[] Groups { get; set; }
    }
}
