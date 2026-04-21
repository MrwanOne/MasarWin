using Masar.Application.Common;
using Masar.Application.DTOs;
using Masar.Application.Interfaces;
using Masar.Domain.Entities;
using Masar.Domain.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace Masar.Application.Services;

public class DocumentService : IDocumentService
{
    private readonly IDocumentRepository _documents;
    private readonly ICurrentUserService _currentUser;

    public DocumentService(IDocumentRepository documents, ICurrentUserService currentUser)
    {
        _documents = documents;
        _currentUser = currentUser;
    }

    public async Task<Result<DocumentDto>> UploadAsync(int projectId, Stream content, string fileName, string? description = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var ms = new MemoryStream();
            await content.CopyToAsync(ms, cancellationToken);
            var contentBytes = ms.ToArray();

            // Calculate Checksum (SHA256)
            string checksum;
            using (var sha256 = SHA256.Create())
            {
                var hashBytes = sha256.ComputeHash(contentBytes);
                checksum = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }

            // Determine Version
            var latest = await _documents.GetLatestVersionAsync(projectId, fileName, cancellationToken);
            var version = (latest?.Version ?? 0) + 1;

            var document = new Document
            {
                ProjectId = projectId,
                FileName = fileName,
                Content = contentBytes,
                ContentType = GetContentType(fileName),
                FileSize = contentBytes.Length,
                Version = version,
                Status = "Submitted",
                Checksum = checksum,
                Description = description,
                CreatedAt = DateTime.UtcNow,
                CreatedByUserId = _currentUser.UserId ?? 0
            };

            await _documents.AddAsync(document, cancellationToken);
            return Result<DocumentDto>.Success(document.ToDto());
        }
        catch (Exception ex)
        {
            return Result<DocumentDto>.Failure($"فشل رفع الملف: {ex.Message} / File upload failed: {ex.Message}");
        }
    }

    public async Task<List<DocumentDto>> GetByProjectAsync(int projectId, CancellationToken cancellationToken = default)
    {
        var list = await _documents.GetByProjectIdAsync(projectId, cancellationToken);
        return list.Select(d => d.ToDto()).ToList();
    }

    public async Task<Result<Stream>> DownloadAsync(int documentId, CancellationToken cancellationToken = default)
    {
        var document = await _documents.GetByIdAsync(documentId, cancellationToken);
        if (document == null) return Result<Stream>.Failure("المستند غير موجود. / Document not found.");

        return Result<Stream>.Success(new MemoryStream(document.Content));
    }

    public async Task<Result> DeleteAsync(int documentId, CancellationToken cancellationToken = default)
    {
        var document = await _documents.GetByIdAsync(documentId, cancellationToken);
        if (document == null) return Result.Failure("المستند غير موجود. / Document not found.");

        await _documents.DeleteAsync(document, cancellationToken);
        return Result.Success();
    }

    public async Task<Result<DocumentDto>> ApproveAsync(int documentId, CancellationToken cancellationToken = default)
    {
        var document = await _documents.GetByIdAsync(documentId, cancellationToken);
        if (document == null) return Result<DocumentDto>.Failure("المستند غير موجود. / Document not found.");

        document.Status = "Approved";
        await _documents.UpdateAsync(document, cancellationToken);
        return Result<DocumentDto>.Success(document.ToDto());
    }

    public async Task<Result<DocumentDto>> RejectAsync(int documentId, CancellationToken cancellationToken = default)
    {
        var document = await _documents.GetByIdAsync(documentId, cancellationToken);
        if (document == null) return Result<DocumentDto>.Failure("المستند غير موجود. / Document not found.");

        document.Status = "Rejected";
        await _documents.UpdateAsync(document, cancellationToken);
        return Result<DocumentDto>.Success(document.ToDto());
    }

    private string GetContentType(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return extension switch
        {
            ".pdf" => "application/pdf",
            ".doc" => "application/msword",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".xls" => "application/vnd.ms-excel",
            ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            ".png" => "image/png",
            ".jpg" => "image/jpeg",
            ".jpeg" => "image/jpeg",
            ".ppt" => "application/vnd.ms-powerpoint",
            ".pptx" => "application/vnd.openxmlformats-officedocument.presentationml.presentation",
            _ => "application/octet-stream"
        };
    }
}
