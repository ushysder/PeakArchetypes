using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;

public static class Localization
{
	static readonly string CachePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "BepInEx", "cache", "PeakArchetypes_LocalizationCache.json");
	static readonly string RemoteUrl = "https://raw.githubusercontent.com/ushysder/PeakArchetypes/refs/heads/main/Localization/Localization.json";

	public static Dictionary<string, Dictionary<string, string>> Data { get; private set; } = [];

	public static string Get(string lang, string key)
	{
		if (Data.TryGetValue(lang, out var dict) && dict.TryGetValue(key, out var value))
			return value;
		return key; // fallback
	}

	/// <summary>
	/// Auto-detects the system's two-letter ISO language code (e.g., "en", "fr").
	/// </summary>
	public static string GetSystemLang()
	{
		var lang = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
		if (!Data.ContainsKey(lang)) lang = "en"; // fallback to English
		return lang;
	}

	/// <summary>
	/// Loads localization data, fetching from GitHub if cache is old or missing.
	/// </summary>
	public static void Init()
	{
		bool shouldFetch = true;

		if (File.Exists(CachePath))
		{
			try
			{
				var cacheObj = JsonConvert.DeserializeObject<LocalizationCache>(File.ReadAllText(CachePath));
				if (cacheObj != null)
				{
					if ((DateTime.UtcNow - cacheObj.LastUpdate).TotalHours < 24)
					{
						Data = cacheObj.Data;
						shouldFetch = false;
					}
				}
			}
			catch { }
		}

		if (shouldFetch)
		{
			if (TryFetchFromGitHub(out var json))
			{
				Data = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, string>>>(json) ?? [];

				var cacheObj = new LocalizationCache
				{
					LastUpdate = DateTime.UtcNow,
					Data = Data
				};

				File.WriteAllText(CachePath, JsonConvert.SerializeObject(cacheObj, Formatting.Indented));
			}
		}
	}

	static bool TryFetchFromGitHub(out string json)
	{
		try
		{
			using var client = new WebClient();
			client.Headers.Add("User-Agent", "PeakArchetypes");
			json = client.DownloadString(RemoteUrl);
			return true;
		}
		catch
		{
			json = null;
			return false;
		}
	}

	public class LocalizationCache
	{
		public Dictionary<string, Dictionary<string, string>> Data { get; set; }
		public DateTime LastUpdate { get; set; }
	}

}