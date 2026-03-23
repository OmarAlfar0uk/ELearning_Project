
using ELearningProject.Features.Shared;
using MediatR;

namespace ELearningProject.Features.Lectures.DeleteLecture
{
    public record DeleteLectureCommand(Guid LectureId) : IRequest<RequestResponse<string>>;
}
