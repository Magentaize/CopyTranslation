namespace System.Reactive.Linq
{
    public static class ObservableExtension
    {
        public static IObservable<string> DistinctUntilChanged(this IObservable<string> source, Action<string> action) 
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

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
                        action(x);
                    }
                });
            });
        }
    }
}
