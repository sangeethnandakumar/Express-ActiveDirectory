# Express-ActiveDirectory
Implementation of Active Directory over .NET Core

ExpressActiveDirectory is a template project that demonstrates an easy to use wrapper implementation over System.DirectoryServices. It allows you to query the AD server with limited coding.

> Inorder to host a project that uses AD authentication, You need to host it using WindowsAuthentication=true if NTLM Kerberos is active on the server, AnonymousAuthentication=false on IIS

### Repository Contents
This repo maintains 2 projects. A demo project that implements the library and the library itself


### Add an ActiveDirectory section on your appsettings.json
I reccomend you to use the same format on appsettings
> NOTE: The values shown here are placeholders

```json
{
  "ActiveDirectory": {
    "LDAPServer": "ad.XXXXXXXX.com",
    "DCServer": "10.20.30.40",

    "BindDN": "CN=XXX,OU=XXX,OU=XXX,DC=XXX,DC=XXX,DC=XXX",
    "BindCredentials": "XXXXXXXX",

    "SearchBase": "DC=ad,DC=XXXX, DC=com",
    "SearchUserTemplate": "(&(objectCategory=person)(objectClass=user)(sAMAccountName=$USERNAME))",
    "SearchGroupsTemplate": "(&(objectCategory=person)(objectClass=user)(objectCategory=user)(|$GROUPS))",

    "UserGroupName": "CN=XXXgroupXXX,OU=XXX,OU=XXX,OU=XXX,OU=XXX,DC=XX,DC=XXX,DC=XXX",
    "AdminGroupName": "CN=XXXgroupXXX,OU=XXX,OU=XXX,OU=XXX,OU=XXX,DC=XX,DC=XXX,DC=XXX",

    "AccountGroups": [
      "CN=XXXgroupXXX,OU=XXX,OU=XXX,OU=XXX,OU=XXX,DC=XX,DC=XXX,DC=XXX",
      "CN=XXXgroupXXX,OU=XXX,OU=XXX,OU=XXX,OU=XXX,DC=XX,DC=XXX,DC=XXX"
    ]
  }
}
```

### Properties Required
Find the details of AD configuration required to connect.
| Property | Explanation
| ------ | ------
| LDAPServer | This is the server that configured to use as an ActiveDirectory server
| DCServer | IP address of Domain Controller server
| BindDN | Username of the agent quering ActiveDirectory
| BindCredentials | Password of the agent quering ActiveDirectory
| SearchBase | This is the starting point of searching inside ActiveDirectory
| SearchUserTemplate | Template for searching a user object in AD. $USERNAME will be replaced on runtime
| SearchGroupsTemplate | Template for searching groups in AD. $$GROUPS will be replaced on runtime
| UserGroupName | Group name of user who is considered USER
| AdminGroupName | Group name of user who is considered ADMIN
| AdminGroupName | This is an array of groups to be considered. This can be ignored if not required by making some code changes also

## Query Result
All AD search results are converted to a UserPrincipal type (custom one). These are the properties that it holds
```csharp
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
```

## ActiveDirectory Operations
The current implementation of this library supports the following on AD

| API | Parameters | Explanation
| ------ | ------ | ------
| LDAPModule.CurrentUser | HttpContext | This API gives the UserPrincipal of currently logged in user. This is obtained from httpcontext. Inject or use HttpContextAccessor
| LDAPModule.GetUser | Username | This API gives the UserPrincipal of a username provided
| LDAPModule.GetUsersUnderADGroups | string[] groups | This API gives a collection of users who are under the provided groups
| LDAPModule.CurrentRole | HttpContext, Username (optional) | This API gives an enumeration of weather the user has USER or ADMIN privileges. It takes the logged in username if username is not provided. Decision of weather a username to be considered as a USER or ADMIN is decided from appsettings.json configuration discussed above
