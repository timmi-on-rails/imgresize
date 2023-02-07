using ImageResizer.Runtime.Traits;
using LanguageExt;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace ImageResizer.Runtime.Sys;

public static class FileMove<RT> where RT : struct, HasFileMove<RT>
{
    /// <summary>
    /// Move file 
    /// </summary>
    /// <param name="fromPath">Source path</param>
    /// <param name="toPath">Destination path</param>
    /// <param name="overwrite">Overwrite if the file already exists at the destination</param>
    /// <typeparam name="RT">Runtime</typeparam>
    /// <returns>Unit</returns>
    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Eff<RT, Unit> move(string fromPath, string toPath, bool overwrite = false) =>
        default(RT).FileMoveEff.Map(e => e.Move(fromPath, toPath, overwrite));
}
