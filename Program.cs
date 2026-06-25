using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.Extensions.AI;
using System.Text.Json;
using Microsoft.Extensions.Configuration;


var configuration = new ConfigurationBuilder()
    .AddUserSecrets<Program>()           // User Secrets
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .Build();


var ituToken = configuration["ItuNlp:Token"] 
    ?? throw new Exception("Itu NLP Token bulunamadı!");

const string DocumentsPath = "/Users/itu/Documents/isg";

// ─── Kernel Setup ───
var builder = Kernel.CreateBuilder();

    
builder.AddOllamaEmbeddingGenerator(
    modelId: "nomic-embed-text",
    endpoint: new Uri("http://localhost:11434"));

var httpClient = new HttpClient { Timeout = TimeSpan.FromMinutes(10) };

builder.AddOpenAIChatCompletion(
    modelId: "qwen3:4b",
    apiKey: "ollama",
    endpoint: new Uri("http://localhost:11434/v1"),
    httpClient: httpClient);

var normalizer = new ItuTextNormalizer(
    new HttpClient(),
    ituToken);

var lemmatizer = new ItuLemmatizer(
    new HttpClient(),
    ituToken);

// Test kodları (isteğe bağlı, istersen silebilirsin)
var x = await normalizer.NormalizeAsync("yuksekten dusmeyi onlemek icin ne kullanilir");
Console.WriteLine("Normalized: " + x);


var lemma1 =
    await lemmatizer.LemmatizeAsync(
        "Yüksekten düşmeyi önlemek için ne kullanılır");


Console.WriteLine(lemma1);


// ─── Kernel Build ───
var kernel = builder.Build();
var embeddingService = kernel.GetRequiredService<IEmbeddingGenerator<string, Embedding<float>>>();
var chatService = kernel.GetRequiredService<IChatCompletionService>();

// ─── Servisleri oluştur ───
var documentLoader = new DocumentLoader(DocumentsPath);
var embeddingManager = new EmbeddingManager(embeddingService, documentLoader, normalizer, lemmatizer);
var searchService = new SearchService(embeddingService);

var embeddedChunks = await embeddingManager.LoadOrBuildAsync();

// ─── Chat Döngüsü ───
while (true)
{
    Console.Write("> ");
    var input = Console.ReadLine()?.Trim();
    if (string.IsNullOrWhiteSpace(input) || input.ToLower() == "exit") break;

    var normalized = await normalizer.NormalizeAsync(input);
    var lemma = await lemmatizer.LemmatizeAsync(normalized);

    Console.WriteLine($"Soru      : {input}");
    Console.WriteLine($"Normalize : {normalized}");
    Console.WriteLine($"Lemma     : {lemma}");

    // Arama
    var results = await searchService.SearchAsync(lemma, embeddedChunks);

    // Sonuçları göster
    Console.WriteLine($"\nSoru: {input}\n");
    foreach (var (i, chunk) in results.Select((r, i) => (i + 1, r)))   // ← result → chunk
    {
        Console.WriteLine($"{i}) [{chunk.FileName}] (skor: {chunk.Score:F3})");
        Console.WriteLine($"   {chunk.Content}");
        Console.WriteLine();
    }

    // Prompt oluştur
    var context = string.Join("\n\n", results.Select(r => r.Content));

    var prompt = $"""
        Sen bir İSG (İş Sağlığı ve Güvenliği) uzmanısın.
        Soru: {input}
        Dokümanlar:
        {context}

        Sadece verilen dokümanlara dayanarak cevap ver.
        Cevabın net, profesyonel ve yardımcı olsun.
        """;

    // Cevap al
    var chatResponse = await chatService.GetChatMessageContentAsync(prompt);  // ← response yerine chatResponse

    Console.WriteLine("Cevap:");
    Console.WriteLine(chatResponse.Content);
    Console.WriteLine(new string('-', 80));
}