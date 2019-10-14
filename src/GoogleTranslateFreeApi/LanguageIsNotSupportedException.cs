using System;

namespace GoogleTranslateFreeApi
{
    class LanguageIsNotSupportedException: Exception
	{
		public readonly Language Language;

		public LanguageIsNotSupportedException(Language language)
			:base("Language is not supported by GoogleTranslate:")
		{
			Language = language;
		}

		public override string Message => base.Message + " " + Language;
	}
}
