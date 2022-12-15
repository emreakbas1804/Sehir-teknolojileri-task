using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace webApi.Identity
{
    public class User:IdentityUser
    {        
        public string FullName { get; set; }        
    }   
    public class UserTokens :IdentityUserToken<String>
    {  
    }    
    
}