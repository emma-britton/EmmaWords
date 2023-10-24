namespace Emma.Lib;

/// <summary>
/// Represents a dictionary definition.
/// </summary>
/// <param name="Word">The word being defined.</param>
/// <param name="Pos">The part of speech that this definition relates to.</param>
/// <param name="See">A word that contains the rest of the definition; for example, a variation or uninflected form.</param>
/// <param name="Content">The content of the definition.</param>
/// <param name="Offensive">Indicates that the word is potentially offensive, so it is not repeated in certain contexts.</param>
public record Definition(string Word, PartOfSpeech Pos, string? See, string Content)
{
    public bool Offensive { get; set; }
}

