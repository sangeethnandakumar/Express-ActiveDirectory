using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;

namespace ExpressAD
{
    public static class LDAPModule
    {

        //Read Active Directory Configuration from appsettings
        public static DirectoryCofig ReadDirectoryConfig(string username = "")
        {
            /* NOTE: Change this code to retrive config data from appsettings.json */
            var LDAPServer = "ActiveDirectory:LDAPServer";
            var SearchBase = "ActiveDirectory:SearchBase";
            var BindDN = "ActiveDirectory:BindDN";
            var BindCredentials = "ActiveDirectory:BindCredentials";
            var SearchUserTemplate = "ActiveDirectory:SearchUserTemplate".Replace("$USERNAME", username);
            var SearchGroupsTemplate = "ActiveDirectory:SearchGroupsTemplate";
            var AdminGroupName = "ActiveDirectory:AdminGroupName";
            var UserGroupName = "ActiveDirectory:UserGroupName";

            //Build a DirectoryConfig from the appsettings values
            return new DirectoryCofig
            {
                LDAPServer = LDAPServer,
                SearchBase = SearchBase,
                BindDN = BindDN,
                BindCredentials = BindCredentials,
                SearchUserTemplate = SearchUserTemplate,
                SearchGroupsTemplate = SearchGroupsTemplate,
                AdminGroup = AdminGroupName,
                UserGroup = UserGroupName
            };
        }

        //This function will return UserPricipal of current user. Return NULL means can't find user from ActiveDirectory.
        //Username of current user is obtained from HttpContext
        public static UserPrincipal CurrentUser(HttpContext context)
        {
            var LoginUsername = context.User.Identity.Name.Split("\\")[1];
            var directoryConfig = ReadDirectoryConfig(LoginUsername);
            var connectionString = $"LDAP://{directoryConfig.LDAPServer}/{directoryConfig.SearchBase}";
            //Bind to AD & Query
            using (DirectoryEntry entry = new DirectoryEntry(connectionString, directoryConfig.BindDN, directoryConfig.BindCredentials, AuthenticationTypes.None))
            {
                DirectorySearcher ds = new DirectorySearcher(entry);
                ds.SearchScope = SearchScope.Subtree;
                ds.Filter = directoryConfig.SearchUserTemplate;
                SearchResult searchResult = ds.FindOne();
                if (searchResult != null)
                {
                    return ConvertToUserPrincipal(searchResult);
                }
                else
                {
                    return null;
                }
            }
        }

        //This function will return a UserPricipal from a username. Return NULL means can't find user from ActiveDirectory
        public static UserPrincipal GetUser(string username)
        {
            var directoryConfig = ReadDirectoryConfig(username);
            var connectionString = $"LDAP://{directoryConfig.LDAPServer}/{directoryConfig.SearchBase}";
            //Bind to AD & Query
            using (DirectoryEntry entry = new DirectoryEntry(connectionString, directoryConfig.BindDN, directoryConfig.BindCredentials, AuthenticationTypes.None))
            {
                DirectorySearcher ds = new DirectorySearcher(entry);
                ds.SearchScope = SearchScope.Subtree;
                ds.Filter = directoryConfig.SearchUserTemplate;
                SearchResult searchResult = ds.FindOne();
                if (searchResult != null)
                {
                    return ConvertToUserPrincipal(searchResult);
                }
                else
                {
                    return null;
                }
            }
        }

        //This function will return a collection of UserPrincipal under enlisted groups.
        //'SearchGroupsTemplate' from appsettings.json is used to query AD
        public static IEnumerable<UserPrincipal> GetUsersUnderADGroups(string[] groupNames)
        {
            var directoryConfig = ReadDirectoryConfig();
            var searchGroups = "";
            foreach (var group in groupNames)
            {
                searchGroups += $"(memberOf={group})";
            }
            directoryConfig.SearchGroupsTemplate = directoryConfig.SearchGroupsTemplate.Replace("$GROUPS", searchGroups);
            //Generate connectionstring
            var connectionString = $"LDAP://{directoryConfig.LDAPServer}/{directoryConfig.SearchBase}";
            //Bind to AD & Query
            using (DirectoryEntry entry = new DirectoryEntry(connectionString, directoryConfig.BindDN, directoryConfig.BindCredentials, AuthenticationTypes.None))
            {
                DirectorySearcher ds = new DirectorySearcher(entry);
                ds.SearchScope = SearchScope.Subtree;
                ds.Filter = directoryConfig.SearchGroupsTemplate;
                SearchResultCollection searchResults = ds.FindAll();
                if (searchResults != null)
                {
                    var userPrincipal = new List<UserPrincipal>();
                    foreach (SearchResult searchResult in searchResults)
                    {
                        userPrincipal.Add(ConvertToUserPrincipal(searchResult));
                    }
                    return userPrincipal;
                }
                else
                {
                    return null;
                }
            }
        }

        //This fucntion will tell the current role (logged in user/provided username) from UserRoles
        public static UserRoles CurrentRole(HttpContext context, string username = null)
        {
            string ADMIN_TEMPLATE = ReadDirectoryConfig().AdminGroup;
            string USER_TEMPLATE = ReadDirectoryConfig().UserGroup;
            var user = new UserPrincipal();
            if (username == null)
            {
                user = CurrentUser(context);
            }
            else
            {
                user = GetUser(username);
            }
            if (user.Groups.Any(ADMIN_TEMPLATE.Contains))
            {
                return UserRoles.ADMIN;
            }
            else if (user.Groups.Any(USER_TEMPLATE.Contains))
            {
                return UserRoles.USER;
            }
            else
            {
                return UserRoles.UNKNOWN;
            }
        }




        #region HELPER FUNCTIONS
        //Validate function get DirectoryProps => Fills any null data to some defaults.
        //Configure or remove this function according to your application needs
        private static DirectoryProps Validate(string props)
        {
            var directoryProps = JsonConvert.DeserializeObject<DirectoryProps>(props);
            if (directoryProps.description == null)
            {
                directoryProps.description = new List<string>();
                directoryProps.description.Add("No info available");
            }
            return directoryProps;
        }

        //This helper fuction converts the SearchResult obtained from a query search into UserPrincipal that has well known standard properties
        private static UserPrincipal ConvertToUserPrincipal(SearchResult searchResult)
        {
            var props = JsonConvert.SerializeObject(searchResult.Properties);
            var directoryProps = Validate(props);
            var userPrincipal = new UserPrincipal
            {
                EmailAddress = directoryProps.userprincipalname.FirstOrDefault(),
                SamAccountName = directoryProps.samaccountname.FirstOrDefault(),
                Description = directoryProps.description.FirstOrDefault(),
                DisplayName = directoryProps.displayname.FirstOrDefault(),
                DistinguishedName = directoryProps.distinguishedname.FirstOrDefault(),
                Guid = searchResult.GetDirectoryEntry().Guid,
                Name = directoryProps.name.FirstOrDefault(),
                UserPrincipalName = directoryProps.userprincipalname.FirstOrDefault(),
                Sid = directoryProps.objectsid[0],
                GivenNames = directoryProps.givenname.ToArray(),
                Groups = directoryProps.memberof.ToArray(),
            };
            return userPrincipal;
        }
        #endregion

    }
}
