using DistSysACW.Models;
using Microsoft.AspNetCore.Http;
using Org.BouncyCastle.Asn1.Ocsp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Ubiety.Dns.Core;

namespace DistSysACW.Middleware
{
    public class AuthMiddleware
    {
        private readonly RequestDelegate _next;

        public AuthMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, Models.UserContext dbContext)
        {
            #region Task5
            // TODO:  Find if a header ‘ApiKey’ exists, and if it does, check the database to determine if the given API Key is valid
            //        Then set the correct roles for the User, using claims
            #endregion
            string Apikey = context.Request.Headers["ApiKey"];
            if (Apikey != null)
            {
                User user1 = dbContext.Users.FirstOrDefault(s => s.ApiKey == Apikey);
                if (user1 != null)
                {
                    var claims = new List<Claim> {

                         new Claim(ClaimTypes.Name, user1.Username,ClaimValueTypes.String),
                         new Claim(ClaimTypes.Role, user1.Role,ClaimValueTypes.String)   
                    };

                    var userIdentity = new ClaimsIdentity(claims, Apikey);
                    context.User.AddIdentity(userIdentity);
                }
           
            }
 
            // Call the next delegate/middleware in the pipeline
            await _next(context);
        }

    }
}
