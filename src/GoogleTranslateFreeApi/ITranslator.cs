using System;
using System.Net;
using System.Threading.Tasks;

namespace GoogleTranslateFreeApi
{
    public interface ITranslator
	{
		Task<TranslationResult> TranslateAsync(ITranslatable item);
		Task<TranslationResult> TranslateAsync(string text, Language from, Language to);
		Task<TranslationResult> TranslateLiteAsync(ITranslatable item);
		Task<TranslationResult> TranslateLiteAsync(string text, Language from, Language to);
		IWebProxy Proxy { get; }
		TimeSpan TimeOut { get; }
	}
}
