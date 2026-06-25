using System.Text;
using System.Security.Cryptography;

public class DocumentLoader
{
    private readonly string _documentsPath;

    public DocumentLoader(string documentsPath = "/Users/itu/Documents/isg")
    {
        _documentsPath = documentsPath;
    }

    public async Task<List<(string FileName, string Content, string Hash)>> LoadDocumentsAsync()
    {
        var result = new List<(string FileName, string Content, string Hash)>();
        var files = Directory.GetFiles(_documentsPath, "*.txt");

        foreach (var file in files)
        {
            var content = await File.ReadAllTextAsync(file);
            var hash = CalculateHash(content);
            var fileName = Path.GetFileName(file);

            result.Add((fileName, content, hash));
        }

        return result;
    }

    private static string CalculateHash(string text)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(text));
        return Convert.ToHexString(bytes);
    }
}