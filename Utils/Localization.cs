using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;

public static class Localization
{
	static readonly string CachePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "LocalizationCache.json");
	static readonly string TimestampPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "LocalizationCache.timestamp");
	static readonly string RemoteUrl = "https://raw.githubusercontent.com/ushysder/PeakArchetypes/refs/heads/dev/Localization/Localization.json";

	public static Dictionary<string, Dictionary<string, string>> Data { get; private set; } = [];

	/// <summary>
	/// Loads localization data, fetching from GitHub if cache is old or missing.
	/// </summary>
	public static void Init()
	{
		bool shouldFetch = true;

		// Check timestamp to see if cache is still fresh
		if (File.Exists(CachePath) && File.Exists(TimestampPath))
		{
			try
			{
				var lastUpdate = DateTime.Parse(File.ReadAllText(TimestampPath));
				if ((DateTime.UtcNow - lastUpdate).TotalHours < 24)
				{
					shouldFetch = false; // Cache is still fresh
				}
			}
			catch { /* Ignore and force fetch */ }
		}

		if (shouldFetch)
		{
			if (TryFetchFromGitHub(out var json))
			{
				LoadJson(json);
				File.WriteAllText(CachePath, json);
				File.WriteAllText(TimestampPath, DateTime.UtcNow.ToString("o"));
				return;
			}
		}

		// If we can't fetch, load from cache
		if (File.Exists(CachePath))
		{
			var json = File.ReadAllText(CachePath);
			LoadJson(json);
		}
		else
		{
			Data.Clear();
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

	static void LoadJson(string json)
	{
		try
		{
			Data = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, string>>>(json);
		}
		catch
		{
			Data = [];
		}
	}

	/// <summary>
	/// Get a localized string for the current system language.
	/// </summary>
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
}