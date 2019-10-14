using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text.Json;

namespace GoogleTranslateFreeApi.TranslationData
{
    [DataContract]
	public sealed class Synonyms: TranslationInfoParser
	{
		[DataMember] public string[] Noun { get; internal set; }
		[DataMember] public string[] Exclamation { get; internal set; }
		[DataMember] public string[] Adjective { get; internal set; }
		[DataMember] public string[] Verb { get; internal set; }
		[DataMember] public string[] Adverb { get; internal set; }
		[DataMember] public string[] Preposition { get; internal set; }
		[DataMember] public string[] Conjunction { get; internal set; }
		[DataMember] public string[] Pronoun { get; internal set; }

		internal Synonyms() { }

		public override string ToString()
		{
			string info = string.Empty;
			info += FormatOutput(Noun, nameof(Noun));
			info += FormatOutput(Verb, nameof(Verb));
			info += FormatOutput(Pronoun, nameof(Pronoun));
			info += FormatOutput(Adverb, nameof(Adverb));
			info += FormatOutput(Adjective, nameof(Adjective));
			info += FormatOutput(Conjunction, nameof(Conjunction));
			info += FormatOutput(Preposition, nameof(Preposition));
			info += FormatOutput(Exclamation, nameof(Exclamation));

			return info.TrimEnd();
		}

		private string FormatOutput(IEnumerable<string> partOfSpeechData, string partOfSpeechName)
		{
			if (partOfSpeechData == null)
				return String.Empty;

			return !partOfSpeechData.Any()
				? String.Empty
				: $"{partOfSpeechName}: {string.Join(", ", partOfSpeechData)} \n";
		}

		internal override bool TryParseMemberAndAdd(string memberName, JsonElement parseInformation)
		{
			var property = GetType().GetRuntimeProperty(memberName.ToCamelCase());
			if (property == null)
				return false;
			
			var synonyms = new List<string>();
            foreach (var synonymsSet in parseInformation.EnumerateArray())
                synonyms.AddRange(Array.ConvertAll(synonymsSet[0].EnumerateArray().ToArray(),
                    x => x.GetString()));

            property.SetMethod.Invoke(this, new object[] { synonyms.ToArray() });
			
			return true;
		}

		internal override int ItemDataIndex => 1;
	}
}
