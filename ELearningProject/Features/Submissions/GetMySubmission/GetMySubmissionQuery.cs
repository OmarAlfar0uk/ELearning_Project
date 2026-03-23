
using ELearningProject.Features.Shared;
using ELearningProject.Features.Submissions.DTOs;
using MediatR;
using System.Text.Json.Serialization;

namespace ELearningProject.Features.Submissions.GetMySubmission
{
    public record GetMySubmissionQuery(Guid AssignmentId) : IRequest<EndpointResponse<SubmissionDto>>
    {
        [JsonIgnore]
        public Guid StudentId { get; set; }
    }
}
