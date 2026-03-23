
using ELearningProject.Features.Progress.DTOs;
using ELearningProject.Features.Shared;
using MediatR;
using System.Text.Json.Serialization;

namespace ELearningProject.Features.Progress.GetMyProgress
{
    public record GetMyProgressQuery(Guid TrackId) : IRequest<EndpointResponse<ProgressDto>>
    {
        [JsonIgnore]
        public Guid StudentId { get; set; }
        
        [JsonIgnore]
        public string UserRole { get; set; } = string.Empty;
    }
}
