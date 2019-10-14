using System;

namespace GoogleTranslateFreeApi
{
    class ExternalKeyParseException : Exception
	{
		public ExternalKeyParseException()
			:this("External key parse failed") { }

		public ExternalKeyParseException(string message)
			:base(message) { }
	}
}
