using BlogDotNetCode.Models.Account;
using System;
using System.Collections.Generic;
using System.Text;

namespace BlogDotNetCode.Services
{
    public interface ITokenService
    {
        public string CreateToken(ApplicationUserIdentity user);
    }
}
