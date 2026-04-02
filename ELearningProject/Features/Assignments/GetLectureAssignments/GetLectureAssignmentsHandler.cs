
using ELearningProject.Contarcts;
using ELearningProject.Features.Assignments.DTOs;
using ELearningProject.Features.Shared;
using ELearningProject.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ELearningProject.Features.Assignments.GetLectureAssignments
{
    public class GetLectureAssignmentsHandler : IRequestHandler<GetLectureAssignmentsQuery, EndpointResponse<List<AssignmentDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetLectureAssignmentsHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<EndpointResponse<List<AssignmentDto>>> Handle(GetLectureAssignmentsQuery request, CancellationToken cancellationToken)
        {
            var assignmentRepository = _unitOfWork.GetRepository<Assignment>();
            var lectureRepository = _unitOfWork.GetRepository<Lecture>();

            // Optional: Check if Lecture exists if we want specific 404
            var lectureExists = await lectureRepository.GetByIdAsync(request.LectureId);
            if (lectureExists == null)
            {
                 return EndpointResponse<List<AssignmentDto>>.NotFoundResponse("Lecture not found.");
            }

            var assignments = await assignmentRepository.FindByCondition(a => a.LectureId == request.LectureId && !a.IsDeleted)
                .Select(a => new AssignmentDto(
                    a.Id,
                    a.Title,
                    a.MaxScore,
                    a.DueDate,
                    a.IsClosed,
                    a.FileUrl
                ))
                .ToListAsync(cancellationToken);

            return EndpointResponse<List<AssignmentDto>>.SuccessResponse(assignments);
        }
    }
}
