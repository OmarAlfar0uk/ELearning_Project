
using ELearningProject.Contarcts;
using ELearningProject.Contracts;
using ELearningProject.Features.Shared;
using ELearningProject.Models;
using MediatR;

namespace ELearningProject.Features.Assignments.UpdateAssignment
{
    public class UpdateAssignmentHandler : IRequestHandler<UpdateAssignmentCommand, RequestResponse<string>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IFileService _fileService;

        public UpdateAssignmentHandler(IUnitOfWork unitOfWork, IFileService fileService)
        {
            _unitOfWork = unitOfWork;
            _fileService = fileService;
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

            if (request.File != null && request.File.Length > 0)
            {
                try
                {
                    var newFileUrl = await _fileService.SaveFileAsync(request.File, "assignments");

                    if (!string.IsNullOrWhiteSpace(assignment.FileUrl))
                    {
                        await _fileService.DeleteFileAsync(assignment.FileUrl);
                    }

                    assignment.FileUrl = newFileUrl;
                }
                catch (ArgumentException ex)
                {
                    return RequestResponse<string>.Fail(ex.Message);
                }
            }

            assignmentRepository.Update(assignment);
            await _unitOfWork.SaveChangesAsync();

            return RequestResponse<string>.Success(null, "Assignment updated successfully.");
        }
    }
}
