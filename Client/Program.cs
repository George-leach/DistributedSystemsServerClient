using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Numerics;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using CoreExtensions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal;

namespace DistSysACWClient
{

    public class User
    {
        public User()
        {
        }
        [Key]
        public string ApiKey { get; set; }
        public string Username { get; set; }
        public string Role { get; set; }
        public string pubkey { get; set; }
    }

        class Client
        {
            static HttpClient client = new HttpClient();
           
            static void Main()
            {
            //http://distsysacw.azurewebsites.net/5710900/
            client.BaseAddress = new Uri("https://localhost:5001/");
           
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
            bool run = true;
            User user = new User();
       
            Console.WriteLine("Hello. What would you like to do?");
            while (run == true)
            {
                string request = Console.ReadLine();
                Console.Clear();
                if (request.ToLower() != "exit")
                    {
                    Console.WriteLine("...please wait...");
                    RunAsync(request,user).GetAwaiter().GetResult();
                    
                    Console.WriteLine("What would you like to do next?");
                
                }
                else
                {
                    run = false;
                }
            }
              }
        public static byte[] StringToByteArray(String hex)
        {
            int NumberChars = hex.Length;
            byte[] bytes = new byte[NumberChars / 2];
            for (int i = 0; i < NumberChars; i += 2)
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            return bytes;
        }
        static async Task<String> CreateUser(string username)
            {
           
            string responsestring = "";
            var content = new StringContent(JsonConvert.SerializeObject(username), System.Text.Encoding.UTF8, "application/json");
            HttpResponseMessage response = await client.PostAsync(
                    "api/user/new", content);
                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine( "Got API Key");
                    responsestring = await response.Content.ReadAsStringAsync();
                    return responsestring;
                }
                else
                {
                    responsestring = await response.Content.ReadAsStringAsync();
                    Console.WriteLine(responsestring);
                    return null;
                }
        
            }
        static async Task<String> DeleteUser(string username, string Apikey)
        {
            client.DefaultRequestHeaders.Add("ApiKey", Apikey);
            string responsestring;
            HttpResponseMessage response = await client.DeleteAsync(
                "api/user/removeuser?username="+username);
            if (response.IsSuccessStatusCode)
            {
                responsestring = "True";

                return responsestring;
            }
            else
            {

                responsestring = "False";

                return responsestring;

            }

        }
        static async Task<String> ChangeRole(string username1, string Apikey, string role)
        {
            string myJson = "{username:" + username1 + ",Role:" + role + "}";
            JObject Json = new JObject();
            Json["username"] = username1;
            Json["role"] = role;
            var JsonLoad = await Task.Run(() => JsonConvert.SerializeObject(Json));
            var httpContent = new StringContent(JsonLoad, Encoding.UTF8, "application/json");
            client.DefaultRequestHeaders.Add("ApiKey", Apikey);
            HttpResponseMessage response;
            
            string responsestring;
            response = await client.PostAsync(
                "api/user/changerole", httpContent);
            if (response.IsSuccessStatusCode)
            {
                responsestring = await response.Content.ReadAsStringAsync();

                return responsestring;
            }
            else
            {
                responsestring = await response.Content.ReadAsStringAsync();

                return responsestring;

            }

        }

        static async Task<string> SignString(string path)
        {
            string responsestring = "";
            HttpResponseMessage response = await client.GetAsync(path);
            if (response.IsSuccessStatusCode)
            {
                responsestring = await response.Content.ReadAsStringAsync();
            }
            return responsestring;
        }
        static async Task<string> BaseTask(string path)
            {
                string responsestring = "";
                HttpResponseMessage response = await client.GetAsync(path);
                if (response.IsSuccessStatusCode)
                 {
                    responsestring = await response.Content.ReadAsStringAsync();
                }
                return responsestring;
            }
            static async Task RunAsync(string newstring,User user)
        { 
            client.DefaultRequestHeaders.Clear();

            try
            {

                String[] splited = newstring.Split(" ");
                string Controller = splited[0];
                string Action = splited[1];
                Task<string> task = null;
                if (Controller == "TalkBack")
                {
                    if (Action == "Hello")
                    {
                        task = BaseTask("Api/" + Controller + "/" + Action);
                        Console.WriteLine(task.Result);


                    }
                    else if (Action == "Sort")
                    {
                        string splitin = splited[2].Replace("[", string.Empty).Replace("]", string.Empty);
                        String[] splitint = splitin.Split(",");
                        string introute = null;
                        foreach (string s in splitint)
                        {
                            introute = introute + "integers=" + s + "&";
                        }
                        introute = introute.Remove(introute.Length - 1, 1);
                        task = BaseTask("Api/" + Controller + "/" + Action + "?" + introute);
                        Console.WriteLine(task.Result);
                    }

                }
                else if (Controller == "User")
                {
                    if (Action == "Get")
                    {
                        task = BaseTask("Api/" + Controller + "/new?username=" + splited[2]);
                        Console.WriteLine(task.Result);
                    }
                    else if (Action == "Post")
                    {
                        string Username = splited[2];
                        task = CreateUser(Username);
                        user.ApiKey = task.Result;
                        user.Username = Username;
                       


                    }
                    else if (Action == "Set")
                    {
                        user.ApiKey = splited[3];
                        user.Username = splited[2];
                        Console.WriteLine("Stored");

                    }
                    else if (Action == "Delete")
                    {
                        if (user.ApiKey != null && user.Username != null)
                        {
                            string tf = await DeleteUser(user.Username, user.ApiKey);
                            Console.WriteLine(tf);

                        }
                        else
                        {
                            Console.WriteLine("You need to do a User Post or User Set first");
                        }

                    }
                    else if (Action == "Role")
                    {
                        if (user.ApiKey != null)
                        {
                            task = ChangeRole(splited[2], user.ApiKey, splited[3]);
                            Console.WriteLine(task.Result);

                        }
                        else
                        {
                            Console.WriteLine("You need to do a User Post or User Set first");
                        }
                    }
                }
                else if (Controller == "Protected")
                {
                    if (user.ApiKey != null)
                    {
                        client.DefaultRequestHeaders.Add("ApiKey", user.ApiKey);
                        if (Action == "Hello")
                        {


                            task = BaseTask("Api/" + Controller + "/" + Action);
                            Console.WriteLine(task.Result);
                        }
                        else if (Action == "SHA1")
                        {

                            task = BaseTask("Api/" + Controller + "/" + Action + "?message=" + splited[2]);
                            Console.WriteLine(task.Result);
                        }
                        else if (Action == "SHA256")
                        {
                            task = BaseTask("Api/" + Controller + "/" + Action + "?message=" + splited[2]);
                            Console.WriteLine(task.Result);
                        }
                        else if (Action == "Get")
                        {
                            task = BaseTask("Api/" + Controller + "/" + Action + splited[2]);
                            if (task.Result != "Couldn’t Get the Public Key")
                            {
                                Console.WriteLine("Got Public Key");
                                user.pubkey = task.Result;
                            }
                            else
                            {
                                Console.WriteLine(task.Result);
                            }

                        }
                        else if (Action == "Sign")
                        {
                            if (user.pubkey != null)
                            {
                                task = SignString("Api/" + Controller + "/" + Action + "?message=" + splited[2]);
                           
                                
                              
                                if (task.Result != "Couldn’t Get the Public Key")
                                {
                                  
                                        string result = task.Result;
                                   
                                    result = result.Replace("-", string.Empty);
                                        RSACryptoServiceProvider clientRsaProvider = new RSACryptoServiceProvider(2048);
                                    string pub = user.pubkey;
                                        clientRsaProvider.FromXmlStringCore22(user.pubkey);
                                        byte[] signedBytes = StringToByteArray(result);
                                    var converter = new ASCIIEncoding();
                                    byte[] plainText = converter.GetBytes(splited[2]);


                                          byte[] toVerify = Encoding.UTF8.GetBytes(splited[2]);
                                         bool verified = clientRsaProvider.VerifyData(plainText, new SHA1CryptoServiceProvider(), signedBytes);
                                    if (verified == true)
                                    {
                                        Console.WriteLine("Message was succesfully signed");
                                    }
                                    else
                                    {
                                        Console.WriteLine("Message was not successfully signed");
                                    }
              
               
                                    }
                            
                             
                                
                                else
                                {
                                    Console.WriteLine("Client doesn’t yet have the public key");
                                }

                            }

                        }
                        else if (Action == "AddFifty")
                        {

                        }
                        else
                        {
                            Console.WriteLine("You need to do a User Post or User Set first");
                        }
                    }

                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.GetBaseException().Message);
            }
            }
        static public byte[] RSADecrypt(byte[] DataToDecrypt, RSAParameters RSAKeyInfo)
        {
            try
            {
                byte[] decryptedData;
                using (RSACryptoServiceProvider RSA = new RSACryptoServiceProvider())
                {
                    RSA.ImportParameters(RSAKeyInfo); decryptedData = RSA.Decrypt(DataToDecrypt, false);
                }
                return decryptedData;
            }
            catch (CryptographicException e)
            {
                Console.WriteLine(e.ToString());
                return null;
            }
        }

    }
   


}
