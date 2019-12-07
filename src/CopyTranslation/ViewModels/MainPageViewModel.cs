using GoogleTranslateFreeApi;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Diagnostics;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Windows.Input;
using Windows.ApplicationModel;
using Windows.ApplicationModel.AppService;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Foundation.Metadata;
using Windows.UI.Input;
using Windows.UI.Popups;
using Windows.UI.Xaml.Input;

namespace CopyTranslation.ViewModels
{
    public sealed class MainPageViewModel : ReactiveObject, IActivatableViewModel
    {
        [ObservableAsProperty]
        public string Translated { get; private set; }

        [ObservableAsProperty]
        public MainPageStatus Status { get; private set; }

        [ObservableAsProperty]
        public double TranslatedTextFontSize { get; private set; }

        public ICommand StatusTappedCommand { get; }

        public ViewModelActivator Activator { get; } = new ViewModelActivator();

        public ISubject<Unit> TranslatedTextFontSizePitchStart { get; } = new Subject<Unit>();
        public ISubject<Unit> TranslatedTextFontSizePitchCompleted { get; } = new Subject<Unit>();
        public ISubject<double> TranslatedTextFontSizePitch { get; } = new Subject<double>();
        public ISubject<PointerPointProperties> TranslatedTextFontSizeWheel { get; } = new Subject<PointerPointProperties>();

        public MainPageViewModel()
        {
            var status = new Subject<MainPageStatus>();
            status.ObserveOnDispatcher()
                .ToPropertyEx(this, x => x.Status);

            BuildTranslatedTextFontSizeProcessor();

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

            // workaround for use unassigned variable 
            Action subTranslate = () => throw new NotImplementedException();
            subTranslate = () =>
            {
                translate.ObserveOnDispatcher()
                    .ToPropertyEx(this, x => x.Translated)
                    .ThrownExceptions
                    .Subscribe(e =>
                    {
                        status.OnNext(MainPageStatus.Failed);
                        Debug.WriteLine($"Exception: {e.Message}");
                        lastError = true;

                        subTranslate();
                    });
            };

            this.WhenActivated(async (CompositeDisposable d) =>
            {
                if (ApiInformation.IsApiContractPresent("Windows.ApplicationModel.FullTrustAppContract", 1, 0))
                {
                    Observable.FromEventPattern<AppServiceTriggerDetails>(
                        h => App.AppServiceConnected += h,
                        h => App.AppServiceConnected -= h)
                    .Subscribe(x =>
                    {
                        subTranslate();
                    });

                    Observable.FromEventPattern(
                        h => App.AppServiceDisconnected += h,
                        h => App.AppServiceDisconnected -= h)
                    .ObserveOnDispatcher()
                    .Subscribe(async x =>
                    {
                        var dlg = new MessageDialog("AppService has disconnected.");
                        await dlg.ShowAsync();
                    });

                    await LaunchFullTrustAsync();
                }
            });
        }

        private void BuildTranslatedTextFontSizeProcessor()
        {
            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;

            // TranslatedText FontSize
            const string FONTSIZE_STR = nameof(TranslatedTextFontSize);
            double defaultFontSize;

            if (localSettings.Values[FONTSIZE_STR] == null)
            {
                localSettings.Values[FONTSIZE_STR] = 36d;
                defaultFontSize = 36d;
            }
            else
            {
                defaultFontSize = (double)localSettings.Values[FONTSIZE_STR];
            }

            const double FONTSIZE_DELTA = 2d;

            var wheel = TranslatedTextFontSizeWheel
                .Select(x => x.MouseWheelDelta)
                .ObserveOn(TaskPoolScheduler.Default)
                .Select(Math.Sign);

            var datum = 1d;
            var pitch = TranslatedTextFontSizePitch
                .SkipUntil(TranslatedTextFontSizePitchStart.Do(_ => datum = 1d))
                .TakeUntil(TranslatedTextFontSizePitchCompleted)
                .Repeat()
                .ObserveOn(TaskPoolScheduler.Default)
                .Sample(TimeSpan.FromSeconds(0.1))
                .Select(x =>
                {
                    var sub = x - datum;
                    if (Math.Abs(sub) >= 0.005)
                    {
                        datum = x;
                        return Math.Sign(sub);
                    }
                    return 0;
                })
                .Where(x => x != 0);

            wheel.Merge(pitch)
                .Select(x =>
                {
                    if (x == 1) return TranslatedTextFontSize + FONTSIZE_DELTA;
                    else if (x == -1) return TranslatedTextFontSize - FONTSIZE_DELTA;
                    else return TranslatedTextFontSize;
                })
                .Do(x => localSettings.Values[FONTSIZE_STR] = x)
                .ObserveOnDispatcher()
                .ToPropertyEx(this, x => x.TranslatedTextFontSize, defaultFontSize);
        }

        private IAsyncAction LaunchFullTrustAsync()
        {
            return FullTrustProcessLauncher.LaunchFullTrustProcessForCurrentAppAsync();
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
