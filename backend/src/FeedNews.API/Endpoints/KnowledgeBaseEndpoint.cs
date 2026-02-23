using Asp.Versioning;
using Asp.Versioning.Builder;
using Carter;
using FeedNews.Application.Common.Repositories;
using FeedNews.Application.Contracts.Services;
using FeedNews.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FeedNews.API.Endpoints;

public class KnowledgeBaseEndpoint : BaseEndpoint, ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        ApiVersion currentVersion = new(1, 0);
        ApiVersionSet apiVersionSet = app
            .NewApiVersionSet()
            .HasApiVersion(currentVersion)
            .ReportApiVersions()
            .Build();

        RouteGroupBuilder builderGroup = app.MapGroup("api/v{apiVersion:apiVersion}/knowledge")
            .WithApiVersionSet(apiVersionSet);

        // Knowledge Entry Endpoints
        builderGroup.MapPost("add", AddKnowledgeEntry)
            .WithName("AddKnowledgeEntry")
            .WithDescription("Add a new knowledge entry")
            .WithOpenApi();

        builderGroup.MapGet("category/{category}", GetByCategory)
            .WithName("GetKnowledgeByCategory")
            .WithDescription("Get all active knowledge entries by category")
            .WithOpenApi();

        builderGroup.MapGet("search", SearchKnowledge)
            .WithName("SearchKnowledge")
            .WithDescription("Search knowledge entries by keywords and optional category")
            .WithOpenApi();

        builderGroup.MapPut("{id}", UpdateKnowledgeEntry)
            .WithName("UpdateKnowledgeEntry")
            .WithDescription("Update an existing knowledge entry")
            .WithOpenApi();

        builderGroup.MapDelete("{id}", DeleteKnowledgeEntry)
            .WithName("DeleteKnowledgeEntry")
            .WithDescription("Soft delete a knowledge entry")
            .WithOpenApi();

        builderGroup.MapPost("mark-dispute", MarkKnowledgeDispute)
            .WithName("MarkKnowledgeDispute")
            .WithDescription("Mark two knowledge entries as disputed/conflicting")
            .WithOpenApi();

        // Analysis Questions Endpoints
        builderGroup.MapPost("questions/add", AddAnalysisQuestion)
            .WithName("AddAnalysisQuestion")
            .WithDescription("Add a new analysis question for a category")
            .WithOpenApi();

        builderGroup.MapGet("questions/{category}", GetQuestionsByCategory)
            .WithName("GetQuestionsByCategory")
            .WithDescription("Get all active analysis questions for a category")
            .WithOpenApi();
    }

    private static async Task<IResult> AddKnowledgeEntry(
        AddKnowledgeEntryRequest request,
        IKnowledgeBaseService knowledgeService,
        CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Category) || 
                string.IsNullOrWhiteSpace(request.Topic) || 
                string.IsNullOrWhiteSpace(request.Description))
            {
                return TypedResults.BadRequest(new { message = "Category, Topic, and Description are required" });
            }

            if (request.ConfidenceScore < 0 || request.ConfidenceScore > 1)
            {
                return TypedResults.BadRequest(new { message = "ConfidenceScore must be between 0 and 1" });
            }

            var entry = new KnowledgeEntry
            {
                Id = Guid.NewGuid(),
                Category = request.Category,
                Topic = request.Topic,
                Description = request.Description,
                SourceUrl = request.SourceUrl ?? string.Empty,
                ConfidenceScore = request.ConfidenceScore,
                Tags = request.Tags ?? Array.Empty<string>(),
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var createdEntry = await knowledgeService.AddEntry(entry);
            return TypedResults.Created($"/api/v1/knowledge/{createdEntry.Id}", createdEntry);
        }
        catch (Exception ex)
        {
            return TypedResults.InternalServerError(new { message = "Error adding knowledge entry", error = ex.Message });
        }
    }

    private static async Task<IResult> GetByCategory(
        string category,
        IKnowledgeBaseService knowledgeService,
        CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(category))
            {
                return TypedResults.BadRequest(new { message = "Category is required" });
            }

            var entries = await knowledgeService.GetByCategory(category);
            return TypedResults.Ok(new { 
                category = category,
                count = entries.Count,
                entries = entries
            });
        }
        catch (Exception ex)
        {
            return TypedResults.InternalServerError(new { message = "Error retrieving knowledge entries", error = ex.Message });
        }
    }

    private static async Task<IResult> SearchKnowledge(
        [FromQuery] string keywords,
        [FromQuery] string? category,
        IKnowledgeBaseService knowledgeService,
        CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(keywords))
            {
                return TypedResults.BadRequest(new { message = "Keywords parameter is required" });
            }

            var results = await knowledgeService.SearchByKeywords(keywords, category);
            return TypedResults.Ok(new {
                query = keywords,
                category = category ?? "all",
                count = results.Count,
                results = results
            });
        }
        catch (Exception ex)
        {
            return TypedResults.InternalServerError(new { message = "Error searching knowledge entries", error = ex.Message });
        }
    }

    private static async Task<IResult> UpdateKnowledgeEntry(
        Guid id,
        UpdateKnowledgeEntryRequest request,
        IKnowledgeBaseService knowledgeService,
        CancellationToken cancellationToken)
    {
        try
        {
            var existing = await knowledgeService.GetById(id);
            if (existing == null)
            {
                return TypedResults.NotFound(new { message = "Knowledge entry not found" });
            }

            if (request.ConfidenceScore < 0 || request.ConfidenceScore > 1)
            {
                return TypedResults.BadRequest(new { message = "ConfidenceScore must be between 0 and 1" });
            }

            existing.Topic = request.Topic ?? existing.Topic;
            existing.Description = request.Description ?? existing.Description;
            existing.SourceUrl = request.SourceUrl ?? existing.SourceUrl;
            existing.ConfidenceScore = request.ConfidenceScore ?? existing.ConfidenceScore;
            if (request.Tags != null)
            {
                existing.Tags = request.Tags;
            }
            existing.UpdatedAt = DateTime.UtcNow;

            var updated = await knowledgeService.UpdateEntry(existing);
            return TypedResults.Ok(updated);
        }
        catch (Exception ex)
        {
            return TypedResults.InternalServerError(new { message = "Error updating knowledge entry", error = ex.Message });
        }
    }

    private static async Task<IResult> DeleteKnowledgeEntry(
        Guid id,
        IKnowledgeBaseService knowledgeService,
        CancellationToken cancellationToken)
    {
        try
        {
            var success = await knowledgeService.DeleteEntry(id);
            if (!success)
            {
                return TypedResults.NotFound(new { message = "Knowledge entry not found" });
            }

            return TypedResults.NoContent();
        }
        catch (Exception ex)
        {
            return TypedResults.InternalServerError(new { message = "Error deleting knowledge entry", error = ex.Message });
        }
    }

    private static async Task<IResult> MarkKnowledgeDispute(
        MarkDisputeRequest request,
        IKnowledgeBaseService knowledgeService,
        CancellationToken cancellationToken)
    {
        try
        {
            if (request.EntryId == Guid.Empty || request.ConflictingEntryId == Guid.Empty)
            {
                return TypedResults.BadRequest(new { message = "Both EntryId and ConflictingEntryId are required" });
            }

            if (request.EntryId == request.ConflictingEntryId)
            {
                return TypedResults.BadRequest(new { message = "Cannot dispute entry against itself" });
            }

            var success = await knowledgeService.MarkAsDisputed(
                request.EntryId,
                request.ConflictingEntryId,
                request.Reason ?? "No reason provided");

            if (!success)
            {
                return TypedResults.BadRequest(new { message = "Failed to mark dispute - one or both entries may not exist" });
            }

            return TypedResults.Ok(new { message = "Dispute marked successfully" });
        }
        catch (Exception ex)
        {
            return TypedResults.InternalServerError(new { message = "Error marking dispute", error = ex.Message });
        }
    }

    private static async Task<IResult> AddAnalysisQuestion(
        AddAnalysisQuestionRequest request,
        IAnalysisQuestionRepository questionRepository,
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Category) || 
                string.IsNullOrWhiteSpace(request.Question))
            {
                return TypedResults.BadRequest(new { message = "Category and Question are required" });
            }

            if (request.Priority < 1 || request.Priority > 3)
            {
                return TypedResults.BadRequest(new { message = "Priority must be 1 (high), 2 (medium), or 3 (low)" });
            }

            var question = new AnalysisQuestion
            {
                Id = Guid.NewGuid(),
                Category = request.Category,
                Question = request.Question,
                Purpose = request.Purpose ?? string.Empty,
                Priority = request.Priority,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await questionRepository.AddAsync(question);
            await unitOfWork.SaveChangesAsync();

            return TypedResults.Created($"/api/v1/knowledge/questions/{question.Id}", question);
        }
        catch (Exception ex)
        {
            return TypedResults.InternalServerError(new { message = "Error adding analysis question", error = ex.Message });
        }
    }

    private static async Task<IResult> GetQuestionsByCategory(
        string category,
        IAnalysisEnhancementService analysisService,
        CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(category))
            {
                return TypedResults.BadRequest(new { message = "Category is required" });
            }

            var questions = await analysisService.GetQuestionsForCategory(category);
            return TypedResults.Ok(new {
                category = category,
                count = questions.Count,
                questions = questions
            });
        }
        catch (Exception ex)
        {
            return TypedResults.InternalServerError(new { message = "Error retrieving analysis questions", error = ex.Message });
        }
    }
}

// Request DTOs
public record AddKnowledgeEntryRequest(
    string Category,
    string Topic,
    string Description,
    string? SourceUrl,
    decimal ConfidenceScore,
    string[]? Tags
);

public record UpdateKnowledgeEntryRequest(
    string? Topic,
    string? Description,
    string? SourceUrl,
    decimal? ConfidenceScore,
    string[]? Tags
);

public record MarkDisputeRequest(
    Guid EntryId,
    Guid ConflictingEntryId,
    string? Reason
);

public record AddAnalysisQuestionRequest(
    string Category,
    string Question,
    string? Purpose,
    int Priority
);
