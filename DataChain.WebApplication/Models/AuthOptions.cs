using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Configuration;

namespace DataChain.WebApplication.Models
{
    public static class AuthOptions
    {
 
        public const int LIFETIME = 120; 
        public static SymmetricSecurityKey GetSymmetricSecurityKey()
        {
            return new SymmetricSecurityKey(Encoding.ASCII.GetBytes(ConfigurationManager.AppSettings["key"]));
        }
    }
}