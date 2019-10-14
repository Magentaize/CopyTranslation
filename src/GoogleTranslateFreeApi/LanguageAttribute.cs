using System;
using System.Runtime.CompilerServices;

namespace GoogleTranslateFreeApi
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
	internal class LanguageAttribute: Attribute
	{
		public string Iso639 { get; }
		public string FullName { get; }
		public LanguageAttribute(string iso, [CallerMemberName] string fullName = "")
		{
			if (string.IsNullOrWhiteSpace(iso))
				throw new ArgumentException(nameof(iso));

			Iso639 = iso;
			FullName = fullName;
		}
	}
}
