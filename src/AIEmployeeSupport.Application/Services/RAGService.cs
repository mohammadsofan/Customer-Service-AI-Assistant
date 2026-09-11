using AIEmployeeSupport.Application.DTOs.Support;
using AIEmployeeSupport.Application.Interfaces;
using AIEmployeeSupport.Application.Interfaces.Services;
using AIEmployeeSupport.Domain.Entities;
using AIEmployeeSupport.Domain.Enums;
using System.Diagnostics;

namespace AIEmployeeSupport.Application.Services;

public class RAGService : IRAGService
{
    private readonly IEmbeddingService _embeddingService;
    private readonly IKnowledgeEmbeddingRepository _embeddingRepository;
    private readonly IKnowledgeScenarioRepository _scenarioRepository;
    private readonly IAIFailoverService _failoverService;
    private readonly IAIConfigurationRepository _configRepository;
    private readonly ISupportQuestionRepository _questionRepository;
    private readonly IAIRequestLogRepository _requestLogRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RAGService(
        IEmbeddingService embeddingService,
        IKnowledgeEmbeddingRepository embeddingRepository,
        IKnowledgeScenarioRepository scenarioRepository,
        IAIFailoverService failoverService,
        IAIConfigurationRepository configRepository,
        ISupportQuestionRepository questionRepository,
        IAIRequestLogRepository requestLogRepository,
        IUnitOfWork unitOfWork)
    {
        _embeddingService = embeddingService;
        _embeddingRepository = embeddingRepository;
        _scenarioRepository = scenarioRepository;
        _failoverService = failoverService;
        _configRepository = configRepository;
        _questionRepository = questionRepository;
        _requestLogRepository = requestLogRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<QuestionResponse> ProcessQuestionAsync(string questionText, Guid employeeId, CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();

        // Save initial question
        var question = new SupportQuestion
        {
            Id = Guid.NewGuid(),
            EmployeeId = employeeId,
            QuestionText = questionText,
            Status = QuestionStatus.New,
            CreatedAt = DateTime.UtcNow
        };
        await _questionRepository.CreateAsync(question, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var config = await _configRepository.GetAsync(cancellationToken);
        if (config == null) throw new InvalidOperationException("AI configuration not found.");

        var questionVector = await _embeddingService.GenerateEmbeddingAsync(questionText, cancellationToken);
        var questionVectorBytes = questionVector.SelectMany(BitConverter.GetBytes).ToArray();
        var similarDocs = await _embeddingRepository.SearchSimilarAsync(questionVectorBytes, config.TopK, config.SimilarityThreshold, cancellationToken);

        if (!similarDocs.Any())
        {
            return await EscalateQuestion(question, null, stopwatch.ElapsedMilliseconds, cancellationToken);
        }

        var topResult = similarDocs.First();
        var topScenario = await _scenarioRepository.GetByIdAsync(topResult.Embedding.ScenarioId, cancellationToken);
        
        var retrievedKnowledge = similarDocs.Select(x => $"Scenario {x.Embedding.ScenarioId}: ...").ToList(); // Simplified retrieval

        var aiRequest = new AIRequest
        {
            QuestionText = questionText,
            RetrievedKnowledge = retrievedKnowledge,
            SystemPrompt = config.SystemPrompt,
            Temperature = config.Temperature,
            MaxTokens = config.MaxTokens,
            ModelName = config.ActiveModel?.ModelName ?? "default"
        };

        AIResponse aiResponse = null!;
        try
        {
            aiResponse = await _failoverService.GenerateAnswerWithFailoverAsync(aiRequest, cancellationToken);
        }
        catch (Exception)
        {
            return await EscalateQuestion(question, null, stopwatch.ElapsedMilliseconds, cancellationToken);
        }

        var log = new AIRequestLog
        {
            Id = Guid.NewGuid(),
            QuestionId = question.Id,
            ProviderId = config.ActiveProviderId,
            ModelId = config.ActiveModelId,
            DurationMs = stopwatch.ElapsedMilliseconds,
            Success = aiResponse.Answered,
            CreatedAt = DateTime.UtcNow,
            IsFailover = false // Could be evaluated properly if tracked in AIFailoverService
        };
        await _requestLogRepository.CreateAsync(log, cancellationToken);

        if (!aiResponse.Answered)
        {
            return await EscalateQuestion(question, topScenario?.Name, stopwatch.ElapsedMilliseconds, cancellationToken);
        }

        question.AnswerText = aiResponse.Summary ?? aiResponse.Reason;
        question.Status = QuestionStatus.Answered;
        question.AnsweredByAI = true;
        question.CompletedAt = DateTime.UtcNow;
        question.ProcessingTimeMs = stopwatch.ElapsedMilliseconds;
        question.ScenarioId = topScenario?.Id;
        question.ConfidenceScore = topResult.Similarity;

        await _questionRepository.UpdateAsync(question, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new QuestionResponse
        {
            Id = question.Id,
            Status = question.Status.ToString(),
            Answered = true,
            Answer = question.AnswerText,
            Steps = aiResponse.Steps,
            ConfidenceScore = topResult.Similarity,
            SourceScenario = topScenario?.Name,
            Escalated = false
        };
    }

    private async Task<QuestionResponse> EscalateQuestion(SupportQuestion question, string? sourceScenario, long duration, CancellationToken cancellationToken)
    {
        question.Status = QuestionStatus.NoAnswer;
        question.Escalated = true;
        question.CompletedAt = DateTime.UtcNow;
        question.ProcessingTimeMs = duration;
        question.AnswerText = "لا أملك إجابة بخصوص هذا الموضوع. يرجى التواصل مع Back Office للحصول على مزيد من المساعدة بخصوص هذه المشكلة.";

        await _questionRepository.UpdateAsync(question, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new QuestionResponse
        {
            Id = question.Id,
            Status = question.Status.ToString(),
            Answered = false,
            Answer = question.AnswerText,
            SourceScenario = sourceScenario,
            Escalated = true
        };
    }
}
