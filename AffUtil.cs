namespace ImageResizer;

using LanguageExt;
using LanguageExt.Effects.Traits;
using System.Linq;

using static LanguageExt.Prelude;

public static class AffUtil
{
    // TODO limit concurrent tasks
    public static Eff<RT, IAsyncEnumerable<Fin<T>>> Merge<RT, T>(
       this Seq<Aff<RT, T>> list,
       int maxConcurrent)
        where RT : struct, HasCancel<RT>
    {
        return Eff<RT, IAsyncEnumerable<Fin<T>>>(rt =>
        {
            List<Task<Fin<T>>> tasks = new(list.Select(i => i.Run(rt).AsTask()));
            return enumerate(tasks);
        });

        static async IAsyncEnumerable<Fin<T>> enumerate(List<Task<Fin<T>>> tasks)
        {
            while (tasks.Count > 0)
            {
                var done = await Task.WhenAny(tasks);
                tasks.Remove(done);
                yield return done.Result;
            }
        }
    }
}
