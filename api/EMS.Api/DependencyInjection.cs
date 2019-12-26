using EMS.Shared.Model;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using EMS.Shared.Constants;

namespace EMS.Api
{
    public static class DependencyInjection
    {
        public static void AddTokenBasedAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            var tokenOption = new TokenConfigOption();
            configuration.GetSection("TokenOption").Bind(tokenOption);

            System.Console.WriteLine(tokenOption);


            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = tokenOption.Issuer,
                    ValidAudience = tokenOption.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenOption.SecretKey))
                };
            });
        }
    
        public static void AddAutoMapper(this IServiceCollection services)
        {
            
        }
        
        public static void ConfigureOptionPattern(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<TokenConfigOption>
                (options => configuration.GetSection("TokenOption").Bind(options));
        }

        public static void AddClaimBasedAuthorization(this IServiceCollection services)
        {
            services.AddAuthorization(option =>
            {
                option.AddPolicy(Policies.CanDeleteUserRoles, p => p.RequireClaim(AppClaims.CanDeleteUserRoles, "true"));
                option.AddPolicy(Policies.CanViewUsers, p => p.RequireRole(Role.Admin));
            });
        }

    }
}
