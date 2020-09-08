using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using CoreExtensions;
using DistSysACW.Models;

using Microsoft.AspNetCore.Mvc;

namespace DistSysACW.Controllers
{
    [Route("api/[controller]/[Action]")]
    [ApiController]
    public class BaseClass
    {
        private static RSACryptoServiceProvider RsaBase;
        public BaseClass()
        {
            RsaBase = new RSACryptoServiceProvider();
            if (GetKeyFromContainer("Key") != null)
            {
                RsaBase.FromXmlStringCore22(GetKeyFromContainer("Key"));
            }
            else
            {
                GenKey_SaveInContainer("Key");
            }
        
       
        }
        public RSACryptoServiceProvider GetRSA
        {
            get
            {
                RsaBase.FromXmlStringCore22(GetKeyFromContainer("Key"));
                return RsaBase;
            }
        }
        public string GetPublicKey {
            get {
                return RsaBase.ToXmlStringCore22(false);
                    }
        }
        public static void GenKey_SaveInContainer(string ContainerName)
        {
            // Create the CspParameters object and set the key container
            // name used to store the RSA key pair.  
            CspParameters cp = new CspParameters();
            cp.KeyContainerName = ContainerName;

            // Create a new instance of RSACryptoServiceProvider that accesses  
            // the key container MyKeyContainerName.  
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(cp);

            // Display the key information to the console.  
            Console.WriteLine("Key added to container: \n  {0}", rsa.ToXmlStringCore22(true));
        }
        public string  GetKeyFromContainer(string ContainerName)
        {
            // Create the CspParameters object and set the key container
            // name used to store the RSA key pair.  
            CspParameters cp = new CspParameters();
            cp.KeyContainerName = ContainerName;

            // Create a new instance of RSACryptoServiceProvider that accesses  
            // the key container MyKeyContainerName.  
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(cp);

            // Display the key information to the console.  
            return rsa.ToXmlStringCore22(true);
        }
        public string GetPrivateKey
        {
            get
            {
                return RsaBase.ToXmlStringCore22(true);
            }
        }
  

    };


    
  


    public class ProtectedController : BaseController
    {
      
     
        public ProtectedController(UserContext context) : base(context) { }
        [ActionName("Hello")]
        public string Hello([FromHeader] string ApiKey)
        {
            using (var ctx = new UserContext())
            {
                User user1 = ctx.Users.FirstOrDefault(s => s.ApiKey == ApiKey);
                this.Response.StatusCode = 200;
                return ("Hello " + user1.Username);
            }
    
        }

        [ActionName("sha1")]
        public string sha1([FromQuery]string Message)
        {
            if (Message != null)
            {
                using (SHA1Managed sha1 = new SHA1Managed())
                {
                    var hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(Message));
                    var sb = new StringBuilder(hash.Length * 2);

                    foreach (byte b in hash)
                    {
                        sb.Append(b.ToString("X2"));
                    }
                    this.Response.StatusCode = 200;
                    return sb.ToString();
                }

            }
            else
            {
                this.Response.StatusCode = 400;
                return ("Bad Request");
            }
        }

        [ActionName("sha256")]
        public string sha256([FromQuery]string Message)
        {
            if (Message != null)
            {
                using (SHA256 sha256 = new SHA256Managed())
                {
                    var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(Message));
                    var sb = new StringBuilder(hash.Length * 2);

                    foreach (byte b in hash)
                    {
                        sb.Append(b.ToString("X2"));
                    }
                    this.Response.StatusCode = 200;
                    return sb.ToString();
                }
            }
            else
            {
                this.Response.StatusCode = 400;
                return ("Bad Request");
            }

        }
        [ActionName("getpublickey")]
        public string Pubkey([FromHeader]string ApiKey)
        {
            using (var ctx = new UserContext())
            {
                User user1 = ctx.Users.FirstOrDefault(s => s.ApiKey == ApiKey);

                if (user1 != null)
                {
                    var rsa = new BaseClass();
                    

                        string _publickey = rsa.GetPublicKey;

                    //var sw = new StringWriter();
                    // var xs = new XmlSerializer(typeof(RSAParameters));
                    //  xs.Serialize(sw, _publickey);
                    Console.WriteLine(_publickey);
                    return _publickey;

                  
                    }
                else
                {
                    return "Couldn’t Get the Public Key";
                }
            }
        }
        [ActionName("sign")]
        public string Sign([FromHeader]string ApiKey,[FromQuery] string message)
        {
            using (var ctx = new UserContext())
            {
                User user1 = ctx.Users.FirstOrDefault(s => s.ApiKey == ApiKey);

                if (user1 != null)
                {


                    var rsa = new BaseClass();

                    
                    var converter = new ASCIIEncoding();
                    byte[] plainText = converter.GetBytes(message);

                    var rsaWrite = rsa.GetRSA;
                    var privateParams = rsaWrite.ExportParameters(true); ;
                    

                    byte[] signature =  rsaWrite.SignData(plainText, new SHA1CryptoServiceProvider());
                    
                    return BitConverter.ToString(signature);

                }
                else
                {
                    return "Couldn’t Get the Public Key";
                }
            }
        }
        static string ByteArrayToHexString(byte[] byteArray)
        { string hexString = ""; 
            if (null != byteArray) 
            { 
                foreach (byte b in byteArray)
                { 
                    hexString += b.ToString("x2"); 
                } 
            } return hexString; }
        static public byte[] RSAEncrypt(byte[] DataToEncrypt, RSAParameters RSAKeyInfo)
        { 
            try
            {
                byte[] encryptedData; using (RSACryptoServiceProvider RSA = new RSACryptoServiceProvider()) 
                { 
                    RSA.ImportParameters(RSAKeyInfo); encryptedData = RSA.Encrypt(DataToEncrypt, false);
                }
                return encryptedData;
            } 
            catch (CryptographicException e) {
                Console.WriteLine(e.Message); return null;
            }
        }
    }
}