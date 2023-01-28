namespace ImageResizer;

using System.Linq;

using LanguageExt;

using static LanguageExt.Prelude;
using static LanguageExt.Pipes.Proxy;

using LanguageExt.Sys.Traits;
using LanguageExt.Sys.IO;
using LanguageExt.Pipes;
using LanguageExt.Effects.Traits;
using LanguageExt.Sys;

public static class Processor<RT>
    where RT : struct, HasCancel<RT>, HasDirectory<RT>, HasFile<RT>, HasConsole<RT>, HasTime<RT>
{
    /// <summary>
    /// Run the processor loop indefinitely.
    /// </summary>
    /// <returns>Observable of optional working state information.
    /// Missing values indicate that there is currently no work to do.</returns>
    public static Producer<RT, Fin<string>, Unit> RunAsync(Options options)
        // TODO add interleaving None values after each check!
        => repeat(ProcessSourceDirectory(options));

    /// <summary>
    /// Process all files in the source directory concurrently.
    /// </summary>
    /// <returns>Sequence of processed files/state info.</returns>
    private static Producer<RT, Fin<string>, Unit> ProcessSourceDirectory(Options options)
        // TODO sleepFor belongs in caller code, where repeat is.    
        => from _1 in Time<RT>.sleepFor(options.CheckDelay)
           from images in Directory<RT>.enumerateFiles(options.SourceDirectory, "*.jpg")
           // TODO check if lazy character of Seq is problematic
           let affs = images.Map(img => ProcessImageFile(img, options))
           from results in AffUtil.Merge<RT, string>(affs, options.MaxConcurrent)
           from _2 in Console<RT>.writeLine("Found " + affs.Length + " files.")
           // TODO dirty to check on images
           from _3 in images.Count > 0 ? Proxy.enumerate<Fin<string>>(results) : Proxy.Pure(unit)
           select unit;


    //        .Scan(
    //            new WorkingStateInfo(taskItemsCount, taskItemsCount),
    //            (ws, _) => ws with { CurrentCount = ws.CurrentCount - 1 })
    //        .StartWith(new WorkingStateInfo(taskItemsCount, taskItemsCount));

    //private static Pipe<RT, string, WorkingStateInfo, Unit> ThePipe =>
    //    from _ in awaiting<string>()
    //    select fold

    /// <summary>
    /// Process a single image file.
    /// </summary>
    /// <returns>Task.</returns>
    private static Aff<RT, string> ProcessImageFile(string path, Options options)
        => from _1 in Eff(() => unit)
           let original = Path.Combine(options.DestinationDirectory, Path.GetFileName(path))
           // TODO use File.move once available -> HasFileMoveIO
           from _2 in File<RT>.copy(path, original)
           from _3 in File<RT>.delete(path)
           from _4 in Img.CopyImageResized<RT>(
               original,
               Path.Combine(options.MovedDirectory, Path.GetFileName(path)),
               options.Width,
               options.Height,
               options.KeepAspectRatio)
           select path;

    /// <summary>
    /// Working state information.
    /// </summary>
    /// <param name="TaskCount">Get total workload.</param>
    /// <param name="CurrentCount">Get current workload.</param>
    public record WorkingStateInfo(int TaskCount, int CurrentCount);
}
