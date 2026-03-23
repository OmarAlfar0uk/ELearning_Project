
using ELearningProject.Contarcts;
using ELearningProject.Features.Shared;
using ELearningProject.Models;
using MediatR;

namespace ELearningProject.Features.Assignments.CreateAssignment
{
    public class CreateAssignmentHandler : IRequestHandler<CreateAssignmentCommand, EndpointResponse<Guid>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public CreateAssignmentHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<EndpointResponse<Guid>> Handle(CreateAssignmentCommand request, CancellationToken cancellationToken)
        {
            var lectureRepository = _unitOfWork.GetRepository<Lecture>();
            var assignmentRepository = _unitOfWork.GetRepository<Assignment>();

            // 1. Check Lecture exists
            var lecture = await lectureRepository.GetByIdAsync(request.LectureId);
            if (lecture == null)
            {
                return EndpointResponse<Guid>.NotFoundResponse("Lecture not found.");
            }

            // 2. Create Assignment
            var assignment = new Assignment
            {
                Title = request.Title,
                MaxScore = request.MaxScore,
                DueDate = request.DueDate,
                LectureId = request.LectureId,
                IsClosed = false
            };

            await assignmentRepository.CreateAsync(assignment);
            await _unitOfWork.SaveChangesAsync();

            return EndpointResponse<Guid>.SuccessResponse(assignment.Id, "Assignment created successfully.", 201);
        }
    }
}
