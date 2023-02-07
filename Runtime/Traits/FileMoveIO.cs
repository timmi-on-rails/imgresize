using LanguageExt;
using LanguageExt.Attributes;
using LanguageExt.Effects.Traits;

namespace ImageResizer.Runtime.Traits;

public interface FileMoveIO
{
    /// <summary>
    /// Move file from one place to another
    /// </summary>
    Unit Move(string fromPath, string toPath, bool overwrite = false);
}

/// <summary>
/// Type-class giving a struct the trait of supporting File IO
/// </summary>
/// <typeparam name="RT">Runtime</typeparam>
[Typeclass("*")]
public interface HasFileMove<RT> : HasCancel<RT>
    where RT : struct, HasCancel<RT>
{
    /// <summary>
    /// Access the file synchronous effect environment
    /// </summary>
    /// <returns>File synchronous effect environment</returns>
    Eff<RT, FileMoveIO> FileMoveEff { get; }
}

