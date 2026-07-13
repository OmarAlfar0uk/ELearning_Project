using ELearningProject.Contarcts; // For IUnitOfWork and IGenericRepository (note typo in namespace)
using ELearningProject.Features.Shared;
using ELearningProject.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ELearningProject.Features.Materials
{
    /// <summary>
    /// Query to retrieve all materials associated with a lecture.
    /// </summary>
    public record GetLectureMaterialsQuery(Guid LectureId) : IRequest<EndpointResponse<List<MaterialDto>>>;

    /// <summary>
    /// Handler to process GetLectureMaterialsQuery.
    /// </summary>
    public class GetLectureMaterialsHandler : IRequestHandler<GetLectureMaterialsQuery, EndpointResponse<List<MaterialDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetLectureMaterialsHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<EndpointResponse<List<MaterialDto>>> Handle(GetLectureMaterialsQuery request, CancellationToken cancellationToken)
        {
            // 1. Verify Lecture exists
            var lectureRepository = _unitOfWork.GetRepository<Lecture>();
            var lecture = await lectureRepository.GetByIdAsync(request.LectureId);
            if (lecture == null)
            {
                return EndpointResponse<List<MaterialDto>>.NotFoundResponse("Lecture not found.");
            }

            // 2. Query Materials
            var materialRepository = _unitOfWork.GetRepository<Material>();
            var materials = await materialRepository
                .FindByCondition(m => m.LectureId == request.LectureId)
                .OrderBy(m => m.CreatedAt)
                .ToListAsync(cancellationToken);

            // 3. Map to DTO list
            var dtos = materials.Select(m => new MaterialDto
            {
                Id = m.Id,
                Name = m.Name,
                Type = m.Type.ToString(),
                FileUrl = m.FileUrl,
                ExternalLink = m.ExternalLink,
                LectureId = m.LectureId,
                CreatedAt = m.CreatedAt
            }).ToList();

            return EndpointResponse<List<MaterialDto>>.SuccessResponse(dtos, "Lecture materials retrieved successfully.");
        }
    }
}
