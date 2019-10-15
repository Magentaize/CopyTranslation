using GoogleTranslateFreeApi;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Diagnostics;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using Windows.ApplicationModel.DataTransfer;
using Windows.UI.Popups;

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
                .Throttle(TimeSpan.FromMilliseconds(200))
                .ObserveOnDispatcher()
                .Select(_ => Clipboard.GetContent())
                .Where(x => x.Contains(StandardDataFormats.Text))
                .SelectMany(x => Observable.FromAsync(() => x.GetTextAsync().AsTask()))
                .DistinctUntilChanged()
                .ObserveOn(TaskPoolScheduler.Default)
                .Select(x => x.Replace("\r\n", " "))
                .SelectMany(x => Observable.FromAsync(() => translator.TranslateLiteAsync(x, en, zh)))
                .Select(x => x.MergedTranslation)
                .ObserveOnDispatcher()
                .ToPropertyEx(this, x => x.Translated)
                .ThrownExceptions
                .Subscribe(e => { Debugger.Break(); })
                ;
        }
    }
}
