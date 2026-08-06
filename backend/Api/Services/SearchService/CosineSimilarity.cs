namespace DocuMind.Api.Services.SearchService;
public static class CosineSimilarity
{
    public static double Calculate(
        IReadOnlyList<float> vector1,
        IReadOnlyList<float> vector2)
    {
        if (vector1.Count != vector2.Count)
            throw new ArgumentException(
                "Vectors must have the same dimensions.");

        double dotProduct = 0;
        double magnitudeA = 0;
        double magnitudeB = 0;

        for (int i = 0; i < vector1.Count; i++)
        {
            dotProduct += vector1[i] * vector2[i];

            magnitudeA += Math.Pow(vector1[i], 2);

            magnitudeB += Math.Pow(vector2[i], 2);
        }

        magnitudeA = Math.Sqrt(magnitudeA);

        magnitudeB = Math.Sqrt(magnitudeB);

        if (magnitudeA == 0 || magnitudeB == 0)
            return 0;

        return dotProduct / (magnitudeA * magnitudeB);
    }
}