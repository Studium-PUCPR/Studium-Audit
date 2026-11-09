using Microsoft.AspNetCore.Authentication.JwtBearer;

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
                if (!string.IsNullOrEmpty(authority))
                    options.Authority = authority;
                if (!string.IsNullOrEmpty(clientId))
                    options.Audience = clientId;

                options.RequireHttpsMetadata = false;
            });

        builder.Services.AddAuthorization();
    }
}
