using GoogleTranslateFreeApi;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Diagnostics;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Windows.Input;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation.Collections;
using Windows.UI.Xaml.Input;

namespace CopyTranslation.ViewModels
{
    public sealed class MainPageViewModel : ReactiveObject
    {
        [ObservableAsProperty]
        public string Translated { get; private set; }

        [ObservableAsProperty]
        public MainPageStatus Status { get; private set; }

        public ICommand StatusTappedCommand { get; }

        public Action SubTranslate;

        public MainPageViewModel()
        {
            var status = new Subject<MainPageStatus>();
            status.ObserveOnDispatcher()
                .ToPropertyEx(this, x => x.Status);

            StatusTappedCommand = ReactiveCommand.Create<TappedRoutedEventArgs>(_ =>
            {
                status.OnNext(Status != MainPageStatus.Pause ? MainPageStatus.Pause : MainPageStatus.Normal);
            });

            var translator = new GoogleTranslator();
            var zh = Language.ChineseSimplified;
            var en = Language.English;

            var lastError = false;

            var translate = Observable.FromEventPattern<object>(
                h => Clipboard.ContentChanged += h,
                h => Clipboard.ContentChanged -= h)
                .Merge(
                    Observable.Return<object>(null)
                        .Where(_ => !lastError)
                        .Do(_ => lastError = false)
                )
                .SubscribeOnDispatcher()
                .ObserveOn(TaskPoolScheduler.Default)
                .Throttle(TimeSpan.FromMilliseconds(50))
                .TakeUntil(status.Where(x => x == MainPageStatus.Pause))
                .RepeatWhen(_ =>
                    status.Where(_ => Status == MainPageStatus.Pause)
                        .Where(x => x == MainPageStatus.Normal)
                        .Do(_ => lastError = false)
                )
                .Do(_ => status.OnNext(MainPageStatus.Busy))
                .SelectMany(_ =>
                    Observable.FromAsync(async () =>
                    {
                        var req = new ValueSet();
                        req.Add("Op", "Clipboard");
                        var res = await App.Connection?.SendMessageAsync(req);
                        return (res.Message["Res"] as string).Trim();
                    })
                    .Catch(Observable.Return(string.Empty))
                )
                .WhereAndElse(x => x != string.Empty && char.IsLetter(x[0]), _ => status.OnNext(MainPageStatus.Normal))
                .DistinctUntilChangedAndElse(_ => status.OnNext(MainPageStatus.Normal))
                .Select(x => x.Replace("\r\n", " "))
                .Select(x => Observable.FromAsync(async () => await translator.TranslateLiteAsync(x, en, zh)))
                .Switch()
                .Select(x => x.MergedTranslation)
                .Do(_ => status.OnNext(MainPageStatus.Normal))
                ;

            SubTranslate = () =>
            {
                translate.ObserveOnDispatcher()
                    .ToPropertyEx(this, x => x.Translated)
                    .ThrownExceptions
                    .Subscribe(e =>
                    {
                        status.OnNext(MainPageStatus.Failed);
                        Debug.WriteLine($"Exception: {e.Message}");
                        lastError = true;

                        SubTranslate();
                    });
            };
        }
    }

    public enum MainPageStatus
    {
        Normal, 
        Failed,
        Busy,
        Successul,
        Pause,
    }
}
