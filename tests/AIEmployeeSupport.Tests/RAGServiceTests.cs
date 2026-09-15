using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AIEmployeeSupport.Application.DTOs.Support;
using AIEmployeeSupport.Application.Interfaces;
using AIEmployeeSupport.Application.Interfaces.Services;
using AIEmployeeSupport.Application.Services;
using AIEmployeeSupport.Domain.Entities;
using AIEmployeeSupport.Domain.Enums;
using FluentAssertions;
using Moq;
using Xunit;

namespace AIEmployeeSupport.Tests;

public class RAGServiceTests
{
    private readonly Mock<IEmbeddingService> _embeddingService = new();
    private readonly Mock<IKnowledgeEmbeddingRepository> _embeddingRepository = new();
    private readonly Mock<IKnowledgeScenarioRepository> _scenarioRepository = new();
    private readonly Mock<IAIFailoverService> _failoverService = new();
    private readonly Mock<IAIConfigurationRepository> _configRepository = new();
    private readonly Mock<ISupportQuestionRepository> _questionRepository = new();
    private readonly Mock<IAIRequestLogRepository> _requestLogRepository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly RAGService _service;

    public RAGServiceTests()
    {
        _service = new RAGService(
            _embeddingService.Object,
            _embeddingRepository.Object,
            _scenarioRepository.Object,
            _failoverService.Object,
            _configRepository.Object,
            _questionRepository.Object,
            _requestLogRepository.Object,
            _unitOfWork.Object
        );
    }

    [Fact]
    public async Task ProcessQuestionAsync_NoSimilarDocs_EscalatesQuestion()
    {
        var employeeId = Guid.NewGuid();
        var config = new AIConfiguration { TopK = 3, SimilarityThreshold = 0.8 };
        _configRepository.Setup(x => x.GetAsync(It.IsAny<CancellationToken>())).ReturnsAsync(config);
        _embeddingService.Setup(x => x.GenerateEmbeddingAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(new float[] { 0.1f, 0.2f });
        _embeddingRepository.Setup(x => x.SearchSimilarAsync(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<double>(), It.IsAny<string?>(), It.IsAny<CancellationToken>())).ReturnsAsync(new List<(KnowledgeEmbedding, double)>());

        var result = await _service.ProcessQuestionAsync("help", employeeId);

        result.Escalated.Should().BeTrue();
        result.Status.Should().Be(QuestionStatus.NoAnswer.ToString());
        result.Answer.Should().Contain("Back Office");

        _questionRepository.Verify(x => x.UpdateAsync(It.Is<SupportQuestion>(q => q.Status == QuestionStatus.NoAnswer && q.Escalated == true), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Fact]
    public async Task ProcessQuestionAsync_BasicFlow_Success()
    {
        var employeeId = Guid.NewGuid();
        var scenarioId = Guid.NewGuid();
        var config = new AIConfiguration { TopK = 3, SimilarityThreshold = 0.8, ActiveModel = new AIModel { ModelName = "test" } };
        _configRepository.Setup(x => x.GetAsync(It.IsAny<CancellationToken>())).ReturnsAsync(config);
        _embeddingService.Setup(x => x.GenerateEmbeddingAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(new float[] { 0.1f, 0.2f });
        
        var searchResult = (Embedding: new KnowledgeEmbedding { ScenarioId = scenarioId }, Similarity: 0.9);
        _embeddingRepository.Setup(x => x.SearchSimilarAsync(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<double>(), It.IsAny<string?>(), It.IsAny<CancellationToken>())).ReturnsAsync(new List<(KnowledgeEmbedding, double)> { searchResult });

        var scenario = new KnowledgeScenario { Id = scenarioId, Name = "Test Scenario", Description = "Test", ResolutionSteps = new List<ResolutionStep>() };
        _scenarioRepository.Setup(x => x.GetByIdAsync(scenarioId, It.IsAny<CancellationToken>())).ReturnsAsync(scenario);

        var aiResponse = new AIResponse { Answered = true, Summary = "Resolved", Reason = "OK", Steps = new List<string>() };
        _failoverService.Setup(x => x.GenerateAnswerWithFailoverAsync(It.IsAny<AIRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(aiResponse);

        var result = await _service.ProcessQuestionAsync("help", employeeId);

        result.Escalated.Should().BeFalse();
        result.Answered.Should().BeTrue();
        result.Answer.Should().Be("Resolved");
        result.Status.Should().Be(QuestionStatus.Answered.ToString());

        _questionRepository.Verify(x => x.UpdateAsync(It.Is<SupportQuestion>(q => q.Status == QuestionStatus.Answered && q.AnsweredByAI == true), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ProcessQuestionAsync_InitialMatch_DoesNotInvokeQueryRewriter()
    {
        var employeeId = Guid.NewGuid();
        var scenarioId = Guid.NewGuid();
        var config = new AIConfiguration { TopK = 3, SimilarityThreshold = 0.25, ActiveModel = new AIModel { ModelName = "test-model" } };
        _configRepository.Setup(x => x.GetAsync(It.IsAny<CancellationToken>())).ReturnsAsync(config);
        _embeddingService.Setup(x => x.GenerateEmbeddingAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(new float[] { 0.1f, 0.2f });

        var searchResult = (Embedding: new KnowledgeEmbedding { ScenarioId = scenarioId }, Similarity: 0.85);
        _embeddingRepository.Setup(x => x.SearchSimilarAsync(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<double>(), "direct query", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<(KnowledgeEmbedding, double)> { searchResult });

        var scenario = new KnowledgeScenario { Id = scenarioId, Name = "Known Scenario", Description = "Desc", ResolutionSteps = new List<ResolutionStep>() };
        _scenarioRepository.Setup(x => x.GetByIdAsync(scenarioId, It.IsAny<CancellationToken>())).ReturnsAsync(scenario);

        var aiResponse = new AIResponse { Answered = true, Summary = "Direct answer", Reason = "OK" };
        _failoverService.Setup(x => x.GenerateAnswerWithFailoverAsync(It.IsAny<AIRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(aiResponse);

        var result = await _service.ProcessQuestionAsync("direct query", employeeId);

        result.Answered.Should().BeTrue();
        result.Escalated.Should().BeFalse();

        // Query rewriter must NEVER be called when initial search succeeds
        _failoverService.Verify(x => x.RewriteQueryWithFailoverAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ProcessQuestionAsync_NoInitialMatch_RewriterRescuesQuery()
    {
        var employeeId = Guid.NewGuid();
        var scenarioId = Guid.NewGuid();
        var originalQuestion = "المشترك بده يعرف قديش فاتورته عالجوال كيف افحصله؟";
        var rewrittenQuery = "فحص فواتير الجوال";

        var config = new AIConfiguration { TopK = 3, SimilarityThreshold = 0.25, ActiveModel = new AIModel { ModelName = "test-model" } };
        _configRepository.Setup(x => x.GetAsync(It.IsAny<CancellationToken>())).ReturnsAsync(config);

        var vector1 = new float[] { 0.1f, 0.2f };
        var vector2 = new float[] { 0.5f, 0.6f };
        _embeddingService.Setup(x => x.GenerateEmbeddingAsync(originalQuestion, It.IsAny<CancellationToken>())).ReturnsAsync(vector1);
        _embeddingService.Setup(x => x.GenerateEmbeddingAsync(rewrittenQuery, It.IsAny<CancellationToken>())).ReturnsAsync(vector2);

        // First search returns empty (no matches)
        _embeddingRepository.Setup(x => x.SearchSimilarAsync(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<double>(), originalQuestion, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<(KnowledgeEmbedding, double)>());

        // Rewriter returns standard Arabic query
        _failoverService.Setup(x => x.RewriteQueryWithFailoverAsync(originalQuestion, It.IsAny<CancellationToken>()))
            .ReturnsAsync(rewrittenQuery);

        // Second search with rewritten query matches scenario
        var searchResult = (Embedding: new KnowledgeEmbedding { ScenarioId = scenarioId }, Similarity: 0.65);
        _embeddingRepository.Setup(x => x.SearchSimilarAsync(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<double>(), rewrittenQuery, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<(KnowledgeEmbedding, double)> { searchResult });

        var scenario = new KnowledgeScenario { Id = scenarioId, Name = "فحص فواتير الجوال", Description = "تفاصيل الفواتير", ResolutionSteps = new List<ResolutionStep>() };
        _scenarioRepository.Setup(x => x.GetByIdAsync(scenarioId, It.IsAny<CancellationToken>())).ReturnsAsync(scenario);

        var aiResponse = new AIResponse { Answered = true, Summary = "يمكنك فحص الفاتورة عبر النظام", Reason = "OK" };
        _failoverService.Setup(x => x.GenerateAnswerWithFailoverAsync(It.IsAny<AIRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(aiResponse);

        var result = await _service.ProcessQuestionAsync(originalQuestion, employeeId);

        result.Answered.Should().BeTrue();
        result.Escalated.Should().BeFalse();
        result.SourceScenario.Should().Be("فحص فواتير الجوال");

        // Rewriter called exactly once
        _failoverService.Verify(x => x.RewriteQueryWithFailoverAsync(originalQuestion, It.IsAny<CancellationToken>()), Times.Once);

        // Final answer generation prompt MUST receive the ORIGINAL question, not the rewritten query
        _failoverService.Verify(x => x.GenerateAnswerWithFailoverAsync(
            It.Is<AIRequest>(r => r.QuestionText.Contains(originalQuestion)),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ProcessQuestionAsync_RewriterFails_EscalatesGracefully()
    {
        var employeeId = Guid.NewGuid();
        var questionText = "سؤال غير مفهوم أبدا";
        var config = new AIConfiguration { TopK = 3, SimilarityThreshold = 0.25 };
        _configRepository.Setup(x => x.GetAsync(It.IsAny<CancellationToken>())).ReturnsAsync(config);
        _embeddingService.Setup(x => x.GenerateEmbeddingAsync(questionText, It.IsAny<CancellationToken>())).ReturnsAsync(new float[] { 0.1f });

        // Initial search returns empty
        _embeddingRepository.Setup(x => x.SearchSimilarAsync(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<double>(), questionText, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<(KnowledgeEmbedding, double)>());

        // Rewriter fails (returns null)
        _failoverService.Setup(x => x.RewriteQueryWithFailoverAsync(questionText, It.IsAny<CancellationToken>()))
            .ReturnsAsync((string?)null);

        var result = await _service.ProcessQuestionAsync(questionText, employeeId);

        result.Escalated.Should().BeTrue();
        result.Answered.Should().BeFalse();
        result.Status.Should().Be(QuestionStatus.NoAnswer.ToString());
        result.Answer.Should().Contain("Back Office");

        _failoverService.Verify(x => x.RewriteQueryWithFailoverAsync(questionText, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ProcessQuestionAsync_RewrittenSearchStillEmpty_TerminatesWithoutLoop()
    {
        var employeeId = Guid.NewGuid();
        var questionText = "سؤال خارج نطاق الخدمة";
        var rewrittenQuery = "استعلام معاد صياغته";

        var config = new AIConfiguration { TopK = 3, SimilarityThreshold = 0.25 };
        _configRepository.Setup(x => x.GetAsync(It.IsAny<CancellationToken>())).ReturnsAsync(config);
        _embeddingService.Setup(x => x.GenerateEmbeddingAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(new float[] { 0.1f });

        // Both searches return empty
        _embeddingRepository.Setup(x => x.SearchSimilarAsync(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<double>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<(KnowledgeEmbedding, double)>());

        _failoverService.Setup(x => x.RewriteQueryWithFailoverAsync(questionText, It.IsAny<CancellationToken>()))
            .ReturnsAsync(rewrittenQuery);

        var result = await _service.ProcessQuestionAsync(questionText, employeeId);

        result.Escalated.Should().BeTrue();
        result.Answered.Should().BeFalse();
        result.Status.Should().Be(QuestionStatus.NoAnswer.ToString());

        // Strictly single-pass: called once, no loop
        _failoverService.Verify(x => x.RewriteQueryWithFailoverAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
