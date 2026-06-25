public interface ITextPreprocessor
{
    Task<string> ProcessAsync(string text);
}