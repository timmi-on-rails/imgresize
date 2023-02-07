using LanguageExt;

namespace ImageResizer.Runtime.Live;

/// <summary>
/// Real world interaction with the file-system
/// </summary>
public readonly struct FileMoveIO : Traits.FileMoveIO
{
    public readonly static Traits.FileMoveIO Default =
        new FileMoveIO();

    /// <inheritdoc/>
    public Unit Move(string fromPath, string toPath, bool overwrite = false)
    {
        File.Move(fromPath, toPath, overwrite);
        return default;
    }
}
