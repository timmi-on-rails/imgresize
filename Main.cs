using ImageResizer.Runtime.Sys;
using ImageResizer.Runtime.Traits;
using LanguageExt;
using LanguageExt.Effects.Traits;
using LanguageExt.Pipes;
using LanguageExt.Sys;
using LanguageExt.Sys.Traits;

using static LanguageExt.Prelude;

namespace ImageResizer;

public static class Main<RT>
    where RT : struct, HasCancel<RT>, HasConsole<RT>, HasConsoleExt<RT>,
    HasFile<RT>, HasFileMove<RT>, HasDirectory<RT>, HasTime<RT>
{
    public static Aff<RT, Unit> MainIO(string[] args)
        => from _1 in cancelKey
           from opts in CommandLineOptions(args)
           from _2 in (Processor<RT>.Run(opts) | writeLine)
           select unit;

    private static Aff<RT, Unit> cancelKey
        => runtime<RT>().Bind(env => env.ConsoleExtEff.Map(console =>
        {
            console.CancelKeyPress += (_, e) =>
            {
                e.Cancel = true;
                env.CancellationTokenSource.Cancel();
            };

            return unit;
        }));

    private static Consumer<RT, Option<WorkingStateInfo>, Unit> writeLine
        => from info in Proxy.awaiting<Option<WorkingStateInfo>>()
           from _1 in OutputInfo(info)
           from _2 in writeLine
           select unit;

    private static Eff<RT, Unit> OutputInfo(Option<WorkingStateInfo> info)
    {
        var msg = info.Match(
            Some: state => $"{state.CurrentCount}/{state.TaskCount}",
            None: () => "Waiting...");

        return
            from width in ConsoleExt<RT>.windowWidth
            from _1 in Console<RT>.write("\r" + new string(' ', width) + "\r")
            from _2 in Console<RT>.write(msg)
            select unit;
    }

    private static Eff<RT, Options> CommandLineOptions(string[] args)
        => SuccessEff(new Options()
        {
            DestinationDirectory = "original",
            MovedDirectory = "resized",
            SourceDirectory = "images",
            Width = 100,
            Height = 100,
            KeepAspectRatio = true,
            MaxConcurrent = 4,
            CheckDelay = 500 * ms,
        });
}
