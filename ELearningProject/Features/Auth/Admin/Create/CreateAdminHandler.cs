using Auth.Models;
using ELearningProject.Features.Shared;
using ELearningProject.Contracts;
using ELearningProject.Data;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace ELearningProject.Features.Auth.Admin.Create
{
    public class CreateAdminHandler
        : IRequestHandler<CreateAdminCommand, EndpointResponse<CreateAdminResponse>>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly UniversitySystemAuthContext _context;
        private readonly IAuditLogger _auditLogger;

        public CreateAdminHandler(
            UserManager<ApplicationUser> userManager,
            UniversitySystemAuthContext context,
            IAuditLogger auditLogger)
        {
            _userManager = userManager;
            _context = context;
            _auditLogger = auditLogger;
        }

        public async Task<EndpointResponse<CreateAdminResponse>> Handle(
            CreateAdminCommand request,
            CancellationToken cancellationToken)
        {
            // 1️⃣ Check if email exists
            if (await _userManager.FindByEmailAsync(request.Email) != null)
            {
                return EndpointResponse<CreateAdminResponse>.ErrorResponse(
                    "Email already exists", 409);
            }

            await using var transaction =
                await _context.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                // 2️⃣ Create admin
                var adminUser = new ApplicationUser
                {
                    Id = Guid.NewGuid(),
                    UserName = request.Email,
                    Email = request.Email,
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    IsActivated = true,
                    EmailConfirmed = true
                };

                var result = await _userManager.CreateAsync(adminUser, request.Password);
                if (!result.Succeeded)
                {
                    await transaction.RollbackAsync(cancellationToken);

                    return EndpointResponse<CreateAdminResponse>.ErrorResponse(
                        "Failed to create admin",
                        400,
                        result.Errors.Select(e => e.Description).ToList()
                    );
                }

                // 3️⃣ Assign role
                var roleResult = await _userManager.AddToRoleAsync(adminUser, "Admin");
                if (!roleResult.Succeeded)
                {
                    await transaction.RollbackAsync(cancellationToken);

                    return EndpointResponse<CreateAdminResponse>.ErrorResponse(
                        "Failed to assign role",
                        400,
                        roleResult.Errors.Select(e => e.Description).ToList()
                    );
                }

                // 4️⃣ Commit DB changes
                await transaction.CommitAsync(cancellationToken);

                // 5️⃣ Audit (❗ خارج الترانزاكشن)
                await _auditLogger.LogAsync(
                    action: "CreateAdmin",
                    targetId: adminUser.Id.ToString(),
                    description: $"Admin created with email {adminUser.Email}"
                );

                return EndpointResponse<CreateAdminResponse>.SuccessResponse(
                    new CreateAdminResponse
                    {
                        AdminId = adminUser.Id,
                        Email = adminUser.Email!,
                        Role = "Admin"
                    },
                    "Admin created successfully",
                    201
                );
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);

                Log.Error(ex, "CreateAdmin failed for {Email}", request.Email);

                return EndpointResponse<CreateAdminResponse>.ErrorResponse(
                    "Unexpected error occurred while creating admin",
                    500
                );
            }
        }
    }
}
