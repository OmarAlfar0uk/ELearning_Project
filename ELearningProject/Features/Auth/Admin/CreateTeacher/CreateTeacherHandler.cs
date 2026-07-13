using Auth.Contarcts; // For IMailKitEmailService (note typo in namespace)
using Auth.Models;
using ELearningProject.Contarcts; // For IUnitOfWork and IGenericRepository (note typo in namespace)
using ELearningProject.Extensions;
using ELearningProject.Features.Shared;
using ELearningProject.Models;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace ELearningProject.Features.Auth.Admin.CreateTeacher
{
    /// <summary>
    /// Handler to process CreateTeacherCommand, creating a teacher user, assigning roles and tracks.
    /// </summary>
    public class CreateTeacherHandler
        : IRequestHandler<CreateTeacherCommand, EndpointResponse<TeacherDto>>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMailKitEmailService _emailService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CreateTeacherHandler(
            UserManager<ApplicationUser> userManager,
            IUnitOfWork unitOfWork,
            IMailKitEmailService emailService,
            IHttpContextAccessor httpContextAccessor)
        {
            _userManager = userManager;
            _unitOfWork = unitOfWork;
            _emailService = emailService;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<EndpointResponse<TeacherDto>> Handle(
            CreateTeacherCommand request,
            CancellationToken cancellationToken)
        {
            // 1. Check uniqueness of email/username, including soft-deleted users
            var normalizedEmail = _userManager.NormalizeEmail(request.Email);
            var normalizedUserName = _userManager.NormalizeName(request.Email);

            var userConflict = await _userManager.Users
                .IgnoreQueryFilters()
                .AnyAsync(u => u.NormalizedEmail == normalizedEmail || u.NormalizedUserName == normalizedUserName, cancellationToken);

            if (userConflict)
            {
                return EndpointResponse<TeacherDto>.ErrorResponse("Email already exists", 409);
            }

            // 2. Validate all TrackIds exist in the Tracks table
            var trackRepository = _unitOfWork.GetRepository<Track>();
            var tracks = await trackRepository
                .FindByCondition(t => request.TrackIds.Contains(t.Id))
                .ToListAsync(cancellationToken);

            var uniqueRequestedTrackCount = request.TrackIds.Distinct().Count();
            if (tracks.Count != uniqueRequestedTrackCount)
            {
                return EndpointResponse<TeacherDto>.ErrorResponse("One or more track IDs are invalid.", 404);
            }

            // 3. Create the Teacher User
            var teacher = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                UserName = request.Email,
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                IsActivated = true,
                EmailConfirmed = true
            };

            // Generate secure temporary password
            var tempPassword = "Temp@" + Guid.NewGuid().ToString("N")[..8] + "1";

            var createResult = await _userManager.CreateAsync(teacher, tempPassword);
            if (!createResult.Succeeded)
            {
                return EndpointResponse<TeacherDto>.ErrorResponse(
                    "Failed to create teacher user.",
                    400,
                    createResult.Errors.Select(e => e.Description).ToList()
                );
            }

            try
            {
                // 4. Add the user to the "Instructor" role
                var roleResult = await _userManager.AddToRoleAsync(teacher, "Instructor");
                if (!roleResult.Succeeded)
                {
                    // Rollback user creation if role assignment fails
                    await _userManager.DeleteAsync(teacher);
                    return EndpointResponse<TeacherDto>.ErrorResponse(
                        "Failed to assign instructor role.",
                        400,
                        roleResult.Errors.Select(e => e.Description).ToList()
                    );
                }

                // 5. Create InstructorTrack row per TrackId
                var adminUser = _httpContextAccessor.HttpContext?.User;
                var adminId = adminUser != null ? adminUser.GetUserId() : Guid.Empty;

                var instructorTrackRepository = _unitOfWork.GetRepository<InstructorTrack>();

                foreach (var trackId in request.TrackIds)
                {
                    var instructorTrack = new InstructorTrack
                    {
                        Id = Guid.NewGuid(),
                        InstructorId = teacher.Id,
                        TrackId = trackId,
                        AssignedByAdminId = adminId
                    };
                    await instructorTrackRepository.CreateAsync(instructorTrack);
                }

                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to complete teacher setup, rolling back user creation.");
                await _userManager.DeleteAsync(teacher);
                return EndpointResponse<TeacherDto>.ErrorResponse(
                    "Unexpected error occurred during teacher track association.",
                    500
                );
            }

            // 6. Send temporary password to teacher's email
            try
            {
                var fullName = $"{teacher.FirstName} {teacher.LastName}";
                var emailBody = $"""
                    <html>
                    <body style="font-family: Arial, sans-serif; color: #333; padding: 20px;">
                      <h2>Welcome to ELearning Platform 🎓</h2>
                      <p>Hello <strong>{fullName}</strong>,</p>
                      <p>Your teacher account has been created successfully. Below are your login credentials:</p>
                      <div style="background:#f4f4f4; border-left:4px solid #4CAF50; padding:16px; margin:20px 0; font-size:20px; font-weight:bold; text-align:center;">
                        Username/Email: {teacher.Email}<br/>
                        Temporary Password: {tempPassword}
                      </div>
                      <p>Please change your password immediately after logging in.</p>
                      <br/>
                      <p>Best regards,<br/>ELearning Team</p>
                    </body>
                    </html>
                    """;

                await _emailService.SendEmailAsync(
                    teacher.Email!,
                    "Your Teacher Account Credentials — ELearning Platform",
                    emailBody
                );
            }
            catch (Exception emailEx)
            {
                Log.Warning(emailEx, "Failed to send credentials email to {Email}", teacher.Email);
            }

            // 7. Map and return response
            var teacherDto = new TeacherDto
            {
                Id = teacher.Id,
                FirstName = teacher.FirstName,
                LastName = teacher.LastName,
                Email = teacher.Email!,
                AssignedTrackNames = tracks.Select(t => t.Name).ToList()
            };

            return EndpointResponse<TeacherDto>.SuccessResponse(
                teacherDto,
                "Teacher created and tracks assigned successfully",
                201
            );
        }
    }
}
