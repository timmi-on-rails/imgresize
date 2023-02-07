namespace ImageResizer;

using LanguageExt;
using LanguageExt.Effects.Traits;
using System.Threading.Channels;

using static LanguageExt.Prelude;

public static class AffUtil
{
    public static Eff<RT, IAsyncEnumerable<Fin<T>>> Merge<RT, T>(
       this Seq<Aff<RT, T>> list,
       int maxConcurrent)
        where RT : struct, HasCancel<RT>
        => Eff<RT, IAsyncEnumerable<Fin<T>>>(env =>
        {
            var channel = Channel.CreateUnbounded<Fin<T>>();
            
            var opts = new ParallelOptions
            {
                MaxDegreeOfParallelism = maxConcurrent,
                CancellationToken = env.CancellationToken
            };

            var task = Parallel.ForEachAsync(list, opts, async (aff, cancellationToken) =>
            {
                var result = await aff.Run(env).ConfigureAwait(false);
                await channel.Writer.WriteAsync(result, cancellationToken).ConfigureAwait(false);
            }).ContinueWith(_ => channel.Writer.Complete());
       
            return go();

            async IAsyncEnumerable<Fin<T>> go()
            {
                await foreach (var t in channel.Reader.ReadAllAsync())
                {
                    yield return t;
                }

                await task;
            }
        });
}
