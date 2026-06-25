public sealed class ItuTextNormalizer
    : ITextNormalizer
{
    private readonly HttpClient _httpClient;
    private readonly string _token;

    public ItuTextNormalizer(
        HttpClient httpClient,
        string token)
    {
        _httpClient = httpClient;
        _token = token;
    }

    public async Task<string>
        NormalizeAsync(
            string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return text;

        text = await DeasciifyAsync(text);
        text = await NormalizeTextAsync(text);

        return text;
    }

    private async Task<string>
        DeasciifyAsync(
            string text)
    {
        return await CallApiAsync(
            "deasciifier",
            text);
    }

    private async Task<string>
        NormalizeTextAsync(
            string text)
    {
        return await CallApiAsync(
            "normalize",
            text);
    }

    private async Task<string>
        CallApiAsync(
            string tool,
            string text)
    {
        var content =
            new FormUrlEncodedContent(
                new Dictionary<string, string>
                {
                    ["tool"] = tool,
                    ["input"] = text,
                    ["token"] = _token
                });

        var response =
            await _httpClient.PostAsync(
                "http://tools.nlp.itu.edu.tr/SimpleApi",
                content);

        response.EnsureSuccessStatusCode();

        var result =
            await response.Content
                .ReadAsStringAsync();

        return result.Trim();
    }
}