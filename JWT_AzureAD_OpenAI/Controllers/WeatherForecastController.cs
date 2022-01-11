using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace JWT_AzureAD_OpenAI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        public IConfiguration Configuration { get; }

       
        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, IConfiguration configuration)
        {
            _logger = logger;
            Configuration = configuration;

        }
        public async Task<JwtSecurityToken> ValidateAsync(string token)
        {

            if (!String.IsNullOrEmpty(token)) token = token.Substring(7);

            string myTenant = Configuration.GetSection("AzureAd").GetSection("TenantId").Value;
            var myAudience = Configuration.GetSection("AzureAd").GetSection("Audience").Value;
            var myIssuer = String.Format(CultureInfo.InvariantCulture, "https://login.microsoftonline.com/{0}/v2.0", myTenant);

            var mySecret = "t.GDqjoBYBhB.tRC@lbq1GdslFjk8=57";
            var mySecurityKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(mySecret));
            var stsDiscoveryEndpoint = String.Format(CultureInfo.InvariantCulture, "https://login.microsoftonline.com/{0}/.well-known/openid-configuration", myTenant);
            var configManager = new ConfigurationManager<OpenIdConnectConfiguration>(stsDiscoveryEndpoint, new OpenIdConnectConfigurationRetriever());
            try
            {
                var config = await configManager.GetConfigurationAsync();
                var tokenHandler = new JwtSecurityTokenHandler();

                var validationParameters = new TokenValidationParameters
                {
                    ValidAudience = myAudience,
                    ValidIssuer = myIssuer,
                    IssuerSigningKeys = config.SigningKeys,
                    ValidateLifetime = false,
                    IssuerSigningKey = mySecurityKey
                };

                var validatedToken = (SecurityToken)new JwtSecurityToken();

                // Throws an Exception as the token is invalid (expired, invalid-formatted, etc.)  
                tokenHandler.ValidateToken(token, validationParameters, out validatedToken);

                return validatedToken as JwtSecurityToken;
            }
            catch(Exception e)
            {
                throw e;
            }
            
        }

        [HttpGet]
        public async Task<System.IdentityModel.Tokens.Jwt.JwtPayload> Get()
        {

            string token = Request.Headers["Authorization"];
            if ((object)token == null) return null;
            try
            {
                var rng = new Random();

                var tokenResult = await ValidateAsync(token);
                if (tokenResult != null)
                {
                    return tokenResult.Payload;
                }
                return null;

            }
            catch(Exception e)
            {
                throw e.InnerException;
            }

          




        }
    }
}
