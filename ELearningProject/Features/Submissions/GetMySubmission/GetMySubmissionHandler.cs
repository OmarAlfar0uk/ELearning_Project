
using ELearningProject.Contarcts;
using ELearningProject.Features.Shared;
using ELearningProject.Features.Submissions.DTOs;
using ELearningProject.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ELearningProject.Features.Submissions.GetMySubmission
{
    public class GetMySubmissionHandler : IRequestHandler<GetMySubmissionQuery, EndpointResponse<SubmissionDto>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetMySubmissionHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<EndpointResponse<SubmissionDto>> Handle(GetMySubmissionQuery request, CancellationToken cancellationToken)
        {
            var submissionRepository = _unitOfWork.GetRepository<Submission>();

            var submission = await submissionRepository.FindByCondition(s =>
                s.AssignmentId == request.AssignmentId && s.StudentId == request.StudentId)
                .FirstOrDefaultAsync(cancellationToken);

            if (submission == null)
            {
                return EndpointResponse<SubmissionDto>.NotFoundResponse("No submission found for this assignment.");
            }

            var dto = new SubmissionDto(
                submission.Id,
                submission.AssignmentId,
                submission.FileUrl,
                submission.Score,
                submission.Feedback,
                submission.IsFinalized,
                submission.CreatedAt
            );

            return EndpointResponse<SubmissionDto>.SuccessResponse(dto);
        }
    }
}
