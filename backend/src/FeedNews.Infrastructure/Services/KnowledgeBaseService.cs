using FeedNews.Application.Common.Repositories;
using FeedNews.Application.Contracts.Services;
using FeedNews.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FeedNews.Infrastructure.Services;

/// <summary>
/// Implementation of IKnowledgeBaseService.
/// Manages retrieval, creation, and maintenance of knowledge base entries.
/// </summary>
public class KnowledgeBaseService : IKnowledgeBaseService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<KnowledgeBaseService> _logger;

    public KnowledgeBaseService(IUnitOfWork unitOfWork, ILogger<KnowledgeBaseService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<List<KnowledgeEntry>> GetByCategory(string category)
    {
        try
        {
            _logger.LogDebug("Retrieving knowledge entries for category: {Category}", category);
            
            var entries = await _unitOfWork.Knowledge.GetByCategoryAsync(category);
            
            _logger.LogDebug("Retrieved {Count} knowledge entries for category: {Category}", entries.Count, category);
            return entries;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving knowledge entries for category: {Category}", category);
            throw;
        }
    }

    public async Task<List<KnowledgeEntry>> SearchByKeywords(string keywords, string? category = null)
    {
        try
        {
            _logger.LogDebug("Searching knowledge entries by keywords: {Keywords}, Category: {Category}", keywords, category ?? "All");
            
            if (string.IsNullOrWhiteSpace(keywords))
            {
                return new List<KnowledgeEntry>();
            }

            var entries = await _unitOfWork.Knowledge.SearchByKeywordsAsync(keywords, category);
            
            _logger.LogDebug("Found {Count} knowledge entries matching keywords: {Keywords}", entries.Count, keywords);
            return entries;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching knowledge entries by keywords: {Keywords}", keywords);
            throw;
        }
    }

    public async Task<List<KnowledgeEntry>> GetActive()
    {
        try
        {
            _logger.LogDebug("Retrieving all active knowledge entries");
            
            var entries = await _unitOfWork.Knowledge.GetActiveAsync();
            
            _logger.LogDebug("Retrieved {Count} active knowledge entries", entries.Count);
            return entries;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving active knowledge entries");
            throw;
        }
    }

    public async Task<KnowledgeEntry> AddEntry(KnowledgeEntry entry)
    {
        try
        {
            if (entry == null)
            {
                throw new ArgumentNullException(nameof(entry));
            }

            entry.CreatedAt = DateTime.UtcNow;
            entry.UpdatedAt = DateTime.UtcNow;
            entry.IsActive = true;

            _logger.LogInformation("Adding new knowledge entry: {Topic} ({Category})", entry.Topic, entry.Category);
            
            await _unitOfWork.Knowledge.AddAsync(entry);
            await _unitOfWork.SaveChangesAsync();
            
            _logger.LogInformation("Successfully added knowledge entry with ID: {Id}", entry.Id);
            return entry;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding knowledge entry: {Topic}", entry?.Topic);
            throw;
        }
    }

    public async Task<KnowledgeEntry> UpdateEntry(KnowledgeEntry entry)
    {
        try
        {
            if (entry == null)
            {
                throw new ArgumentNullException(nameof(entry));
            }

            var existingEntry = await _unitOfWork.Knowledge.GetByIdAsync(entry.Id);
            if (existingEntry == null)
            {
                throw new InvalidOperationException($"Knowledge entry with ID {entry.Id} not found");
            }

            entry.UpdatedAt = DateTime.UtcNow;
            entry.CreatedAt = existingEntry.CreatedAt; // Preserve original creation time

            _logger.LogInformation("Updating knowledge entry: {Id}", entry.Id);
            
            await _unitOfWork.Knowledge.UpdateAsync(entry);
            await _unitOfWork.SaveChangesAsync();
            
            _logger.LogInformation("Successfully updated knowledge entry: {Id}", entry.Id);
            return entry;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating knowledge entry: {Id}", entry?.Id);
            throw;
        }
    }

    public async Task<bool> DeleteEntry(Guid id)
    {
        try
        {
            _logger.LogInformation("Soft deleting knowledge entry: {Id}", id);
            
            var entry = await _unitOfWork.Knowledge.GetByIdAsync(id);
            if (entry == null)
            {
                _logger.LogWarning("Knowledge entry not found for deletion: {Id}", id);
                return false;
            }

            entry.IsActive = false;
            entry.UpdatedAt = DateTime.UtcNow;
            
            await _unitOfWork.Knowledge.UpdateAsync(entry);
            await _unitOfWork.SaveChangesAsync();
            
            _logger.LogInformation("Successfully soft-deleted knowledge entry: {Id}", id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting knowledge entry: {Id}", id);
            throw;
        }
    }

    public async Task<bool> MarkAsDisputed(Guid entryId, Guid conflictingEntryId, string reason)
    {
        try
        {
            _logger.LogInformation("Marking knowledge entries as disputed. Entry1: {EntryId}, Entry2: {ConflictingId}", entryId, conflictingEntryId);
            
            // Verify both entries exist
            var entry1 = await _unitOfWork.Knowledge.GetByIdAsync(entryId);
            var entry2 = await _unitOfWork.Knowledge.GetByIdAsync(conflictingEntryId);
            
            if (entry1 == null || entry2 == null)
            {
                _logger.LogWarning("One or both knowledge entries not found. Entry1: {Entry1Found}, Entry2: {Entry2Found}", entry1 != null, entry2 != null);
                return false;
            }

            var dispute = new KnowledgeDispute
            {
                Id = Guid.NewGuid(),
                KnowledgeEntryId = entryId,
                ConflictingEntryId = conflictingEntryId,
                Reason = reason,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.KnowledgeDisputes.AddAsync(dispute);
            await _unitOfWork.SaveChangesAsync();
            
            _logger.LogInformation("Successfully created dispute between entries: {EntryId} and {ConflictingId}", entryId, conflictingEntryId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking entries as disputed. Entry1: {EntryId}, Entry2: {ConflictingId}", entryId, conflictingEntryId);
            throw;
        }
    }

    public async Task<KnowledgeEntry?> GetById(Guid id)
    {
        try
        {
            return await _unitOfWork.Knowledge.GetByIdAsync(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving knowledge entry by ID: {Id}", id);
            throw;
        }
    }
}
