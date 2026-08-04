using System;
using System.Threading.Tasks;
using DocuMind.Api.Services.EmbeddingService;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Api.Tests.Services;

public class GoogleGenAiClientTests
{
    [Fact]
    public void Constructor_ThrowsArgumentException_WhenApiKeyIsMissing()
    {
        var optionsMock = new Mock<IOptions<GoogleAIOptions>>();
        optionsMock.Setup(x => x.Value).Returns(new GoogleAIOptions { ApiKey = string.Empty });

        Action act = () => _ = new GoogleGenAiClient(optionsMock.Object);

        act.Should().Throw<ArgumentException>()
            .WithMessage("GoogleAI:ApiKey must be configured.*")
            .And.ParamName.Should().Be("ApiKey");
    }

    [Fact]
    public void Constructor_DoesNotThrow_WhenApiKeyIsProvided()
    {
        var optionsMock = new Mock<IOptions<GoogleAIOptions>>();
        optionsMock.Setup(x => x.Value).Returns(new GoogleAIOptions { ApiKey = "test-key" });

        Action act = () => _ = new GoogleGenAiClient(optionsMock.Object);

        act.Should().NotThrow();
    }

    [Fact]
    public void Dispose_CanBeCalledMultipleTimes()
    {
        var optionsMock = new Mock<IOptions<GoogleAIOptions>>();
        optionsMock.Setup(x => x.Value).Returns(new GoogleAIOptions { ApiKey = "test-key" });

        var client = new GoogleGenAiClient(optionsMock.Object);

        Action firstDispose = () => client.Dispose();
        Action secondDispose = () => client.Dispose();

        firstDispose.Should().NotThrow();
        secondDispose.Should().NotThrow();
    }

    [Fact]
    public async Task DisposeAsync_CanBeCalledMultipleTimes()
    {
        var optionsMock = new Mock<IOptions<GoogleAIOptions>>();
        optionsMock.Setup(x => x.Value).Returns(new GoogleAIOptions { ApiKey = "test-key" });

        var client = new GoogleGenAiClient(optionsMock.Object);

        Func<Task> firstDisposeAsync = () => client.DisposeAsync().AsTask();
        Func<Task> secondDisposeAsync = () => client.DisposeAsync().AsTask();

        await firstDisposeAsync.Should().NotThrowAsync();
        await secondDisposeAsync.Should().NotThrowAsync();
    }
}
