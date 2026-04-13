using Jaffar_Mall_Rent_Management_System.Models;
using Jaffar_Mall_Rent_Management_System.Repositories;
using Microsoft.AspNetCore.Http;

namespace Jaffar_Mall_Rent_Management_System.Services
{
    public class DocumentServices
    {
        private readonly DocumentRepository _documentRepository;
        private readonly IWebHostEnvironment _env;

        public DocumentServices(DocumentRepository documentRepository, IWebHostEnvironment env)
        {
            _documentRepository = documentRepository;
            _env = env;
        }

        public async Task<BackendResponse<bool>> AddDocumentAsync(string description, long tenantId, IFormFile file)
        {
            try
            {
                await _documentRepository.CreateTableIfNotExistsAsync();

                if (file == null || file.Length == 0)
                {
                    return BackendResponse<bool>.Failure("Please select a valid file to upload.", 400);
                }

                // Ensure the uploads directory exists
                string uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "receipts");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                // Generate unique filename to avoid overriding
                string uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }

                var document = new Document
                {
                    Description = description,
                    TenantId = tenantId,
                    FilePath = "/uploads/receipts/" + uniqueFileName,
                    UploadedAt = DateTime.UtcNow
                };

                bool isAdded = await _documentRepository.AddDocumentAsync(document);
                if (isAdded)
                {
                    return BackendResponse<bool>.Success(true, "Document uploaded successfully.");
                }
                
                // Cleanup file if DB insertion failed
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }

                return BackendResponse<bool>.Failure("Failed to add document to database.", 500);
            }
            catch (Exception ex)
            {
                return BackendResponse<bool>.Failure("An error occurred: " + ex.Message, 500);
            }
        }

        public async Task<BackendResponse<IEnumerable<Document>>> GetAllDocumentsAsync()
        {
            try
            {
                await _documentRepository.CreateTableIfNotExistsAsync();
                var docs = await _documentRepository.GetAllDocumentsAsync();
                return BackendResponse<IEnumerable<Document>>.Success(docs, "Documents retrieved successfully");
            }
            catch (Exception ex)
            {
                return BackendResponse<IEnumerable<Document>>.Failure("An error occurred: " + ex.Message, 500);
            }
        }

        public async Task<BackendResponse<bool>> DeleteDocumentAsync(long id)
        {
            try
            {
                var document = await _documentRepository.GetDocumentByIdAsync(id);
                if (document == null)
                {
                    return BackendResponse<bool>.Failure("Document not found.", 404);
                }

                // Delete file from disk
                string webRootPath = _env.WebRootPath;
                string fullPath = Path.Combine(webRootPath, document.FilePath.TrimStart('/'));
                
                if (System.IO.File.Exists(fullPath))
                {
                    System.IO.File.Delete(fullPath);
                }

                bool isDeleted = await _documentRepository.DeleteDocumentAsync(id);
                if (isDeleted)
                {
                    return BackendResponse<bool>.Success(true, "Document deleted correctly.");
                }

                return BackendResponse<bool>.Failure("Failed to delete document from database.", 500);
            }
            catch (Exception ex)
            {
                return BackendResponse<bool>.Failure("An error occurred: " + ex.Message, 500);
            }
        }
    }
}
