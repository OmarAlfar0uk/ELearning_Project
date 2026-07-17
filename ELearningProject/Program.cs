using Auth.Behaviors;
using Auth.Contarcts;
using Auth.Data.Seeding;
using Auth.Models;
using Auth.Repositories;
using Auth.Services;
using ELearningProject.Contarcts;
using ELearningProject.Contracts;
using ELearningProject.Data;
using ELearningProject.Features.Assignments;
using ELearningProject.Features.Auth;
using ELearningProject.Features.Progress;
using ELearningProject.Features.Tracks;
using ELearningProject.Features.Lectures;
using ELearningProject.Features.Materials;
using ELearningProject.Features.Exams;
using ELearningProject.Features.Notifications;
using ELearningProject.Features.Auth.UpdateUserProfile;
using ELearningProject.Features.Batche;
using ELearningProject.Features.Extensions;
using ELearningProject.Features.Submissions;
using ELearningProject.Features.Files;
using ELearningProject.Features.Dashboard;
using ELearningProject.Features.Articles;
using ELearningProject.Features.Coins;
using ELearningProject.Repositories;
using ELearningProject.Seeding;
using ELearningProject.Services;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Reflection;
using System.Text;

namespace ELearningProject
{
    public class Program
    {
        public static async Task Main(string[] args)
        {

            #region Serilog
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.Console(
                outputTemplate:
                "[{Timestamp:HH:mm:ss} {Level:u3}] [{CorrelationId}] {Message:lj}{NewLine}{Exception}"
                   )

                .WriteTo.File(
                    path: "Logs/log-.txt",
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 7,
                    shared: true)
                .CreateLogger();
            #endregion

            var builder = WebApplication.CreateBuilder(args);

            #region --- Services ---
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();

            #region Swagger Configuration
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Auth API", Version = "v1" });
                c.MapType<IFormFile>(() => new OpenApiSchema
                {
                    Type = "string",
                    Format = "binary"
                });
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                        },
                        Array.Empty<string>()
                    }
                });
            });
            #endregion

            #region Database
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
            builder.Services.AddDbContext<UniversitySystemAuthContext>(options =>
                options.UseSqlServer(connectionString));
            #endregion

            #region Identity
            builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
            {
                // Password
                options.Password.RequireDigit = false;
                options.Password.RequiredLength = 6;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;

                // User
                options.User.RequireUniqueEmail = false;

                // Lockout
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(10);
                options.Lockout.AllowedForNewUsers = true;
            })
              .AddEntityFrameworkStores<UniversitySystemAuthContext>()
              .AddDefaultTokenProviders();
            #endregion

            #region JWT Configuration
            var jwtSettings = builder.Configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"];

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = "JwtBearer";
                options.DefaultChallengeScheme = "JwtBearer";
            })
            .AddJwtBearer("JwtBearer", options =>
            {
                options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings["Issuer"],
                    ValidAudience = jwtSettings["Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey!))
                };
                options.Events = new Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];
                        var path = context.HttpContext.Request.Path;
                        if (!string.IsNullOrEmpty(accessToken) &&
                            (path.StartsWithSegments("/hubs")))
                        {
                            context.Token = accessToken;
                        }
                        return Task.CompletedTask;
                    }
                };
            });

            builder.Services.AddSignalR();
            builder.Services.AddScoped<Microsoft.AspNetCore.Authorization.IAuthorizationHandler, TrackOwnershipHandler>();
            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("TrackOwnership", policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.Requirements.Add(new TrackOwnershipRequirement());
                });
            });
            builder.Services.AddAntiforgery();
            #endregion

            #region CORS
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                });
            });
            #endregion

            #region DI Registrations
            builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
            builder.Services.AddScoped<IImageHelper, ImageHelper>();
            builder.Host.UseSerilog();
            builder.Services.AddAuditLogging();
            builder.Services.AddScoped<IAuditLogger, AuditLogger>();
            builder.Services.AddCustomRateLimiting();
            builder.Services.AddScoped<ITokenService, JwtService>();
            builder.Services.AddScoped<IMailKitEmailService, MailKitEmailService>();
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddMediatR(Assembly.GetExecutingAssembly());
            builder.Services.AddScoped<IUpdateUserProfileOrchestrator, UpdateUserProfileOrchestrator>();
            builder.Services.AddTransient(
            typeof(IPipelineBehavior<,>),
            typeof(ValidationBehavior<,>)
            );
            builder.Services.AddScoped<ELearningProject.Features.Progress.Services.IProgressService, ELearningProject.Features.Progress.Services.ProgressService>();
            builder.Services.AddScoped<ELearningProject.Features.Notifications.Services.INotificationService, ELearningProject.Features.Notifications.Services.NotificationService>();
            builder.Services.AddScoped<IFileService, FileService>();

            


            #endregion


            #region Caching
            builder.Services.AddMemoryCache();
            builder.Services.AddHttpContextAccessor();

            #endregion

            #endregion

            var app = builder.Build();

            #region --- Migration & Seeding ---
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;

                try
                {
                    Console.WriteLine("📊 [Auth] Starting database migration...");

                    var context = services.GetRequiredService<UniversitySystemAuthContext>();
                    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
                    var roleManager = services.GetRequiredService<RoleManager<ApplicationRole>>();

                    // 1️⃣ Apply migrations
                    await context.Database.MigrateAsync();

                    // 2️⃣ Seed roles
                    await RoleSeeder.SeedRolesAsync(roleManager);

                    // 3️⃣ Seed SuperAdmin (depends on roles)
                    await SuperAdminSeeder.SeedSuperAdminAsync(userManager, roleManager);

                    Console.WriteLine("✅ [Auth] Database migration & seeding completed.");
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"❌ [Auth] Error during migration/seeding: {ex.Message}");
                    Console.ResetColor();
                }
            }


            #endregion

            #region --- Pipeline ---

            // ✅ Global error handler - MUST be first in pipeline
            app.UseMiddleware<Auth.Middlewares.GlobalExceptionMiddleware>();

            // Swagger always enabled for dev convenience
            app.UseSwagger();
            app.UseSwaggerUI();

            //app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCors("AllowAll"); // CORS first

            app.UseMiddleware<ELearningProject.Middlewares.CorrelationIdMiddleware>();
            app.UseAuthentication();
            app.UseAntiforgery();
            app.UseAuthorization();
            app.UseRateLimiter();
            app.MapControllers();
            app.MapGet("/", () => "Auth Service is running...");
            app.MapAuthEndpoints();
            app.MapBatchEndpoints();
            app.MapSubmissionEndpoints();
            app.MapAssignmentEndpoints();
            app.MapProgressEndpoints();
            app.MapTrackEndpoints();
            app.MapLectureEndpoints();
            app.MapMaterialEndpoints();
            app.MapExamEndpoints();
            app.MapNotificationEndpoints();
            app.MapFileEndpoints();
            app.MapDashboardEndpoints();
            app.MapArticleEndpoints();
            app.MapCoinEndpoints();

            app.MapHub<ELearningProject.Features.Notifications.Hubs.NotificationHub>("/hubs/notifications");

            #endregion

            app.Run();
        }

    }
}
