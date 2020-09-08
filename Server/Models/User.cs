using Microsoft.EntityFrameworkCore.SqlServer.Query.ExpressionTranslators.Internal;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace DistSysACW.Models
{
    public class User
    {
        #region Task2
        // TODO: Create a User Class for use with Entity Framework
        // Note that you can use the [key] attribute to set your ApiKey Guid as the primary key 
        #endregion
        public User() { 
        }
        [Key] 
        public string ApiKey { get; set; }
        public string Username { get; set; }
        public string Role { get; set; }
       
    }

    #region Task13?
    // TODO: You may find it useful to add code here for Logging
    #endregion

    public static class UserDatabaseAccess
    {
        #region Task3 
        // TODO: Make methods which allow us to read from/write to the database 
        #endregion

        public static string CreateNewUser(UserContext context, string username)
        {
            String ApiKeyTemp = Guid.NewGuid().ToString();
            String temprole;
            if (context.Users.Count() <= 0)
            {
                temprole = "Admin";
            }
            else
            {
                temprole = "User";
            }
            
            
            User usr = new User() { Username = username, ApiKey = ApiKeyTemp,Role = temprole };
            context.Users.Add(usr);
            context.SaveChanges();
            return ApiKeyTemp;

        }

        public static void RemoveUser(UserContext context,User user1)
        {
       
           context.Users.Remove(user1);
           context.SaveChanges();
        

        }
        public static void ChangeRole(UserContext ctx, string username, string role)
        {
            User User = ctx.Users.FirstOrDefault(s => s.Username == username);
            ctx.Users.Remove(User);
            ctx.SaveChanges();
            User.Role = role;
            ctx.Users.Add(User);
            ctx.SaveChanges();
            


        }


        public static bool CheckUser(UserContext ctx ,string Username)
        {
         
                
            
                if (ctx.Users.FirstOrDefault(s => s.Username == Username) != null)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            

        }



    }


}