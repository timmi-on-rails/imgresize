﻿namespace ImageResizer
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using LanguageExt;

    using static LanguageExt.Prelude;

    using SixLabors.ImageSharp.Formats.Jpeg;
    using ImageResizer.Components;

    public class Processor
    {
        /// <summary>
        /// Maximum tasks to use.
        /// </summary>
        private int maxTasks = 5;

        public Processor(Options options)
            => Options = options;

        /// <summary>
        /// Run the processor loop indefinitely.
        /// </summary>
        /// <returns>Task.</returns>
        public Task ProcessAsync() => Task.Run(async () =>
        {
            while (true)
            {
                var tasks = CheckForImageFiles(Options.SourceDirectory);

                CurrentCount = TaskCount = tasks.Count();
                Working = true;

                var jobs = tasks.Select(item => ProcessAsync(item)).ToList();

                await Task.WhenAll(jobs);
                Working = false;

                Thread.Sleep(1000);
            }
        });

        /// <summary>
        /// Find all image files in the given directory.
        /// </summary>
        /// <param name="directory">Absolute directory path.</param>
        /// <returns>List of task items.</returns>
        private static IEnumerable<TaskItem> CheckForImageFiles(string directory)
            => new DirectoryInfo(directory)
                .GetFiles()
                .Where(f => f.Extension.Contains("jpg", StringComparison.InvariantCultureIgnoreCase))
                .Select(f => new TaskItem(f.FullName));

        /// <summary>
        /// Process a single task item.
        /// </summary>
        /// <returns></returns>
        private async Task ProcessAsync(TaskItem item)
        {
            string original = Path.Combine(Options.DestinationDirectory, Path.GetFileName(item.Value));
            File.Move(item.Value, original);

            var img = Resizer.Resize(
                new TaskItem(original),
                Options.Width,
                Options.Height,
                Options.KeepAspectRatio);

            if (img != null)
            {
                using var writeStream = File.OpenWrite(Path.Combine(Options.MovedDirectory, Path.GetFileName(item.Value)));
                await img.SaveAsync(writeStream, new JpegEncoder { ColorType = JpegColorType.Rgb, Quality = 85 });
                CurrentCount--;
            }
        }

        /// <summary>
        /// Working state.
        /// </summary>
        public bool Working { get; private set; }

        /// <summary>
        /// Get total workload.
        /// </summary>
        public int TaskCount { get; private set; }

        /// <summary>
        /// Get current workload.
        /// </summary>
        public int CurrentCount { get; private set; }

        /// <summary> 
        /// Options set during start up.
        /// </summary>
        public Options Options { get; }
    }
}
