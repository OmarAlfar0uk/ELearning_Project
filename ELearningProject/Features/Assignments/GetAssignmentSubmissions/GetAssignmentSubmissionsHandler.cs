
using ELearningProject.Contarcts;
using ELearningProject.Features.Shared;
using ELearningProject.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ELearningProject.Features.Assignments.GetAssignmentSubmissions
{
    public class GetAssignmentSubmissionsHandler : IRequestHandler<GetAssignmentSubmissionsQuery, EndpointResponse<List<AssignmentSubmissionDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetAssignmentSubmissionsHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<EndpointResponse<List<AssignmentSubmissionDto>>> Handle(GetAssignmentSubmissionsQuery request, CancellationToken cancellationToken)
        {
            var submissionRepository = _unitOfWork.GetRepository<Submission>();

            // Check assignment existence? Not strictly necessary if query returns empty, but good for UX.
            // Let's skip for now unless requested.

            var submissions = await submissionRepository.FindByCondition(s => s.AssignmentId == request.AssignmentId)
                .Include(s => s.Student)
                .Select(s => new AssignmentSubmissionDto(
                    s.Id,
                    s.StudentId,
                    $"{s.Student.FirstName} {s.Student.LastName}",
                    s.FileUrl,
                    s.Score,
                    s.IsFinalized,
                    s.CreatedAt
                ))
                .ToListAsync(cancellationToken);

            return EndpointResponse<List<AssignmentSubmissionDto>>.SuccessResponse(submissions);
        }
    }
}
