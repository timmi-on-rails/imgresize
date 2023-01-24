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
        => await Main<Runtime>(args).RunUnit(Runtime.New());

    public static Aff<RT, Unit> Main<RT>(string[] args)
        where RT : struct, HasCancel<RT>, HasConsole<RT>, HasFile<RT>, HasDirectory<RT>
    {
        var opts = new Options()
        {
            DestinationDirectory = "original",
            MovedDirectory = "resized",
            SourceDirectory = "images",
            Width = 100,
            Height = 100,
            KeepAspectRatio = true,
            MaxConcurrent = 4,
            CheckDelay = 500 * ms,
        };


        //Console.CancelKeyPress += (o, e) =>
        //{
        //    e.Cancel = true;
        //    runtime.CancellationTokenSource.Cancel();
        //};

        //var producer = Processor.RunAsync<RT>(opts);

        //var effect = producer | writeLine<RT>();
        //return effect.RunEffect<RT, Unit>();

        return
            from __ in Console<RT>.writeLine("sad")
            from _ in (Processor.RunAsync<RT>(opts) | writeLine<RT>())
            select unit;


        //using var _ = statusObservable.Subscribe(workingState =>
        //{
        //    var msg = workingState.Match(
        //        Some: state => $"{state.CurrentCount}/{state.TaskCount}",
        //        None: () => "Waiting...");

        //    do { Console.Write("\b \b"); } while (Console.CursorLeft > 0);
        //    Console.Write(msg);
        //});

        //await statusObservable.LastOrDefaultAsync();
    }

    private static Consumer<RT, Fin<string>, Unit> writeLine<RT>()
        where RT : struct, HasCancel<RT>, HasConsole<RT>
        => from fin in Proxy.awaiting<Fin<string>>()
           from _ in Console<RT>.writeLine(fin.ToOption().IfNone("failed"))
           select unit;
}
