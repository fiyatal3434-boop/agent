public class NoOpNormalizer
    : ITextNormalizer
{
    public Task<string> NormalizeAsync(
        string text)
    {
        return Task.FromResult(text);
    }
}