using ImageResizer.Runtime.Traits;
using LanguageExt;
using LanguageExt.Effects.Traits;
using LanguageExt.Sys.Traits;
using System.Text;

using static LanguageExt.Prelude;

namespace ImageResizer.Runtime.Live;

public readonly struct LiveRuntime :
    HasCancel<LiveRuntime>,
    HasConsole<LiveRuntime>,
    HasConsoleExt<LiveRuntime>,
    HasFile<LiveRuntime>,
    HasFileMove<LiveRuntime>,
    HasEncoding<LiveRuntime>,
    HasTextRead<LiveRuntime>,
    HasTime<LiveRuntime>,
    HasEnvironment<LiveRuntime>,
    HasDirectory<LiveRuntime>
{
    readonly RuntimeEnv env;

    /// <summary>
    /// Constructor
    /// </summary>
    LiveRuntime(RuntimeEnv env) =>
        this.env = env;


    /// <summary>
    /// Configuration environment accessor
    /// </summary>
    public RuntimeEnv Env =>
        env ?? throw new InvalidOperationException("Runtime Env not set.  Perhaps because of using default(Runtime) or new Runtime() rather than Runtime.New()");

    /// <summary>
    /// Constructor function
    /// </summary>
    public static LiveRuntime New() =>
        new LiveRuntime(new RuntimeEnv(new CancellationTokenSource(), Encoding.Default));

    /// <summary>
    /// Constructor function
    /// </summary>
    /// <param name="source">Cancellation token source</param>
    public static LiveRuntime New(CancellationTokenSource source) =>
        new LiveRuntime(new RuntimeEnv(source, Encoding.Default));

    /// <summary>
    /// Constructor function
    /// </summary>
    /// <param name="encoding">Text encoding</param>
    public static LiveRuntime New(Encoding encoding) =>
        new LiveRuntime(new RuntimeEnv(new CancellationTokenSource(), encoding));

    /// <summary>
    /// Constructor function
    /// </summary>
    /// <param name="encoding">Text encoding</param>
    /// <param name="source">Cancellation token source</param>
    public static LiveRuntime New(Encoding encoding, CancellationTokenSource source) =>
        new LiveRuntime(new RuntimeEnv(source, encoding));

    /// <summary>
    /// Create a new Runtime with a fresh cancellation token
    /// </summary>
    /// <remarks>Used by localCancel to create new cancellation context for its sub-environment</remarks>
    /// <returns>New runtime</returns>
    public LiveRuntime LocalCancel =>
        new LiveRuntime(new RuntimeEnv(new CancellationTokenSource(), Env.Encoding));

    /// <summary>
    /// Direct access to cancellation token
    /// </summary>
    public CancellationToken CancellationToken =>
        Env.Token;

    /// <summary>
    /// Directly access the cancellation token source
    /// </summary>
    /// <returns>CancellationTokenSource</returns>
    public CancellationTokenSource CancellationTokenSource =>
        Env.Source;

    /// <summary>
    /// Get encoding
    /// </summary>
    /// <returns></returns>
    public Encoding Encoding =>
        Env.Encoding;

    /// <summary>
    /// Access the console environment
    /// </summary>
    /// <returns>Console environment</returns>
    public Eff<LiveRuntime, ConsoleIO> ConsoleEff =>
        SuccessEff(LanguageExt.Sys.Live.ConsoleIO.Default);

    public Eff<LiveRuntime, Traits.ConsoleExtIO> ConsoleExtEff =>
        SuccessEff(ConsoleExtIO.Default);

    /// <summary>
    /// Access the file environment
    /// </summary>
    /// <returns>File environment</returns>
    public Eff<LiveRuntime, FileIO> FileEff =>
        SuccessEff(LanguageExt.Sys.Live.FileIO.Default);

    public Eff<LiveRuntime, Traits.FileMoveIO> FileMoveEff =>
        SuccessEff(FileMoveIO.Default);

    /// <summary>
    /// Access the directory environment
    /// </summary>
    /// <returns>Directory environment</returns>
    public Eff<LiveRuntime, DirectoryIO> DirectoryEff =>
        SuccessEff(LanguageExt.Sys.Live.DirectoryIO.Default);

    /// <summary>
    /// Access the TextReader environment
    /// </summary>
    /// <returns>TextReader environment</returns>
    public Eff<LiveRuntime, TextReadIO> TextReadEff =>
        SuccessEff(LanguageExt.Sys.Live.TextReadIO.Default);

    /// <summary>
    /// Access the time environment
    /// </summary>
    /// <returns>Time environment</returns>
    public Eff<LiveRuntime, TimeIO> TimeEff =>
        SuccessEff(LanguageExt.Sys.Live.TimeIO.Default);

    /// <summary>
    /// Access the operating-system environment
    /// </summary>
    /// <returns>Operating-system environment environment</returns>
    public Eff<LiveRuntime, EnvironmentIO> EnvironmentEff =>
        SuccessEff(LanguageExt.Sys.Live.EnvironmentIO.Default);
}

public class RuntimeEnv
{
    public readonly CancellationTokenSource Source;
    public readonly CancellationToken Token;
    public readonly Encoding Encoding;

    public RuntimeEnv(CancellationTokenSource source, CancellationToken token, Encoding encoding)
    {
        Source = source;
        Token = token;
        Encoding = encoding;
    }

    public RuntimeEnv(CancellationTokenSource source, Encoding encoding) : this(source, source.Token, encoding)
    {
    }
}