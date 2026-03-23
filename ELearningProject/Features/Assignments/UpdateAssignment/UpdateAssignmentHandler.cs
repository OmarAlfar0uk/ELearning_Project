
using ELearningProject.Contarcts;
using ELearningProject.Features.Shared;
using ELearningProject.Models;
using MediatR;

namespace ELearningProject.Features.Assignments.UpdateAssignment
{
    public class UpdateAssignmentHandler : IRequestHandler<UpdateAssignmentCommand, RequestResponse<string>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public UpdateAssignmentHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<RequestResponse<string>> Handle(UpdateAssignmentCommand request, CancellationToken cancellationToken)
        {
            var assignmentRepository = _unitOfWork.GetRepository<Assignment>();

            var assignment = await assignmentRepository.GetByIdAsync(request.AssignmentId);
            if (assignment == null)
            {
                return RequestResponse<string>.Fail("Assignment not found.");
            }

            if (assignment.IsClosed)
            {
                return RequestResponse<string>.Fail("Cannot update a closed assignment.");
            }

            assignment.Title = request.Title;
            assignment.MaxScore = request.MaxScore;
            assignment.DueDate = request.DueDate;

            assignmentRepository.Update(assignment);
            await _unitOfWork.SaveChangesAsync();

            return RequestResponse<string>.Success(null, "Assignment updated successfully.");
        }
    }
}
