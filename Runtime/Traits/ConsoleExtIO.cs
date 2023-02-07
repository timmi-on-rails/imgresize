using LanguageExt;
using LanguageExt.Attributes;
using LanguageExt.Effects.Traits;

namespace ImageResizer.Runtime.Traits;

public interface ConsoleExtIO
{
    int WindowWidth { get; }

    event ConsoleCancelEventHandler? CancelKeyPress;
}

/// <summary>
/// Type-class giving a struct the trait of supporting File IO
/// </summary>
/// <typeparam name="RT">Runtime</typeparam>
[Typeclass("*")]
public interface HasConsoleExt<RT> : HasCancel<RT>
    where RT : struct, HasCancel<RT>
{
    /// <summary>
    /// Access the file synchronous effect environment
    /// </summary>
    /// <returns>File synchronous effect environment</returns>
    Eff<RT, ConsoleExtIO> ConsoleExtEff { get; }
}
