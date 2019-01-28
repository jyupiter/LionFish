using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Cryptography;
using System.Web;

namespace LionFishWeb.Utility
{
	public static class Encoding
	{


		public static string Base64Encode(string plainText)
		{
			var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
			return System.Convert.ToBase64String(plainTextBytes);
		}

		public static string Base64Decode(string base64EncodedData)
		{
			var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
			return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
		}
	}
	public static class ScopedReferenceMap
	{
		private const int Buffer = 32;

		/// <summary>
		/// Extension method to retrieve a public facing indirect value
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="value"></param>
		/// <returns></returns>
		public static string GetIndirectReference<T>(this T value)
		{
			//Get a converter to convert value to string
			var converter = TypeDescriptor.GetConverter(typeof(T));
			if (!converter.CanConvertTo(typeof(string)))
			{
				throw new ApplicationException("Can't convert value to string");
			}

			var directReference = converter.ConvertToString(value);
			return CreateOrAddMapping(directReference);
		}

		/// <summary>
		/// Extension method to retrieve the direct value from the user session
		/// if it doesn't exists, the session has ended or this is possibly an attack
		/// </summary>
		/// <param name="indirectReference"></param>
		/// <returns></returns>
		public static string GetDirectReference(this string indirectReference)
		{
			var map = HttpContext.Current.Session["RefMap"];
			if (map == null) throw new ApplicationException("Can't retrieve direct reference map");

			return ((Dictionary<string, string>)map)[indirectReference];
		}

		private static string CreateOrAddMapping(string directReference)
		{
			var indirectReference = GetUrlSaveValue();
			var map =
			   (Dictionary<string, string>)HttpContext.Current.Session["RefMap"] ??
						new Dictionary<string, string>();

			//If we have it, return it.
			if (map.ContainsKey(directReference)) return map[directReference];


			map.Add(directReference, indirectReference);
			map.Add(indirectReference, directReference);

			HttpContext.Current.Session["RefMap"] = map;
			return indirectReference;
		}

		private static string GetUrlSaveValue()
		{
			var csprng = new RNGCryptoServiceProvider();
			var buffer = new Byte[Buffer];

			//generate the random indirect value
			csprng.GetBytes(buffer);

			//base64 encode the random indirect value to a URL safe transmittable value
			return HttpServerUtility.UrlTokenEncode(buffer);
		}



	}
}