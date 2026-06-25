public class EmbeddedChunk
{
    public string FileName { get; set; } = "";
    public string FileHash {get; set;} = "";

    public int ChunkNo { get; set; }

    public string Content { get; set; } = "";
    public string NormalizedContent { get; set; } = "";
public string LemmatizedContent { get; set; } = "";
    public float[] Embedding { get; set; } = [];
}