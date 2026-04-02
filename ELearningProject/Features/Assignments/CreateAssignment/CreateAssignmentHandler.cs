
using ELearningProject.Contarcts;
using ELearningProject.Contracts;
using ELearningProject.Features.Shared;
using ELearningProject.Models;
using MediatR;

namespace ELearningProject.Features.Assignments.CreateAssignment
{
    public class CreateAssignmentHandler : IRequestHandler<CreateAssignmentCommand, EndpointResponse<Guid>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IFileService _fileService;

        public CreateAssignmentHandler(IUnitOfWork unitOfWork, IFileService fileService)
        {
            _unitOfWork = unitOfWork;
            _fileService = fileService;
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

            // 2. Save file if provided
            string? fileUrl = null;
            if (request.File != null && request.File.Length > 0)
            {
                try
                {
                    fileUrl = await _fileService.SaveFileAsync(request.File, "assignments");
                }
                catch (ArgumentException ex)
                {
                    return EndpointResponse<Guid>.ErrorResponse(ex.Message, 400);
                }
            }

            // 3. Create Assignment
            var assignment = new Assignment
            {
                Title = request.Title,
                MaxScore = request.MaxScore,
                DueDate = request.DueDate,
                LectureId = request.LectureId,
                IsClosed = false,
                FileUrl = fileUrl
            };

            await assignmentRepository.CreateAsync(assignment);
            await _unitOfWork.SaveChangesAsync();

            return EndpointResponse<Guid>.SuccessResponse(assignment.Id, "Assignment created successfully.", 201);
        }
    }
}
