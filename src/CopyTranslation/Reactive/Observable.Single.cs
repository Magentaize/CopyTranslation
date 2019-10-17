using JetBrains.Annotations;

namespace System.Reactive.Linq
{
    public static class ObservableExtension
    {
        public static IObservable<T> WhereAndElse<T>([NotNull]this IObservable<T> source, [NotNull]Func<T, bool> predicate, Action<T> action)
        {
            return Observable.Create<T>(o =>
            {
                return source.Subscribe(x =>
                {
                    if (predicate(x))
                        o.OnNext(x);
                    else
                        action?.Invoke(x);
                });
            });
        }

        public static IObservable<string> DistinctUntilChangedAndElse([NotNull]this IObservable<string> source, Action<string> action) 
        {
            return Observable.Create<string>(o =>
            {
                string last = string.Empty;

                return source.Subscribe(x =>
                {
                    if (last != x)
                    {
                        last = x;
                        o.OnNext(x);
                    }
                    else
                    {
                        action?.Invoke(x);
                    }
                });
            });
        }
    }
}
