using ELearningProject.Features.Shared;
using MediatR;
using System;

namespace ELearningProject.Features.Batche.GetBatchById
{
    public record GetBatchByIdQuery(Guid Id) : IRequest<EndpointResponse<BatchDetailsDto>>;
}
