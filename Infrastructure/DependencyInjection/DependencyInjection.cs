using System.Text;
using System.Text.Json;
using BackendJX3D.Application.Interfaces.IMapper;
using BackendJX3D.Application.Interfaces.IServices;
using BackendJX3D.Application.Mapper;
using BackendJX3D.Infrastructure.Resources;
using BackendJX3D.Application.Services;
using BackendJX3D.Infrastructure.Auth;
using BackendJX3D.Infrastructure.Repositories.IRepository;
using BackendJX3D.Infrastructure.Repositories.Repository;
using BackendJX3D.Infrastructure.Session;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Network.Header;

namespace BackendJX3D.Infrastructure.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        AddServices(services);
        AddJWT(services, configuration);
        AddAuthenticationAndAuthorization(services, configuration);
        AddCorsConfig(services);
        AddMapper(services);
        AddLoadingResourcesService();
        // KProtocol.InitProtocol();
        return services;
    }
    
    private static void AddServices(this IServiceCollection services)
    {
        services.AddScoped<IAccountService, AccountService>();
        services.AddScoped<IServerListService, ServerListService>();
        services.AddScoped<IWorldService, WorldService>();
        services.AddScoped<IPlayerService, PlayerService>();
        services.AddScoped<INpcService, NpcService>();
        services.AddScoped<IITemService, ItemService>();
        services.AddScoped<ISkillService, SkillService>();
        services.AddScoped<IChatService, ChatService>();
        services.AddScoped<ITaskService, TaskService>();
        services.AddSingleton<ISessionManager, SessionManager>();
    }
    
    private static void AddJWT(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtOptions>(
            configuration.GetSection(JwtOptions.SectionName));

        services.AddScoped<IJwtService, JwtService>();
        
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, CurrentUser>();
    }
    
    private static void AddAuthenticationAndAuthorization(this IServiceCollection services, IConfiguration configuration)
    {
        var jwt = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()!;

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,

                    ValidIssuer = jwt.Issuer,
                    ValidAudience = jwt.Audience,

                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Secret))
                };
                
                options.Events = new JwtBearerEvents
                {
                    OnChallenge = async context =>
                    {
                        context.HandleResponse();

                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        context.Response.ContentType = "application/json";

                        await context.Response.WriteAsync(JsonSerializer.Serialize(new
                        {
                            success = false,
                            statusCode = StatusCodes.Status401Unauthorized,
                            error = new
                            {
                                errorCode = "unauthorized",
                                errorMessage = "Bạn chưa đăng nhập."
                            }
                        }));
                    },

                    OnForbidden = async context =>
                    {
                        context.Response.StatusCode = StatusCodes.Status403Forbidden;
                        context.Response.ContentType = "application/json";

                        await context.Response.WriteAsync(JsonSerializer.Serialize(new
                        {
                            success = false,
                            statusCode = StatusCodes.Status403Forbidden,
                            error = new
                            {
                                errorCode = "forbidden",
                                errorMessage = "Bạn không có quyền truy cập."
                            }
                        }));
                    }
                };
            });

        services.AddAuthorization();
    }

    private static void AddCorsConfig(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                policy
                    .AllowAnyOrigin()
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            });
        });
    }

    private static void AddMapper(this IServiceCollection services)
    {
        services.AddScoped<IItemMapper, ItemMapper>();
        services.AddScoped<ISkillMapper, SkillMapper>();
        services.AddScoped<INpcMapper, NpcMapper>();
        services.AddScoped<ITaskMapper, TaskMapper>();
        services.AddScoped<IChatMapper, ChatMapper>();  
    }
    
    private static void AddLoadingResourcesService()
    {
        LoadResource.InitResources();
    }
}