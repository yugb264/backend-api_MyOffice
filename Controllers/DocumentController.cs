using backend_api.Data;
using backend_api.DTOs.Common;
using backend_api.DTOs.Document;
using backend_api.DTOs.Requests;
using backend_api.DTOs.Version;
using backend_api.Helpers;
using backend_api.Inerfaces.Services;
using backend_api.Interfaces.Repositories;
using backend_api.Interfaces.Services;
using backend_api.Models;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace backend_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class DocumentController : ControllerBase
    {
        
        private readonly string _storagePath;
        private readonly TokenService _tokenService;
        private readonly FileStorageService _fileStorage;
        private readonly IDocumentService _documentService;
        private readonly IOnlyOfficeService _onlyOfficeService;
        private readonly IDocumentDiffService _diffService;

        // FluentValidation validators
        private readonly IValidator<UploadDocumentRequest> _uploadValidator;
        private readonly IValidator<DocumentIdRequest> _documentIdValidator;
        private readonly IValidator<DocumentConfigRequest> _documentConfigValidator;
        private readonly IValidator<CallbackRequest> _callbackValidator;
        private readonly IValidator<DiffConfigRequest> _diffConfigValidator;
        private readonly IValidator<DeleteVersionRequest> _deleteVersionValidator;
        private readonly IValidator<CompareVersionsRequest> _compareValidator;

        public DocumentController(
     
         TokenService tokenService,
         FileStorageService fileStorage,
         IDocumentService documentService,
         IOnlyOfficeService onlyOfficeService,
         IDocumentDiffService diffService,
         IValidator<UploadDocumentRequest> uploadValidator,
         IValidator<DocumentIdRequest> documentIdValidator,
         IValidator<DocumentConfigRequest> documentConfigValidator,
         IValidator<CallbackRequest> callbackValidator,
         IValidator<DiffConfigRequest> diffConfigValidator,
         IValidator<DeleteVersionRequest> deleteVersionValidator,
         IValidator<CompareVersionsRequest> compareValidator
         )
        {
            
            _tokenService = tokenService;
            _fileStorage = fileStorage;
            _documentService = documentService;
            _onlyOfficeService = onlyOfficeService;
            _diffService = diffService;

            _uploadValidator = uploadValidator;
            _documentIdValidator = documentIdValidator;
            _documentConfigValidator = documentConfigValidator;
            _callbackValidator = callbackValidator;
            _diffConfigValidator = diffConfigValidator;
            _deleteVersionValidator = deleteVersionValidator;
            _compareValidator = compareValidator;

            _storagePath = Path.Combine(Directory.GetCurrentDirectory(), "document-storage");
            if (!Directory.Exists(_storagePath))
            {
                Directory.CreateDirectory(_storagePath);
            }

          ;
        }

   

        [HttpPost("upload")]
        
        public async Task<IActionResult> Upload(IFormFile file)
        {
            // Validation
            var request = new UploadDocumentRequest { File = file };
            var validation = await _uploadValidator.ValidateAsync(request);
            var errorResponse = ValidationHelper.ToErrorResponse(validation);
            if (errorResponse != null) return errorResponse;

            try
            {
                var (name, _, _) = _tokenService.GetUser(HttpContext);

                var result = await _documentService.UploadAsync(file, name);
                if (result.IsDuplicate)
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = result.Message,
                        Data = new
                        {
                            fileName = result.FileName
                        }
                    });
                }
                return Ok(ApiResponse<UploadResponseDto>.SuccessResponse(
    result,
    result.Message
));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{id}/config")]
        public async Task<IActionResult> GetDocumentConfig(int id, [FromQuery] int? versionId)
        {
            // Validation
            var request = new DocumentConfigRequest { Id = id, VersionId = versionId };
            var validation = await _documentConfigValidator.ValidateAsync(request);
            var errorResponse = ValidationHelper.ToErrorResponse(validation);
            if (errorResponse != null) return errorResponse;

            try
            {
                var (name, email, oid) = _tokenService.GetUser(HttpContext);

                var config = _onlyOfficeService.GetConfig(id, versionId, name, email, oid);

                return Ok(config);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("callback/{id}")]
        public async Task<IActionResult> Callback(int id, [FromBody] JObject data)
        {
            try
            {
                var userName = HttpContext.Request.Query["user"].FirstOrDefault() ?? "Unknown User";
                var versionIdStr = HttpContext.Request.Query["versionId"].FirstOrDefault();
                int? versionId = string.IsNullOrEmpty(versionIdStr) ? null : int.Parse(versionIdStr);

                // Validation
                var request = new CallbackRequest
                {
                    DocumentId = id,
                    Data = data,
                    UserName = userName,
                    VersionId = versionId
                };
                var validation = await _callbackValidator.ValidateAsync(request);
                var errorResponse = ValidationHelper.ToErrorResponse(validation);
                if (errorResponse != null) return errorResponse;

                var result = await _onlyOfficeService.HandleCallback(id, data, userName, versionId);

                return new JsonResult(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Callback error: " + ex.Message);
                return new JsonResult(new { error = 1 });
            }
        }

        [HttpGet("files/{filename}")]
        public async Task<IActionResult> Download(string filename)
        {
            // Validation - reuse DiffConfigValidator for filename safety checks
            var request = new DiffConfigRequest { FileName = filename };
            var validation = await _diffConfigValidator.ValidateAsync(request);
            var errorResponse = ValidationHelper.ToErrorResponse(validation);
            if (errorResponse != null) return errorResponse;

            Console.WriteLine("DOWNLOAD API HIT  " + filename);
            var bytes = _fileStorage.GetFile(filename);

            if (bytes == null)
            
                throw new FileNotFoundException("File not found");
            
            Console.WriteLine(" FILE SERVED SUCCESSFULLY");
            var extension = Path.GetExtension(filename).ToLower();

            var contentType = extension switch
            {
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".doc" => "application/msword",
                _ => "application/octet-stream"
            };

            return File(bytes, contentType, filename);
        }

        [HttpGet("{id}/versions")]
        public async Task<IActionResult> GetVersions(int id)
        {
            // Validation
            var request = new DocumentIdRequest { Id = id };
            var validation = await _documentIdValidator.ValidateAsync(request);
            var errorResponse = ValidationHelper.ToErrorResponse(validation);
            if (errorResponse != null) return errorResponse;

            var result = await _documentService.GetVersionsAsync(id);
            return Ok(ApiResponse<List<VersionDto>>.SuccessResponse(
      result,
      "Versions fetched successfully"
  ));
        }





        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _documentService.GetAllAsync();
            return Ok(ApiResponse<List<DocumentDto>>.SuccessResponse(
    result,
    "Documents fetched successfully"
));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDocument(int id)
        {
            // Validation
            var request = new DocumentIdRequest { Id = id };
            var validation = await _documentIdValidator.ValidateAsync(request);
            var errorResponse = ValidationHelper.ToErrorResponse(validation);
            if (errorResponse != null) return errorResponse;

            try
            {
                await _documentService.DeleteDocumentAsync(id);
                return Ok(ApiResponse<string>.SuccessResponse(
    null,
    "Document deleted successfully"
));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpDelete("{documentId}/versions/{versionId}")]
        public async Task<IActionResult> DeleteVersion(int documentId, int versionId)
        {
            // Validation
            var request = new DeleteVersionRequest { DocumentId = documentId, VersionId = versionId };
            var validation = await _deleteVersionValidator.ValidateAsync(request);
            var errorResponse = ValidationHelper.ToErrorResponse(validation);
            if (errorResponse != null) return errorResponse;

            try
            {
                await _documentService.DeleteVersionAsync(documentId, versionId);
                return Ok(ApiResponse<string>.SuccessResponse(
    null,
    "Version deleted successfully"
));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            // Validation
            var request = new DocumentIdRequest { Id = id };
            var validation = await _documentIdValidator.ValidateAsync(request);
            var errorResponse = ValidationHelper.ToErrorResponse(validation);
            if (errorResponse != null) return errorResponse;

            var result = await _documentService.GetDocumentByIdAsync(id);
            return Ok(ApiResponse<DocumentDetailDto>.SuccessResponse(
    result,
    "Document fetched successfully"
));
        }

        [HttpGet("{documentId}/compare")]
        public async Task<IActionResult> Compare(int documentId, int v1, int v2)
        {
            // Validation
            var request = new CompareVersionsRequest { DocumentId = documentId, V1 = v1, V2 = v2 };
            var validation = await _compareValidator.ValidateAsync(request);
            var errorResponse = ValidationHelper.ToErrorResponse(validation);
            if (errorResponse != null) return errorResponse;

            try
            {
                var fileName = await _diffService.GenerateDiffAsync(documentId, v1, v2);

                return Ok(new
                {
                    fileName
                });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("diff-config")]
        public async Task<IActionResult> GetDiffConfig(string fileName)
        {
            // Validation
            var request = new DiffConfigRequest { FileName = fileName };
            var validation = await _diffConfigValidator.ValidateAsync(request);
            var errorResponse = ValidationHelper.ToErrorResponse(validation);
            if (errorResponse != null) return errorResponse;

            var fileUrl = $"http://host.docker.internal:5000/api/document/files/{fileName}";


            var config = new
            {
                document = new
                {
                    fileType = "docx",
                    key = Guid.NewGuid().ToString(),
                    title = "Comparison Result",
                    url = fileUrl
                },
                documentType = "word",
                editorConfig = new
                {
                    mode = "view" // 🔥 IMPORTANT: read-only
                }
            };

            return Ok(config);
        }




    }
}
