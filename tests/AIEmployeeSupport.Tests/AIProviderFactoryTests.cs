using System;
using System.Net.Http;
using AIEmployeeSupport.Domain.Enums;
using AIEmployeeSupport.Infrastructure.AI;
using AIEmployeeSupport.Infrastructure.AI.Providers;
using FluentAssertions;
using Moq;
using Xunit;

namespace AIEmployeeSupport.Tests;

public class AIProviderFactoryTests
{
    [Fact]
    public void CreateClient_OpenAI_ReturnsOpenAIProvider()
    {
        var httpClientFactory = new Mock<IHttpClientFactory>();
        httpClientFactory.Setup(x => x.CreateClient("AIProviderClient")).Returns(new HttpClient());
        
        var factory = new AIProviderFactory(httpClientFactory.Object);

        var client = factory.CreateClient(ProviderType.OpenAI, "fake-api-key");

        client.Should().BeOfType<OpenAIProvider>();
    }

    [Fact]
    public void CreateClient_Gemini_ReturnsGeminiProvider()
    {
        var httpClientFactory = new Mock<IHttpClientFactory>();
        httpClientFactory.Setup(x => x.CreateClient("AIProviderClient")).Returns(new HttpClient());
        
        var factory = new AIProviderFactory(httpClientFactory.Object);

        var client = factory.CreateClient(ProviderType.Gemini, "fake-api-key");

        client.Should().BeOfType<GeminiProvider>();
    }

    [Fact]
    public void CreateClient_Anthropic_ReturnsAnthropicProvider()
    {
        var httpClientFactory = new Mock<IHttpClientFactory>();
        httpClientFactory.Setup(x => x.CreateClient("AIProviderClient")).Returns(new HttpClient());
        
        var factory = new AIProviderFactory(httpClientFactory.Object);

        var client = factory.CreateClient(ProviderType.Anthropic, "fake-api-key");

        client.Should().BeOfType<AnthropicProvider>();
    }
}
