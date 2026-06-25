using System.ComponentModel;
using Microsoft.SemanticKernel;


public class DocumentPlugin
{
    [KernelFunction]
    [Description(
    "İSG mevzuatı, prosedürleri ve dokümanları içerisinde arama yapar. Kullanıcı yönetmelik, prosedür, zorunluluk veya doküman içeriği soruyorsa kullanılmalıdır.")]
    public string SearchDocuments(
        [Description("Kullanıcının sorusu")]
        string question)
    {
        Console.WriteLine(">>> SearchDocuments başladı");

   
        var questionLower =
            question.ToLowerInvariant();

        var questionWords =
            questionLower.Split(
                ' ',
                StringSplitOptions.RemoveEmptyEntries);

        var results = new List<SearchResult>();
        var chunks = LoadChunks();
        
        Console.WriteLine(
            $"Chunk sayısı: {chunks.Count}");

        foreach (var chunk in chunks)
        {
            var score =
                CalculateScore(
                    chunk.Content.ToLowerInvariant(),
                    questionWords);

            results.Add(new SearchResult
            {
                FileName = chunk?.FileName ?? "",
                ChunkNo = chunk?.ChunkNo ?? 0,
                Content = chunk?.Content ?? "",
                Score = score
            });
        }



        var top3 = results
            .OrderByDescending(r => r.Score)
            .Take(3)
            .ToList();

        Console.WriteLine("----- TOP3 -----");

        foreach (var item in top3)
        {
            Console.WriteLine(
                $"{item.FileName} " +
                $"{item.ChunkNo} " +
                $"{item.Score}");

            Console.WriteLine(item.Content);
        }
    

        return """Aşağıdaki bilgiler İSG dokümanlarından alınmıştır.""" +
                string.Join(
                    "\n\n",
                    top3.Select(x => x.Content));

    }

    private static async Task<List<EmbeddedChunk>> LoadEmbeddedChunksAsync()
    {
        var chunks = new List<EmbeddedChunk>();

        var files =
            Directory.GetFiles(
                "/Users/itu/Documents/isg",
                "*.txt");

        foreach (var file in files)
        {
            var text =
                await File.ReadAllTextAsync(file);

            var parts =
                text.Split(
                    Environment.NewLine,
                    StringSplitOptions.RemoveEmptyEntries);

            var index = 1;

            foreach (var part in parts)
            {
                chunks.Add(
                    new EmbeddedChunk
                    {
                        FileName =
                            Path.GetFileName(file),

                        ChunkNo = index++,

                        Content = part
                    });
            }
        }

        return chunks;
    }



    private static List<DocumentChunk>
    LoadChunks()
    {
        var chunks =
            new List<DocumentChunk>();

        var files =
            Directory.GetFiles(
                "/Users/itu/Documents/isg",
                "*.txt");

        foreach (var file in files)
        {
            var text =
                File.ReadAllText(file);

            var parts =
                text.Split(
                    Environment.NewLine,
                    StringSplitOptions.RemoveEmptyEntries);

            var index = 1;

            foreach (var part in parts)
            {
                chunks.Add(
                    new DocumentChunk
                    {
                        FileName =
                            Path.GetFileName(file),

                        ChunkNo = index++,

                        Content = part
                    });
            }
        }

        return chunks;
    }

        private static int CalculateScore(
        string content,
        string[] questionWords)
    {
        var score = 0;

        foreach (var word in questionWords)
        {
            if (SearchConstants.IgnoredWords.Contains(word))
                continue;

            if (content.Contains(word))
                score++;
        }

        return score;
    }

    private static double CosineSimilarity(
    float[] v1,
    float[] v2)
    {
        double dot = 0;
        double mag1 = 0;
        double mag2 = 0;

        for (int i = 0; i < v1.Length; i++)
        {
            dot += v1[i] * v2[i];
            mag1 += v1[i] * v1[i];
            mag2 += v2[i] * v2[i];
        }

        return dot /
            (Math.Sqrt(mag1) *
                Math.Sqrt(mag2));
    }
}