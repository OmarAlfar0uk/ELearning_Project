using Auth.Contarcts;
using Auth.Models;
using ELearningProject.Features.Shared;
using ELearningProject.Contracts;
using ELearningProject.Data;
using ELearningProject.Models;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace ELearningProject.Features.Auth.Admin.CreateStudent
{
    public class CreateStudentHandler
        : IRequestHandler<CreateStudentCommand, EndpointResponse<CreateStudentResponse>>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly UniversitySystemAuthContext _context;
        private readonly IAuditLogger _auditLogger;
        private readonly IMailKitEmailService _emailService;

        public CreateStudentHandler(
            UserManager<ApplicationUser> userManager,
            UniversitySystemAuthContext context,
            IAuditLogger auditLogger,
            IMailKitEmailService emailService)
        {
            _userManager = userManager;
            _context = context;
            _auditLogger = auditLogger;
            _emailService = emailService;
        }

        public async Task<EndpointResponse<CreateStudentResponse>> Handle(
            CreateStudentCommand request,
            CancellationToken cancellationToken)
        {
            // 1️⃣ Check email/username uniqueness, including soft-deleted users.
            if (await IdentityUserConflictHelper.ExistsByEmailOrUsernameIncludingDeletedAsync(
                    _context,
                    _userManager,
                    request.Email,
                    cancellationToken))
            {
                return EndpointResponse<CreateStudentResponse>
                    .ErrorResponse("Email already exists", 409);
            }

            // 2️⃣ Check Batch Exists (Before Transaction to fail fast)
            var batch = await _context.Batches.FindAsync(new object[] { request.BatchId }, cancellationToken);
            if (batch == null)
            {
                 return EndpointResponse<CreateStudentResponse>
                    .ErrorResponse("Batch not found", 404);
            }

            await using var transaction =
                await _context.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                if (!Enum.TryParse<Gender>(request.Gender, true, out var gender))
                {
                    return EndpointResponse<CreateStudentResponse>.ErrorResponse("Invalid gender value", 400);
                }

                // 3️⃣ Create User
                var student = new ApplicationUser
                {
                    Id = Guid.NewGuid(),
                    UserName = request.Email,
                    Email = request.Email,
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    Gender = gender, 
                    IsActivated = false,
                    EmailConfirmed = false
                };

                var createResult = await _userManager.CreateAsync(student);
                if (!createResult.Succeeded)
                {
                    await transaction.RollbackAsync(cancellationToken);

                    return EndpointResponse<CreateStudentResponse>.ErrorResponse(
                        "Failed to create student",
                        400,
                        createResult.Errors.Select(e => e.Description).ToList()
                    );
                }

                // 4️⃣ Assign Student role
                var roleResult = await _userManager.AddToRoleAsync(student, "Student");
                if (!roleResult.Succeeded)
                {
                    await transaction.RollbackAsync(cancellationToken);

                    return EndpointResponse<CreateStudentResponse>.ErrorResponse(
                        "Failed to assign student role",
                        400,
                        roleResult.Errors.Select(e => e.Description).ToList()
                    );
                }

                // 5️⃣ Create BatchStudent
                var batchStudent = new BatchStudent
                {
                    StudentId = student.Id,
                    BatchId = request.BatchId,
                    Rank = 0,
                    AverageScore = 0
                };
                
                _context.BatchStudents.Add(batchStudent);
                // We need to save changes here or later? 
                // Context tracks it. SaveChanges at the end should be enough for BatchStudent and ActivationCode.
                // However, userManager operations already saved changes to Users/UserRoles tables.

                // 6️⃣ Generate activation code
                var code = Guid.NewGuid().ToString("N")[..6].ToUpper();

                var activationCode = new ActivationCode
                {
                    Id = Guid.NewGuid(),
                    Code = code,
                    UserId = student.Id,
                    Role = "Student",
                    ExpiryDate = DateTime.UtcNow.AddDays(3),
                    IsUsed = false
                };

                _context.ActivationCodes.Add(activationCode);
                
                // Save BatchStudent and ActivationCode
                await _context.SaveChangesAsync(cancellationToken);

                // 7️⃣ Commit transaction
                await transaction.CommitAsync(cancellationToken);

                // 8️⃣ Audit log (outside transaction)
                await _auditLogger.LogAsync(
                    action: "CreateStudent",
                    targetId: student.Id.ToString(),
                    description: $"Student created with email {student.Email} and added to batch {batch.Name}"
                );

                // 9️⃣ Send activation email
                try
                {
                    var fullName = $"{student.FirstName} {student.LastName}";
                    var emailBody = $"""
                        <html>
                        <body style="font-family: Arial, sans-serif; color: #333; padding: 20px;">
                          <h2>Welcome to ELearning Platform 🎓</h2>
                          <p>Hello <strong>{fullName}</strong>,</p>
                          <p>Your student account has been created successfully. Use the activation code below to activate your account and set your password:</p>
                          <div style="background:#f4f4f4; border-left:4px solid #4CAF50; padding:16px; margin:20px 0; font-size:28px; font-weight:bold; letter-spacing:6px; text-align:center;">
                            {code}
                          </div>
                          <p>This code is valid for <strong>3 days</strong>.</p>
                          <p>If you did not expect this email, please ignore it.</p>
                          <br/>
                          <p>Best regards,<br/>ELearning Team</p>
                        </body>
                        </html>
                        """;

                    await _emailService.SendEmailAsync(
                        student.Email!,
                        "Your Activation Code — ELearning Platform",
                        emailBody
                    );
                }
                catch (Exception emailEx)
                {
                    Log.Warning(emailEx, "Failed to send activation email to {Email}", student.Email);
                    // Don't fail the request if email sending fails
                }

                return EndpointResponse<CreateStudentResponse>.SuccessResponse(
                    new CreateStudentResponse
                    {
                        StudentId = student.Id,
                        Email = student.Email!,
                        ActivationCode = code
                    },
                    "Student created and added to batch successfully",
                    201
                );
            }
            catch (DbUpdateException ex) when (IdentityUserConflictHelper.IsDuplicateUserNameConflict(ex))
            {
                await transaction.RollbackAsync(cancellationToken);

                Log.Warning(ex, "CreateStudent conflict for {Email}", request.Email);

                return EndpointResponse<CreateStudentResponse>.ErrorResponse(
                    "Email already exists",
                    409
                );
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);

                Log.Error(ex, "CreateStudent failed for {Email}", request.Email);

                return EndpointResponse<CreateStudentResponse>.ErrorResponse(
                    "Unexpected error occurred while creating student",
                    500
                );
            }
        }
    }
}
