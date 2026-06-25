using Microsoft.Extensions.AI;

public class SearchService
{
    private readonly IEmbeddingGenerator<string, Embedding<float>> _embeddingService;

    public SearchService(IEmbeddingGenerator<string, Embedding<float>> embeddingService)
    {
        _embeddingService = embeddingService;
    }

    public async Task<List<SearchResult>> SearchAsync(string question, List<EmbeddedChunk> chunks, int topN = 3)
    {
        var questionEmbedding = await _embeddingService.GenerateAsync(question);
        var questionVector = questionEmbedding.Vector.ToArray();

        return chunks
            .Select(chunk => new SearchResult
            {
                FileName = chunk.FileName,
                ChunkNo = chunk.ChunkNo,
                Content = chunk.Content,
                Score = CosineSimilarity(questionVector, chunk.Embedding)
            })
            .OrderByDescending(r => r.Score)
            .Take(topN)
            .ToList();
    }

    private static double CosineSimilarity(float[] v1, float[] v2)
    {
        double dot = 0, mag1 = 0, mag2 = 0;

        for (int i = 0; i < v1.Length; i++)
        {
            dot += v1[i] * v2[i];
            mag1 += v1[i] * v1[i];
            mag2 += v2[i] * v2[i];
        }

        return dot / (Math.Sqrt(mag1) * Math.Sqrt(mag2));
    }
}