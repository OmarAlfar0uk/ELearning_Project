using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using ELearningProject.Contarcts; // For IUnitOfWork and IGenericRepository (note typo in namespace)
using ELearningProject.Models;
using ELearningProject.Extensions;
using System.Security.Claims;

namespace ELearningProject.Features.Auth
{
    /// <summary>
    /// Handler to evaluate the TrackOwnershipRequirement against the request context and route parameters.
    /// </summary>
    public class TrackOwnershipHandler : AuthorizationHandler<TrackOwnershipRequirement>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUnitOfWork _unitOfWork;

        public TrackOwnershipHandler(IHttpContextAccessor httpContextAccessor, IUnitOfWork unitOfWork)
        {
            _httpContextAccessor = httpContextAccessor;
            _unitOfWork = unitOfWork;
        }

        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context, 
            TrackOwnershipRequirement requirement)
        {
            var user = context.User;
            if (user == null || !user.Identity!.IsAuthenticated)
            {
                return;
            }

            var role = user.GetUserRole();

            // 1. SuperAdmin and Admin roles bypass ownership checks and succeed automatically
            if (role == "SuperAdmin" || role == "Admin")
            {
                context.Succeed(requirement);
                return;
            }

            // 2. Only Instructors are subject to the ownership checks. Other roles (e.g. Student) fail here.
            if (role != "Instructor")
            {
                return;
            }

            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null)
            {
                return;
            }

            var routeValues = httpContext.Request.RouteValues;
            Guid? trackId = null;

            // 3. Resolve parent Track ID from route parameters
            if (routeValues.TryGetValue("trackId", out var trackIdObj) && trackIdObj != null)
            {
                if (Guid.TryParse(trackIdObj.ToString(), out var parsedTrackId))
                {
                    trackId = parsedTrackId;
                }
            }
            else if (routeValues.TryGetValue("lectureId", out var lectureIdObj) && lectureIdObj != null)
            {
                if (Guid.TryParse(lectureIdObj.ToString(), out var lectureId))
                {
                    // Query Lecture to find parent TrackId
                    var lecture = await _unitOfWork.GetRepository<Lecture>().GetByIdAsync(lectureId);
                    if (lecture != null)
                    {
                        trackId = lecture.TrackId;
                    }
                }
            }
            else if (routeValues.TryGetValue("assignmentId", out var assignmentIdObj) && assignmentIdObj != null)
            {
                if (Guid.TryParse(assignmentIdObj.ToString(), out var assignmentId))
                {
                    // Query Assignment to find parent TrackId
                    var assignment = await _unitOfWork.GetRepository<Assignment>().GetByIdAsync(assignmentId);
                    if (assignment != null)
                    {
                        trackId = assignment.TrackId;
                    }
                }
            }
            else if (routeValues.TryGetValue("materialId", out var materialIdObj) && materialIdObj != null)
            {
                if (Guid.TryParse(materialIdObj.ToString(), out var materialId))
                {
                    // Query Material to find parent TrackId via Lecture
                    var material = await _unitOfWork.GetRepository<Material>().GetByIdAsync(materialId);
                    if (material != null)
                    {
                        var lecture = await _unitOfWork.GetRepository<Lecture>().GetByIdAsync(material.LectureId);
                        if (lecture != null)
                        {
                            trackId = lecture.TrackId;
                        }
                    }
                }
            }
            else if (routeValues.TryGetValue("examId", out var examIdObj) && examIdObj != null)
            {
                if (Guid.TryParse(examIdObj.ToString(), out var examId))
                {
                    // Query Exam to find its parent TrackId
                    var exam = await _unitOfWork.GetRepository<Exam>().GetByIdAsync(examId);
                    if (exam != null)
                    {
                        trackId = exam.TrackId;
                    }
                }
            }
            else if (routeValues.TryGetValue("answerId", out var answerIdObj) && answerIdObj != null)
            {
                if (Guid.TryParse(answerIdObj.ToString(), out var answerId))
                {
                    // Query ExamAnswer -> ExamQuestion -> Exam to find its parent TrackId
                    var answer = await _unitOfWork
                        .GetRepository<ExamAnswer>()
                        .FindByCondition(currentAnswer => currentAnswer.Id == answerId)
                        .Include(currentAnswer => currentAnswer.ExamQuestion)
                            .ThenInclude(question => question.Exam)
                        .FirstOrDefaultAsync();

                    if (answer?.ExamQuestion?.Exam != null)
                    {
                        trackId = answer.ExamQuestion.Exam.TrackId;
                    }
                }
            }

            // If no valid target track could be resolved from the route parameters, deny access
            if (trackId == null)
            {
                return;
            }

            // 4. Verify in the database if the instructor is assigned to the resolved track
            var userId = user.GetUserId();
            var instructorTrackRepository = _unitOfWork.GetRepository<InstructorTrack>();
            
            var isAssigned = await instructorTrackRepository
                .FindByCondition(it => it.InstructorId == userId && it.TrackId == trackId.Value)
                .AnyAsync();

            if (isAssigned)
            {
                context.Succeed(requirement);
            }
        }
    }
}
