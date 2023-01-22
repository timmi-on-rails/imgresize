﻿namespace ImageResizer;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;

using LanguageExt;

using static LanguageExt.Prelude;

using LanguageExt.Sys.Traits;
using LanguageExt.Sys.IO;
using LanguageExt.Sys.Live;

public static class Processor
{
    /// <summary>
    /// Run the processor loop indefinitely.
    /// </summary>
    /// <returns>Observable of optional working state information.
    /// Missing values indicate that there is currently no work to do.</returns>
    public static IObservable<Option<WorkingStateInfo>> RunAsync(
        Options options,
        CancellationToken cancellationToken = default)
        => Observable.Defer(() => ProcessDirectory(options, cancellationToken))
            .Select(Some)
            .Append(None)
            .Concat(Wait<Option<WorkingStateInfo>>(
                options.CheckDelay.ToTimeSpan(),
                cancellationToken))
            .Repeat()
            .OnErrorResumeNext(Observable.Empty<Option<WorkingStateInfo>>())
            .Replay(1)
            .AutoConnect(0);

    /// <summary>
    /// Creates an observable sequence that emits no elements and
    /// completes after the given duration.
    /// If cancellation is requested the sequence immediately
    /// emits an error.
    /// </summary>
    private static IObservable<T> Wait<T>(
        TimeSpan duration,
        CancellationToken cancellationToken)
        => Observable
            .Never<T>()
            .ToTask(cancellationToken)
            .ToObservable()
            .TakeUntil(Observable.Timer(duration));

    /// <summary>
    /// Process all files in the source directory concurrently.
    /// </summary>
    /// <returns>Sequence of processed files/state info.</returns>
    private static IObservable<WorkingStateInfo> ProcessDirectory(
        Options options,
        CancellationToken cancellationToken)
    {
        var taskItems = CheckForImageFiles(options.SourceDirectory);
        int taskItemsCount = taskItems.Count();

        return taskItems
            .Select(item => Observable.Defer(() =>
                cancellationToken.IsCancellationRequested ?
                    Observable.Empty<string>() :
                    Observable.FromAsync(async () =>
                    (await (ProcessAsync<Runtime>(item, options).Run(Runtime.New()))).ThrowIfFail())))
            .Merge(options.MaxConcurrent)
            .Scan(
                new WorkingStateInfo(taskItemsCount, taskItemsCount),
                (ws, _) => ws with { CurrentCount = ws.CurrentCount - 1 })
            .StartWith(new WorkingStateInfo(taskItemsCount, taskItemsCount));
    }

    /// <summary>
    /// Find all image files in the given directory.
    /// </summary>
    /// <param name="directory">Absolute directory path.</param>
    /// <returns>List of task items.</returns>
    private static IEnumerable<string> CheckForImageFiles(string directory)
        => new DirectoryInfo(directory)
            .GetFiles()
            .Where(f => f.Extension.Contains("jpg", StringComparison.InvariantCultureIgnoreCase))
            .Select(f =>f.FullName);

    /// <summary>
    /// Process a single image.
    /// </summary>
    /// <returns>Task.</returns>
    private static Aff<RT, string> ProcessAsync<RT>(string path, Options options)
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
