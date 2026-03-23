
using ELearningProject.Contarcts;
using ELearningProject.Features.Shared;
using ELearningProject.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ELearningProject.Extensions;

namespace ELearningProject.Features.Lectures.CreateLecture
{
    public class CreateLectureHandler : IRequestHandler<CreateLectureCommand, EndpointResponse<Guid>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ELearningProject.Features.Notifications.Services.INotificationService _notificationService;
        private readonly Microsoft.AspNetCore.Http.IHttpContextAccessor _httpContextAccessor;

        public CreateLectureHandler(
            IUnitOfWork unitOfWork, 
            ELearningProject.Features.Notifications.Services.INotificationService notificationService,
            Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor)
        {
            _unitOfWork = unitOfWork;
            _notificationService = notificationService;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<EndpointResponse<Guid>> Handle(CreateLectureCommand request, CancellationToken cancellationToken)
        {
            var trackRepository = _unitOfWork.GetRepository<Track>();
            var lectureRepository = _unitOfWork.GetRepository<Lecture>();

            // 1. Check Track exists
            var track = await trackRepository.GetByIdAsync(request.TrackId);
            if (track == null)
            {
                return EndpointResponse<Guid>.NotFoundResponse("Track not found.");
            }

            // RBAC Check for Instructor
            var user = _httpContextAccessor.HttpContext?.User;
            var userId = ELearningProject.Extensions.ClaimsPrincipalExtensions.GetUserId(user!);
            var userRole = ELearningProject.Extensions.ClaimsPrincipalExtensions.GetUserRole(user!);

            if (userRole != "Admin" && userRole != "SuperAdmin")
            {
                return EndpointResponse<Guid>.ErrorResponse("You don't have permission to create a lecture in this track.", 403);
            }

            // 2. Check Title Uniqueness in Track
            var existingLecture = await lectureRepository.FindByCondition(l => 
                l.TrackId == request.TrackId && l.Title == request.Title)
                .AnyAsync(cancellationToken);

            if (existingLecture)
            {
                return EndpointResponse<Guid>.ErrorResponse("Lecture with this title already exists in the track.", 409);
            }

            // 3. Create Lecture
            var lecture = new Lecture
            {
                Title = request.Title,
                ContentText = request.ContentText,
                DriveLink = request.DriveLink,
                FileUrl = request.FileUrl,
                TrackId = request.TrackId
            };

            await lectureRepository.CreateAsync(lecture);
            await _unitOfWork.SaveChangesAsync();

            // Notify students in the batch
            var students = await _unitOfWork.GetRepository<Models.BatchStudent>()
                .FindByCondition(bs => bs.Batch.Tracks.Any(t => t.Id == request.TrackId))
                .Select(bs => bs.StudentId)
                .ToListAsync(cancellationToken);

            foreach (var studentId in students)
            {
                await _notificationService.CreateNotificationAsync(
                    studentId,
                    "New Lecture Added",
                    $"A new lecture '{lecture.Title}' has been added to your track.",
                    ELearningProject.Models.NotificationType.LectureAdded,
                    $"/lectures/{lecture.Id}"
                );
            }

            return EndpointResponse<Guid>.SuccessResponse(lecture.Id, "Lecture created successfully.", 201);
        }
    }
}
