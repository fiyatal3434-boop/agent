public interface ILemmatizer
{
    Task<string> LemmatizeAsync(string text);
}