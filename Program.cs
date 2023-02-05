namespace ImageResizer;

using LanguageExt;
using LanguageExt.Effects.Traits;
using LanguageExt.Pipes;
using LanguageExt.Sys;
using LanguageExt.Sys.Live;
using LanguageExt.Sys.Traits;

using static LanguageExt.Prelude;

class Program
{
    private static async Task Main(string[] args)
        => await MainIO<Runtime>(args).RunUnit(Runtime.New());

    // TODO
    //Console.CancelKeyPress += (o, e) =>
    //{
    //    e.Cancel = true;
    //    runtime.CancellationTokenSource.Cancel();
    //};

    public static Aff<RT, Unit> MainIO<RT>(string[] args)
        where RT : struct, HasCancel<RT>, HasConsole<RT>, HasFile<RT>, HasDirectory<RT>, HasTime<RT>
        => from opts in CommandLineOptions<RT>(args)
           from _ in (Processor<RT>.Run(opts) | writeLine<RT>())
           select unit;

    private static Consumer<RT, Option<WorkingStateInfo>, Unit> writeLine<RT>()
        where RT : struct, HasCancel<RT>, HasConsole<RT>
        => from info in Proxy.awaiting<Option<WorkingStateInfo>>()
           from _1 in OutputInfo<RT>(info)
           from _2 in writeLine<RT>()
           select unit;

    private static Eff<RT, Unit> OutputInfo<RT>(Option<WorkingStateInfo> info)
        where RT : struct, HasConsole<RT>
    {
        var msg = info.Match(
            Some: state => $"{state.CurrentCount}/{state.TaskCount}",
            None: () => "Waiting...");

        return from _1 in Console<RT>.write(
            "\r" + new string(' ', 40 /* TODO missing Console window with in ConsoleIO */) + "\r")
               from _2 in Console<RT>.write(msg)
               select unit;
    }

    private static Eff<RT, Options> CommandLineOptions<RT>(string[] args)
        where RT : struct, HasCancel<RT>
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
