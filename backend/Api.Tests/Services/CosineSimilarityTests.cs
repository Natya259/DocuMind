using DocuMind.Api.Services.SearchService;
using FluentAssertions;

namespace DocuMind.Api.Services.SearchService;

public class CosineSimilarityTests
{
    [Fact]
    public void Calculate_ReturnsOne_ForIdenticalVectors()
    {
        var vectorA = new float[] { 1f, 2f, 3f };
        var vectorB = new float[] { 1f, 2f, 3f };

        var result = CosineSimilarity.Calculate(vectorA, vectorB);

        result.Should().BeApproximately(1.0, 1e-9);
    }

    [Fact]
    public void Calculate_ReturnsZero_WhenOneVectorIsZero()
    {
        var vectorA = new float[] { 0f, 0f, 0f };
        var vectorB = new float[] { 1f, 2f, 3f };

        var result = CosineSimilarity.Calculate(vectorA, vectorB);

        result.Should().Be(0);
    }

    [Fact]
    public void Calculate_ReturnsZero_ForOrthogonalVectors()
    {
        var vectorA = new float[] { 1f, 0f, 0f };
        var vectorB = new float[] { 0f, 1f, 0f };

        var result = CosineSimilarity.Calculate(vectorA, vectorB);

        result.Should().BeApproximately(0.0, 1e-9);
    }

    [Fact]
    public void Calculate_ReturnsExpectedValue_ForKnownVectors()
    {
        var vectorA = new float[] { 1f, 2f, 3f };
        var vectorB = new float[] { 4f, 5f, 6f };

        var result = CosineSimilarity.Calculate(vectorA, vectorB);

        result.Should().BeApproximately(0.974631846, 1e-9);
    }

    [Fact]
    public void Calculate_ThrowsArgumentException_WhenVectorLengthsDiffer()
    {
        var vectorA = new float[] { 1f, 2f };
        var vectorB = new float[] { 1f, 2f, 3f };

        Action action = () => CosineSimilarity.Calculate(vectorA, vectorB);

        action.Should().Throw<ArgumentException>()
            .WithMessage("Vectors must have the same dimensions.");
    }
}
