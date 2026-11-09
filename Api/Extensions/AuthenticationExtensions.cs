using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace Studium_Audit.Api.Extensions;

public static class AuthenticationExtensions
{
    public static void AddStudiumAuthentication(this WebApplicationBuilder builder)
    {
        var authority = builder.Configuration["AWS:Cognito:Authority"];
        var clientId = builder.Configuration["AWS:Cognito:ClientId"];

        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.MapInboundClaims = false;
                if (!string.IsNullOrEmpty(authority))
                    options.Authority = authority;
                if (!string.IsNullOrEmpty(clientId))
                    options.Audience = clientId;

                options.RequireHttpsMetadata = false;

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    NameClaimType = "sub",
                    RoleClaimType = "cognito:groups"
                };
            });

        builder.Services.AddAuthorizationBuilder()
            .AddPolicy("AdminOnly", policy =>
                policy.RequireRole("Administrator", "admin", "Admin"));
    }
}
