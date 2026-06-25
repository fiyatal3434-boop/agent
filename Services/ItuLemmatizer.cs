public class ItuLemmatizer : ILemmatizer
{
    private readonly HttpClient _httpClient;
    private readonly string _token;

    public ItuLemmatizer(
        HttpClient httpClient,
        string token)
    {
        _httpClient = httpClient;
        _token = token;
    }

    public async Task<string> LemmatizeAsync(string text)
    {
        var response = await _httpClient.PostAsync(
            "http://tools.nlp.itu.edu.tr/SimpleApi",
            new FormUrlEncodedContent(
            [
                new("tool", "pipelineSSMorph"),
                new("input", text),
                new("token", _token)
            ]));

        var content = await response.Content.ReadAsStringAsync();

        var lemmas = new List<string>();

        foreach (var line in content.Split('\n'))
        {
            var parts = line.Split('\t');

            if(parts.Length < 3)
                continue;

            var lemma = parts[2].Trim();

            if(!StopWords.Turkish.Contains(lemma))
            {
                lemmas.Add(lemma);
            }
        }

        return string.Join(" ", lemmas);
    }
}