using GoogleTranslateFreeApi;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Diagnostics;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation.Collections;

namespace CopyTranslation.ViewModels
{
    public sealed class MainPageViewModel : ReactiveObject
    {
        [ObservableAsProperty]
        public string Translated { get; set; }

        [Reactive]
        public string IsBusy { get; set; }

        public MainPageViewModel()
        {
            var translator = new GoogleTranslator();
            var zh = Language.ChineseSimplified;
            var en = Language.English;

            Observable.FromEventPattern<object>(h => Clipboard.ContentChanged += h, h => Clipboard.ContentChanged -= h)
                .SubscribeOnDispatcher()
                .Throttle(TimeSpan.FromMilliseconds(50))
                .ObserveOn(TaskPoolScheduler.Default)
                .SelectMany(_ => Observable.FromAsync(async () =>
                {
                    var req = new ValueSet();
                    req.Add("Op", "Clipboard");
                    return await App.Connection.SendMessageAsync(req);
                }))
                .Select(x => x.Message["Res"] as string)
                .Select(x => x.Trim())
                .Where(x => x != string.Empty)
                .DistinctUntilChanged()
                .Select(x => x.Replace("\r\n", " "))
                .SelectMany(x => Observable.FromAsync(async () => await translator.TranslateLiteAsync(x, en, zh)))
                .Select(x => x.MergedTranslation)
                .ObserveOnDispatcher()
                .Retry()
                .ToPropertyEx(this, x => x.Translated)
                .ThrownExceptions
                .Subscribe(e => { Debug.WriteLine($"Exception: {e.Message}"); })
                ;
        }
    }
}
