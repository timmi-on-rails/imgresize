using ImageResizer.Runtime.Traits;
using LanguageExt;
using System.Runtime.CompilerServices;

using static LanguageExt.Prelude;

namespace ImageResizer.Runtime.Sys;

public static class ConsoleExt<RT> where RT : struct, HasConsoleExt<RT>
{
    /// <summary>
    /// Move file
    /// </summary>
    /// <param name="fromPath">Source path</param>
    /// <param name="toPath">Destination path</param>
    /// <param name="overwrite">Overwrite if the file already exists at the destination</param>
    /// <typeparam name="RT">Runtime</typeparam>
    /// <returns>Unit</returns>
    public static Eff<RT, int> windowWidth
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => default(RT).ConsoleExtEff.Map(e => e.WindowWidth);
    }
}
