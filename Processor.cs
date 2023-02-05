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

/// <summary>
/// Working state information.
/// </summary>
/// <param name="TaskCount">Get total workload.</param>
/// <param name="CurrentCount">Get current workload.</param>
public record WorkingStateInfo(int TaskCount, int CurrentCount);

// TODO:
// Return error sum-type for better error messages
// Handle error-recovery code -> file.delete etc.

public static class Processor<RT>
    where RT : struct, HasCancel<RT>, HasDirectory<RT>, HasFile<RT>, HasConsole<RT>, HasTime<RT>
{
    /// <summary>
    /// Run the processor loop indefinitely.
    /// </summary>
    /// <returns>Observable of optional working state information.
    /// Missing values indicate that there is currently no work to do.</returns>
    public static Producer<RT, Option<WorkingStateInfo>, Unit> Run(Options options)
        => repeat(
            from _1 in (
                ProcessSourceDirectory(options) |
                Pipe.map<RT, WorkingStateInfo, Option<WorkingStateInfo>, Unit>(Some))
            from _2 in yield(Option<WorkingStateInfo>.None)
            from _3 in Time<RT>.sleepFor(options.CheckDelay)
            select unit);

    /// <summary>
    /// Process all files in the source directory concurrently.
    /// </summary>
    /// <returns>Sequence of processed files/state info.</returns>
    private static Producer<RT, WorkingStateInfo, Unit> ProcessSourceDirectory(Options options)
    {
        return from images in Directory<RT>.enumerateFiles(options.SourceDirectory, "*.jpg")
               from _1 in images.Count > 0 ?
                    yield(new WorkingStateInfo(images.Count, images.Count)) :
                    Pure(unit)
               from _2 in go(images) | Increment(images.Count)
               select unit;

        Producer<RT, Fin<Unit>, Unit> go(Seq<string> images)
            => from results in images
                    .Map(img => ProcessImageFile(img, options))
                    .Merge(options.MaxConcurrent)
               from _ in yieldAll(results)
               select unit;

        static Pipe<RT, Fin<Unit>, WorkingStateInfo, Unit> Increment(int count)
        {
            var counter = Atom(count);

            return from _1 in awaiting<Fin<Unit>>()
                   from _2 in counter.SwapEff<RT>(c => SuccessEff(c - 1))
                   from _3 in yield(new WorkingStateInfo(count, counter))
                   select unit;
        }
    }

    /// <summary>
    /// Process a single image file.
    /// </summary>
    private static Aff<RT, Unit> ProcessImageFile(string path, Options options)
        => from _1 in Eff(() => unit)
           let original = Path.Combine(options.DestinationDirectory, Path.GetFileName(path))
           // TODO use File.move once available -> HasFileMoveIO
           from _2 in File<RT>.copy(path, original)
           from _3 in File<RT>.delete(path)
           from _4 in Img<RT>.CopyImageResized(
               original,
               Path.Combine(options.MovedDirectory, Path.GetFileName(path)),
               options.Width,
               options.Height,
               options.KeepAspectRatio)
           select unit;
}
