using FeedNews.Application.Common.Repositories;
using FeedNews.Domain.Entities;
using FeedNews.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace FeedNews.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for KnowledgeDispute entities
/// </summary>
public class KnowledgeDisputeRepository : IKnowledgeDisputeRepository
{
    private readonly FeedNewsContext _context;

    public KnowledgeDisputeRepository(FeedNewsContext context)
    {
        _context = context;
    }

    public async Task AddAsync(KnowledgeDispute dispute)
    {
        dispute.CreatedAt = DateTime.UtcNow;
        await _context.KnowledgeDisputes.AddAsync(dispute);
    }

    public async Task<KnowledgeDispute?> GetByIdAsync(Guid id)
    {
        return await _context.KnowledgeDisputes.FirstOrDefaultAsync(d => d.Id == id);
    }

    public async Task<List<KnowledgeDispute>> GetByKnowledgeEntryAsync(Guid knowledgeEntryId)
    {
        return await _context.KnowledgeDisputes
            .Where(d => d.KnowledgeEntryId == knowledgeEntryId || d.ConflictingEntryId == knowledgeEntryId)
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<KnowledgeDispute>> GetActiveDisputesAsync()
    {
        return await _context.KnowledgeDisputes
            .Where(d => d.ResolvedAt == null)
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync();
    }
}
