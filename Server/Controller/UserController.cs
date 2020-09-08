using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Web.Helpers;
using DistSysACW.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DistSysACW.Controllers
{
 
    public class UserController : BaseController    
    {

        /// <summary>
        /// Constructs a TalkBack controller, taking the UserContext through dependency injection
        /// </summary>
        /// <param name="context">DbContext set as a service in Startup.cs and dependency injected</param>
        public UserController(UserContext context) : base(context) { }
   
        [HttpGet]
        public string New([FromQuery]string username)
        {
            using (var ctx = new UserContext())
            {
                return UserDatabaseAccess.CheckUser(ctx, username) == true
                ? "True - User Does Exist! Did you mean to do a POST to create a new user?"
                : "False - User Does Not Exist! Did you mean to do a POST to create a new user?";
            }

        }
        [HttpPost]
        [ActionName("new")]
        public IActionResult PostAction([FromBody] string username)
        {

            using (var ctx = new UserContext())
            {
                if (UserDatabaseAccess.CheckUser(ctx,username) == true)
                {
                    this.Response.StatusCode = 403;
                    return new ObjectResult("Oops. This username is already in use. Please try again with a new username.");
                }
                else if (username == null)
                {
                    this.Response.StatusCode = 400;
                    return new ObjectResult("Oops. Make sure your body contains a string with your username and your Content-Type is Content-Type:application/json");
                }
                else
                {
                    string ret = UserDatabaseAccess.CreateNewUser(ctx, username);
                    return Ok(ret);
                }
            }



        }
        [HttpDelete]
        [ActionName("RemoveUser")]
        [Authorize(Roles = "Admin,User")]
        public Boolean Remove([FromHeader] string ApiKey,[FromQuery] string username)
        {

            using (var ctx = new UserContext())
            {
               User  user1 = ctx.Users.FirstOrDefault(s => s.Username == username);
                if (user1.ApiKey == ApiKey)
                {

                    UserDatabaseAccess.RemoveUser(ctx,user1);
                    this.Response.StatusCode = 200;
                    return (true);
                }
                else
                {
                    this.Response.StatusCode = 200;
                    return (false);
                }
         
                  
            }
         }
        [HttpPost]
        [ActionName("ChangeRole")]
        [Authorize(Roles = "Admin")]
        public IActionResult Change([FromBody]JObject values)
        {
            string username = (string)values["username"];
            string role = (string)values["role"];
            try {
                using (var ctx = new UserContext())
                {
                    if (role.ToLower() != "user" && role.ToLower() != "admin")
                    {
                        return BadRequest("NOT DONE: Role does not exist");
                    }
                    else if (UserDatabaseAccess.CheckUser(ctx, username) == false)
                    {
                        return BadRequest("NOT DONE: Username does not exist");
                    }
                    else
                    {
                        UserDatabaseAccess.ChangeRole(ctx, username, role);
                        return Ok("DONE");
                    }
                }
            }
            catch
            {
                return BadRequest("NOT DONE: An error occured");
            }
        }



    }
}

