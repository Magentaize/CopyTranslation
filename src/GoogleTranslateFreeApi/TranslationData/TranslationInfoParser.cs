using System;
using System.Text.Json;

namespace GoogleTranslateFreeApi.TranslationData
{
    public abstract class TranslationInfoParser
  {
    internal abstract bool TryParseMemberAndAdd(string memberName, JsonElement parseInformation);
    internal abstract int ItemDataIndex { get; }
    ////////////////////////////////////////////////////////////////////////////////////////////
    //I've created a method, because the where: new() statement requires a public constructor///
    ////////////////////////////////////////////////////////////////////////////////////////////    
    internal static T Create<T>() where T : TranslationInfoParser
    {
      Type type = typeof(T);

      if (type == typeof(ExtraTranslations))
        return new ExtraTranslations() as T;
      if (type == typeof(Definitions))
        return new Definitions() as T;
      if (type == typeof(Synonyms))
        return new Synonyms() as T;

      throw new NotSupportedException("type");
    }
  }
}