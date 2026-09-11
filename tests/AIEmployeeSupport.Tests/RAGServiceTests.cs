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
        _embeddingRepository.Setup(x => x.SearchSimilarAsync(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<double>(), It.IsAny<CancellationToken>())).ReturnsAsync(new List<(KnowledgeEmbedding, double)>());

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
        _embeddingRepository.Setup(x => x.SearchSimilarAsync(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<double>(), It.IsAny<CancellationToken>())).ReturnsAsync(new List<(KnowledgeEmbedding, double)> { searchResult });

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
}
