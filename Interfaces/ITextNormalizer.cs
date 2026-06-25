public interface ITextNormalizer
{
    Task<string> NormalizeAsync(string text);
}