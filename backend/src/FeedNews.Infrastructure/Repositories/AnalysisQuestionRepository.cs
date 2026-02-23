using FeedNews.Application.Common.Repositories;
using FeedNews.Domain.Entities;
using FeedNews.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace FeedNews.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for AnalysisQuestion entities
/// </summary>
public class AnalysisQuestionRepository : IAnalysisQuestionRepository
{
    private readonly FeedNewsContext _context;

    public AnalysisQuestionRepository(FeedNewsContext context)
    {
        _context = context;
    }

    public async Task<List<AnalysisQuestion>> GetByCategoryAsync(string category)
    {
        return await _context.AnalysisQuestions
            .Where(q => q.Category == category && q.IsActive)
            .OrderBy(q => q.Priority)
            .ThenByDescending(q => q.UpdatedAt)
            .ToListAsync();
    }

    public async Task<List<AnalysisQuestion>> GetActiveAsync()
    {
        return await _context.AnalysisQuestions
            .Where(q => q.IsActive)
            .OrderBy(q => q.Priority)
            .ThenByDescending(q => q.UpdatedAt)
            .ToListAsync();
    }

    public async Task<AnalysisQuestion?> GetByIdAsync(Guid id)
    {
        return await _context.AnalysisQuestions.FirstOrDefaultAsync(q => q.Id == id);
    }

    public async Task AddAsync(AnalysisQuestion question)
    {
        question.CreatedAt = DateTime.UtcNow;
        question.UpdatedAt = DateTime.UtcNow;
        await _context.AnalysisQuestions.AddAsync(question);
    }

    public async Task UpdateAsync(AnalysisQuestion question)
    {
        question.UpdatedAt = DateTime.UtcNow;
        _context.AnalysisQuestions.Update(question);
        await Task.CompletedTask;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var question = await _context.AnalysisQuestions.FirstOrDefaultAsync(q => q.Id == id);
        if (question == null)
        {
            return false;
        }

        question.IsActive = false;
        question.UpdatedAt = DateTime.UtcNow;
        _context.AnalysisQuestions.Update(question);
        return true;
    }
}
