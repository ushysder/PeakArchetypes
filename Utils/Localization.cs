using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.Json;

public static class Localization
{
	public static Dictionary<string, Dictionary<string, string>> Data = [];
	static readonly string cacheFile = Path.Combine(Environment.CurrentDirectory, "LocalizationCache.json");
	static readonly TimeSpan cacheDuration = TimeSpan.FromHours(24); // cache validity

	public static void LoadFromUrl(string url)
	{
		bool useCache = false;

		// Check if cache exists and is fresh
		if (File.Exists(cacheFile))
		{
			DateTime lastWrite = File.GetLastWriteTimeUtc(cacheFile);
			if ((DateTime.UtcNow - lastWrite) < cacheDuration)
			{
				useCache = true;
			}
		}

		if (!useCache)
		{
			try
			{
				using WebClient client = new();
				string json = client.DownloadString(url);

				// Save cache locally
				File.WriteAllText(cacheFile, json);

				Console.WriteLine("[RemoteLocalization] Fetched JSON from URL.");
				Data = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, string>>>(json);
				return;
			}
			catch (Exception e)
			{
				Console.WriteLine("[RemoteLocalization] Failed to fetch from URL: " + e.Message);
			}
		}

		// Load from cache if fetch failed or cache is still valid
		if (File.Exists(cacheFile))
		{
			Console.WriteLine("[RemoteLocalization] Loading from cache.");
			string cachedJson = File.ReadAllText(cacheFile);
			Data = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, string>>>(cachedJson);
		}
		else
		{
			Console.WriteLine("[RemoteLocalization] No cache available, empty localization.");
			Data = [];
		}
	}

	public static string Get(string lang, string key)
	{
		if (Data.TryGetValue(lang, out var dict) && dict.TryGetValue(key, out var value))
			return value;
		return key; // fallback if missing
	}
}