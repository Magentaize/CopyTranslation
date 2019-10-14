﻿using System;

namespace GoogleTranslateFreeApi
{
    public class Proxy : System.Net.IWebProxy
	{
		public System.Net.ICredentials Credentials
		{
			get;
			set;
		}

		private readonly Uri _proxyUri;

		public Proxy(Uri proxyUri)
		{
			_proxyUri = proxyUri;
		}

		public Uri GetProxy(Uri destination)
		{
			return _proxyUri;
		}

		public bool IsBypassed(Uri host)
		{
			return false;
		}
	}
}
