
using ELearningProject.Contarcts;
using ELearningProject.Features.Shared;
using ELearningProject.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ELearningProject.Features.Assignments.DeleteAssignment
{
    public class DeleteAssignmentHandler : IRequestHandler<DeleteAssignmentCommand, RequestResponse<string>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public DeleteAssignmentHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<RequestResponse<string>> Handle(DeleteAssignmentCommand request, CancellationToken cancellationToken)
        {
            var assignmentRepository = _unitOfWork.GetRepository<Assignment>();

            var assignment = await assignmentRepository.FindByCondition(a => a.Id == request.AssignmentId)
                .Include(a => a.Submissions) // We need to check for submissions
                .FirstOrDefaultAsync(cancellationToken);

            if (assignment == null)
            {
                return RequestResponse<string>.Fail("Assignment not found.");
            }

            if (assignment.Submissions.Any())
            {
                return RequestResponse<string>.Fail("Cannot delete assignment because it has submissions.");
            }

            assignmentRepository.Delete(assignment);
            await _unitOfWork.SaveChangesAsync();

            return RequestResponse<string>.Success(null, "Assignment deleted successfully.");
        }
    }
}
