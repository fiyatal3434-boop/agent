using Microsoft.Extensions.AI;
using System.Text.Json;

public class EmbeddingManager
{
    private const string EmbeddingsFile = "embeddings.json";
    private readonly IEmbeddingGenerator<string, Embedding<float>> _embeddingService;
    private readonly DocumentLoader _documentLoader;
    private readonly ITextNormalizer _normalizer;
    private readonly ILemmatizer _lemmatizer;

    public EmbeddingManager(IEmbeddingGenerator<string, Embedding<float>> embeddingService, DocumentLoader documentLoader, ITextNormalizer normalizer, ILemmatizer lemmatizer)
    {
        _embeddingService = embeddingService;
        _documentLoader = documentLoader;
        _normalizer = normalizer;
        _lemmatizer = lemmatizer;
    }

    public async Task<List<EmbeddedChunk>> LoadOrBuildAsync()
    {
        Console.WriteLine("Embedding'ler hazırlanıyor...");

        var cached = File.Exists(EmbeddingsFile) 
            ? await LoadEmbeddingsAsync() 
            : new List<EmbeddedChunk>();

        var documents = await _documentLoader.LoadDocumentsAsync();
        var result = new List<EmbeddedChunk>();

        foreach (var (fileName, content, hash) in documents)
        {
            var cachedForFile = cached.Where(x => x.FileName == fileName).ToList();

            // Değişmemişse cache'den kullan
            if (cachedForFile.Count > 0 && cachedForFile.All(x => x.FileHash == hash))
            {
                Console.WriteLine($"{fileName} değişmedi, cache'den yüklendi.");
                result.AddRange(cachedForFile);
                continue;
            }

            // Yeniden embed et
            Console.WriteLine($"{fileName} yeniden embed ediliyor...");
            var chunks = await CreateEmbeddingsForDocumentAsync(fileName, content, hash);
            result.AddRange(chunks);
        }

        await SaveEmbeddingsAsync(result);
        Console.WriteLine($"Toplam {result.Count} chunk hazır.");

        return result;
    }

    private async Task<List<EmbeddedChunk>> CreateEmbeddingsForDocumentAsync(string fileName, string text, string hash)
    {
        var parts = text.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
        var result = new List<EmbeddedChunk>();
        int index = 1;

        foreach (var part in parts)
        {
            var normalized = await _normalizer.NormalizeAsync(part);
            var lemma = await _lemmatizer.LemmatizeAsync(normalized);
            var embedding = await _embeddingService.GenerateAsync(lemma);

            result.Add(new EmbeddedChunk
            {
                FileName = fileName,
                FileHash = hash,
                ChunkNo = index++,
                Content = part.Trim(),
                NormalizedContent = normalized,
                LemmatizedContent = lemma,
                Embedding = embedding.Vector.ToArray()
            });
        }

        return result;
    }

    private static async Task SaveEmbeddingsAsync(List<EmbeddedChunk> chunks)
    {
        var options = new JsonSerializerOptions { WriteIndented = true };
        var json = JsonSerializer.Serialize(chunks, options);
        await File.WriteAllTextAsync(EmbeddingsFile, json);
    }

    private static async Task<List<EmbeddedChunk>> LoadEmbeddingsAsync()
    {
        var json = await File.ReadAllTextAsync(EmbeddingsFile);
        return JsonSerializer.Deserialize<List<EmbeddedChunk>>(json) ?? new List<EmbeddedChunk>();
    }
}