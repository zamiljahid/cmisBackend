using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace WebAPI
{
    public class Jwt
    {
        private readonly IConfiguration _configuration;
        public Jwt()
        {
            _configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build();
        }

        public JwtToken GenerateJwtToken()
        {
            try
            {
                string secretKey = _configuration.GetValue<string>("SecretKey") ?? "";
                string issuer = _configuration.GetValue<string>("Issuer") ?? "";
                string audience = _configuration.GetValue<string>("Audience") ?? "";
                string expirationHour = _configuration.GetValue<string>("ExpirationHour") ?? "";
                int h = Convert.ToInt32(expirationHour);
                DateTime expirationTime = DateTime.Now.AddHours(h);

                // Define the security key (using HMACSHA256 algorithm)
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));

                // Define signing credentials
                var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                // Define the claims (payload data)
                var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, "subject"),  // Subject claim
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()), // JWT ID claim
            new Claim(JwtRegisteredClaimNames.Iss, issuer),  // Issuer claim
            new Claim(JwtRegisteredClaimNames.Aud, audience) // Audience claim
        };

                // Create the JWT token
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new System.Security.Claims.ClaimsIdentity(claims),
                    Expires = expirationTime,  // Set expiration time
                    Issuer = issuer,
                    Audience = audience,
                    SigningCredentials = credentials
                };

                // Create a JWT handler and generate the token
                var tokenHandler = new JwtSecurityTokenHandler();
                var token = tokenHandler.CreateToken(tokenDescriptor);

                // Return the token as a string
                var objToken = new JwtToken
                {
                    Token = tokenHandler.WriteToken(token),
                    IssueDate = DateTime.Now,
                    ExpiryDate = expirationTime
                };

                return objToken;
            }
            catch (Exception ex)
            {

                throw new Exception(ex.Message);
            }

        
        }
        public bool ValidateJwtToken(string token)
        {
            try
            {
                string secretKey = _configuration.GetValue<string>("SecretKey") ?? "";
                string issuer = _configuration.GetValue<string>("Issuer") ?? "";
                string audience = _configuration.GetValue<string>("Audience") ?? "";

                // Define the security key
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));

                // Define token validation parameters
                var tokenHandler = new JwtSecurityTokenHandler();
                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,  // Ensure the token has not expired
                    ValidIssuer = issuer,
                    ValidAudience = audience,
                    IssuerSigningKey = key
                };

                // Validate the token
                var principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);

                // Token is valid if it reaches this point
                return true;
            }
            catch (SecurityTokenExpiredException)
            {
                return false;
            }
            catch (SecurityTokenInvalidSignatureException)
            {
                return false;
            }
            catch (Exception)
            {
                return false;
            }

        }
    }
    public class JwtToken
    {
        public string Token { get; set; }
        public DateTime IssueDate { get; set; }
        public DateTime ExpiryDate { get; set; }

    }
}
