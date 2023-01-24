namespace ImageResizer;

using System;
using System.Linq;

using LanguageExt;

using static LanguageExt.Prelude;

using LanguageExt.Sys.Traits;
using LanguageExt.Sys.IO;
using LanguageExt.Sys.Live;
using LanguageExt.Pipes;
using LanguageExt.Effects.Traits;
using LanguageExt.Sys;

public static class Processor
{
    /// <summary>
    /// Run the processor loop indefinitely.
    /// </summary>
    /// <returns>Observable of optional working state information.
    /// Missing values indicate that there is currently no work to do.</returns>
    public static Producer<RT, Fin<string>, Unit> RunAsync<RT>(Options options)
        where RT : struct, HasCancel<RT>, HasDirectory<RT>, HasFile<RT>, HasConsole<RT>
        => from producer in Eff(() => ProcessSourceDirectory<RT>(options))
           from _0 in Console<RT>.writeLine("about to check")
           from _1 in producer
           from _3 in Console<RT>.writeLine("checked")
           from _2 in Aff<RT, Unit>(async _ =>
           {
               await Task.Delay(1000);
               return unit;
           })
           select unit;
    //=> Observable.Defer(() => ProcessDirectory<Runtime>(options, cancellationToken))
    //    .Select(Some)
    //    .Append(None)
    //    .Concat(Wait<Option<WorkingStateInfo>>(
    //        options.CheckDelay.ToTimeSpan(),
    //        cancellationToken))
    //    .Repeat()
    //    .OnErrorResumeNext(Observable.Empty<Option<WorkingStateInfo>>())
    //    .Replay(1)
    //    .AutoConnect(0);

    /// <summary>
    /// Creates an observable sequence that emits no elements and
    /// completes after the given duration.
    /// If cancellation is requested the sequence immediately
    /// emits an error.
    /// </summary>
    //private static IObservable<T> Wait<T>(
    //    TimeSpan duration,
    //    CancellationToken cancellationToken)
    //    => Observable
    //        .Never<T>()
    //        .ToTask(cancellationToken)
    //        .ToObservable()
    //        .TakeUntil(Observable.Timer(duration));

    /// <summary>
    /// Process all files in the source directory concurrently.
    /// </summary>
    /// <returns>Sequence of processed files/state info.</returns>
    private static Producer<RT, Fin<string>, Unit> ProcessSourceDirectory<RT>(Options options)
            where RT : struct, HasDirectory<RT>, HasCancel<RT>, HasFile<RT>, HasConsole<RT>
            => from images in Directory<RT>.enumerateFiles(options.SourceDirectory, "*.jpg")
               let affs = images.Select(img => ProcessImageFile<RT>(img, options)).ToArray()
               from results in AffUtil.Merge<RT, string>(affs)
               from __ in Console<RT>.writeLine("Found " + affs.Length + " files.")
               from _ in Proxy.enumerate<Fin<string>>(results)
               select unit;

    //{
    //    var taskItems = ListImageFiles<Runtime>(options.SourceDirectory).Run(Runtime.New()).ThrowIfFail();
    //    int taskItemsCount = taskItems.Count();

    //    return taskItems
    //        .Select(item => Observable.Defer(() =>
    //            cancellationToken.IsCancellationRequested ?
    //                Observable.Empty<string>() :
    //                Observable.FromAsync(async () =>
    //                (await (ProcessAsync<Runtime>(item, options).Run(Runtime.New()))).ThrowIfFail())))
    //        .Merge(options.MaxConcurrent)
    //        .Scan(
    //            new WorkingStateInfo(taskItemsCount, taskItemsCount),
    //            (ws, _) => ws with { CurrentCount = ws.CurrentCount - 1 })
    //        .StartWith(new WorkingStateInfo(taskItemsCount, taskItemsCount));
    //}

    /// <summary>
    /// Process a single image file.
    /// </summary>
    /// <returns>Task.</returns>
    private static Aff<RT, string> ProcessImageFile<RT>(string path, Options options)
        where RT : struct, HasFile<RT>
        => from _1 in Eff(() => unit)
           let original = Path.Combine(options.DestinationDirectory, Path.GetFileName(path))
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
