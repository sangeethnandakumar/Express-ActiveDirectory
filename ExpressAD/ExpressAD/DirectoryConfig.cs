using System;
using System.Collections.Generic;
using System.Text;

namespace ExpressAD
{
    public class DirectoryCofig
    {
        //ActiveDirectory Server
        public string LDAPServer { get; set; }        

        //Username and Password to login to AD
        public string BindDN { get; set; }
        public string BindCredentials { get; set; }

        //Query string template to search on AD
        public string SearchBase { get; set; }
        public string SearchGroupsTemplate { get; internal set; }
        public string SearchUserTemplate { get; internal set; }

        //Group to be considered as user and admin
        public string UserGroup { get; internal set; }
        public string AdminGroup { get; internal set; }
    }
}
