using ELearningProject.Contarcts; // For IUnitOfWork and IGenericRepository (note typo in namespace)
using ELearningProject.Features.Shared;
using ELearningProject.Models;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace ELearningProject.Features.Materials
{
    /// <summary>
    /// Handler to process AddMaterialCommand, validating lecture existence and persisting the material.
    /// </summary>
    public class AddMaterialHandler : IRequestHandler<AddMaterialCommand, EndpointResponse<MaterialDto>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public AddMaterialHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<EndpointResponse<MaterialDto>> Handle(AddMaterialCommand request, CancellationToken cancellationToken)
        {
            // 1. Verify Lecture exists
            var lectureRepository = _unitOfWork.GetRepository<Lecture>();
            var lecture = await lectureRepository.GetByIdAsync(request.LectureId);
            if (lecture == null)
            {
                return EndpointResponse<MaterialDto>.NotFoundResponse("Lecture not found.");
            }

            // 2. Create Material
            var material = new Material
            {
                Name = request.Name,
                Type = request.Type,
                FileUrl = request.FileUrl,
                ExternalLink = request.ExternalLink,
                LectureId = request.LectureId
            };

            var materialRepository = _unitOfWork.GetRepository<Material>();
            await materialRepository.CreateAsync(material);
            await _unitOfWork.SaveChangesAsync();

            // 3. Map to DTO
            var dto = new MaterialDto
            {
                Id = material.Id,
                Name = material.Name,
                Type = material.Type.ToString(),
                FileUrl = material.FileUrl,
                ExternalLink = material.ExternalLink,
                LectureId = material.LectureId,
                CreatedAt = material.CreatedAt
            };

            return EndpointResponse<MaterialDto>.SuccessResponse(dto, "Material added successfully.", 201);
        }
    }
}
