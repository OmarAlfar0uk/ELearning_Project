
using ELearningProject.Contarcts;
using ELearningProject.Features.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;


namespace ELearningProject.Features.Files
{
    public static class Endpoints
    {
        public static void MapFileEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("api/v1/files")
                           .WithTags("Files")
                           .RequireAuthorization();

            group.MapPost("/upload", async (
                HttpRequest request,
                ELearningProject.Contracts.IFileService fileService,
                ClaimsPrincipal userPrincipal,
                ILoggerFactory loggerFactory) => 
            {
                var file = request.Form.Files.GetFile("file");
                var folder = request.Form["folder"].FirstOrDefault();

                var logger = loggerFactory.CreateLogger("FileUpload");
                try
                {
                    if (file == null || file.Length == 0)
                    {
                         return (IResult)Results.BadRequest(EndpointResponse<string>.ErrorResponse("No file was provided.", 400));
                    }

                    var userId = userPrincipal.FindFirstValue(ClaimTypes.NameIdentifier);
                    Guid? userGuid = string.IsNullOrEmpty(userId) ? null : Guid.Parse(userId);

                    var url = await fileService.SaveFileAsync(file, folder ?? "General", userGuid);
                    return (IResult)Results.Ok(EndpointResponse<string>.SuccessResponse(url, "File uploaded successfully."));
                }
                catch (ArgumentException ex)
                {
                     logger.LogWarning(ex, "Invalid file upload attempt: {Message}", ex.Message);
                     return (IResult)Results.BadRequest(EndpointResponse<string>.ErrorResponse(ex.Message, 400));
                }
                catch (Exception ex)
                {
                     logger.LogError(ex, "Unexpected error during file upload");
                     return (IResult)Results.StatusCode(500);
                }
            })
            .DisableAntiforgery()
            .WithName("Upload File")
            .WithSummary("Upload a single file with DB tracking")
            .Produces<EndpointResponse<string>>(200)
            .Produces<EndpointResponse<string>>(400);

            group.MapPost("/bulk", async (
                HttpRequest request,
                ELearningProject.Contracts.IFileService fileService,
                ClaimsPrincipal userPrincipal) =>
            {
                var files = request.Form.Files;
                var folder = request.Form["folder"].FirstOrDefault();

                if (files == null || files.Count == 0)
                    return Results.BadRequest(EndpointResponse<string>.ErrorResponse("No files provided.", 400));

                var userId = userPrincipal.FindFirstValue(ClaimTypes.NameIdentifier);
                Guid? userGuid = string.IsNullOrEmpty(userId) ? null : Guid.Parse(userId);

                var urls = await fileService.SaveFilesBulkAsync(files.ToList(), folder ?? "General", userGuid);
                return Results.Ok(EndpointResponse<List<string>>.SuccessResponse(urls, $"{urls.Count} files uploaded successfully."));
            })
            .DisableAntiforgery()
            .WithName("Bulk Upload Files")
            .WithSummary("Upload multiple files at once")
            .Produces<EndpointResponse<List<string>>>(200);

            group.MapGet("/", async (ELearningProject.Contracts.IFileService fileService) =>
            {
                var files = await fileService.GetAllFilesAsync();
                return Results.Ok(EndpointResponse<List<ELearningProject.Models.UploadedFile>>.SuccessResponse(files));
            })
            .WithName("List Files")
            .WithSummary("Get a list of all uploaded files")
            .Produces<EndpointResponse<List<ELearningProject.Models.UploadedFile>>>(200);

            group.MapGet("/{id:guid}", async (Guid id, ELearningProject.Contracts.IFileService fileService) =>
            {
                var file = await fileService.GetFileByIdAsync(id);
                return file != null 
                    ? Results.Ok(EndpointResponse<ELearningProject.Models.UploadedFile>.SuccessResponse(file))
                    : Results.NotFound(EndpointResponse<string>.ErrorResponse("File not found.", 404));
            })
            .WithName("Get File Details")
            .WithSummary("Get metadata for a specific file")
            .Produces<EndpointResponse<ELearningProject.Models.UploadedFile>>(200)
            .Produces<EndpointResponse<string>>(404);

            group.MapPatch("/{id:guid}", async (Guid id, [FromBody] string newFileName, ELearningProject.Contracts.IFileService fileService) =>
            {
                var success = await fileService.UpdateFileMetadataAsync(id, newFileName);
                return success
                    ? Results.Ok(EndpointResponse<string>.SuccessResponse(null, "File name updated successfully."))
                    : Results.NotFound(EndpointResponse<string>.ErrorResponse("File not found.", 404));
            })
            .WithName("Update File Name")
            .WithSummary("Change the display name of an uploaded file")
            .Produces<EndpointResponse<string>>(200)
            .Produces<EndpointResponse<string>>(404);

            group.MapGet("/download/{id:guid}", async (Guid id, ELearningProject.Contracts.IFileService fileService, IWebHostEnvironment env) =>
            {
                var fileRecord = await fileService.GetFileByIdAsync(id);
                if (fileRecord == null) return Results.NotFound();

                var webRootPath = env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                var uploadsIndex = fileRecord.FileUrl.IndexOf("Uploads", StringComparison.OrdinalIgnoreCase);
                if (uploadsIndex == -1) return Results.NotFound();

                var relativePath = fileRecord.FileUrl.Substring(uploadsIndex);
                var fullPath = Path.Combine(webRootPath, relativePath);

                if (!File.Exists(fullPath)) return Results.NotFound();

                return Results.File(fullPath, fileRecord.ContentType, fileRecord.FileName);
            })
            .WithName("Download File")
            .WithSummary("Download a file directly with its original name")
            .Produces(200, null, "application/octet-stream");

            group.MapDelete("/", async (
                [FromQuery] string fileUrl,
                ELearningProject.Contracts.IFileService fileService) =>
            {
                if (string.IsNullOrWhiteSpace(fileUrl))
                {
                    return Results.BadRequest(EndpointResponse<string>.ErrorResponse("File URL is required.", 400));
                }

                var deleted = await fileService.DeleteFileAsync(fileUrl);
                return deleted
                    ? Results.NoContent()
                    : Results.BadRequest(EndpointResponse<string>.ErrorResponse("File could not be deleted or was not found.", 400));
            })
            .WithName("Delete File")
            .WithSummary("Delete a file by its URL and remove from DB")
            .Produces(204)
            .Produces<EndpointResponse<string>>(400);
        }
    }
}
