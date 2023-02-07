namespace ImageResizer.Runtime.Live;

public readonly struct ConsoleExtIO : Traits.ConsoleExtIO
{
    public readonly static Traits.ConsoleExtIO Default =
        new ConsoleExtIO();

    public int WindowWidth => Console.WindowWidth;

    public event ConsoleCancelEventHandler? CancelKeyPress
    {
        add => Console.CancelKeyPress += value;
        remove => Console.CancelKeyPress -= value;
    }
}
