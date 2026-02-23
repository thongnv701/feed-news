using FeedNews.Domain.Entities;

namespace FeedNews.Application.Common.Repositories;

/// <summary>
/// Repository interface for AnalysisQuestion entities
/// </summary>
public interface IAnalysisQuestionRepository
{
    /// <summary>
    /// Get all active analysis questions for a specific category
    /// </summary>
    Task<List<AnalysisQuestion>> GetByCategoryAsync(string category);

    /// <summary>
    /// Get all active analysis questions ordered by priority
    /// </summary>
    Task<List<AnalysisQuestion>> GetActiveAsync();

    /// <summary>
    /// Get analysis question by ID
    /// </summary>
    Task<AnalysisQuestion?> GetByIdAsync(Guid id);

    /// <summary>
    /// Add a question
    /// </summary>
    Task AddAsync(AnalysisQuestion question);

    /// <summary>
    /// Update a question
    /// </summary>
    Task UpdateAsync(AnalysisQuestion question);

    /// <summary>
    /// Soft delete a question (marks as inactive)
    /// </summary>
    Task<bool> DeleteAsync(Guid id);
}
