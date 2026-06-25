// StopWords.cs
using System.Collections.Generic;

public static class StopWords
{
    public static readonly HashSet<string> Turkish =
    [
        "için", "ile", "ve", "de", "da", "bir", "bu", "şu", "o", "ne", "hangi",
        "mi", "mı", "mu", "mü", "ama", "fakat", "ancak", "veya", "ya da", "ki",
        "daha", "çok", "az", "her", "hiç", "bazı", "tüm", "tüm", "olarak"
        // İstersen buraya daha fazla stop word ekleyebilirsin
    ];
}