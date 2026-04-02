
using ELearningProject.Contarcts;
using ELearningProject.Features.Assignments.DTOs;
using ELearningProject.Features.Shared;
using ELearningProject.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ELearningProject.Features.Assignments.GetAssignmentDetails
{
    public class GetAssignmentDetailsHandler : IRequestHandler<GetAssignmentDetailsQuery, EndpointResponse<AssignmentDetailsDto>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetAssignmentDetailsHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<EndpointResponse<AssignmentDetailsDto>> Handle(GetAssignmentDetailsQuery request, CancellationToken cancellationToken)
        {
            var assignmentRepository = _unitOfWork.GetRepository<Assignment>();

            var assignment = await assignmentRepository.FindByCondition(a => a.Id == request.AssignmentId && !a.IsDeleted)
                .Include(a => a.Lecture)
                    .ThenInclude(l => l.Track)
                .FirstOrDefaultAsync(cancellationToken);

            if (assignment == null)
            {
                return EndpointResponse<AssignmentDetailsDto>.NotFoundResponse("Assignment not found.");
            }

            var dto = new AssignmentDetailsDto(
                assignment.Id,
                assignment.Title,
                assignment.MaxScore,
                assignment.DueDate,
                assignment.IsClosed,
                assignment.LectureId,
                assignment.Lecture.Title, // Assuming Lecture has Title
                assignment.Lecture.TrackId,
                assignment.Lecture.Track.Name, // Assuming Track has Name
                assignment.FileUrl
            );

            return EndpointResponse<AssignmentDetailsDto>.SuccessResponse(dto);
        }
    }
}
