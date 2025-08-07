using BepInEx.Configuration;

namespace KomiChallenge.Utils;

public class PConfig
{
	#region DrunkSettings

	public static ConfigEntry<float> drunk_maxFallInterval;
	public static ConfigEntry<float> drunk_minFallInterval;
	public static ConfigEntry<float> drunk_passOutDuration;
	public static ConfigEntry<float> drunk_timeToMaxDrunkness;

	#endregion DrunkSettings

	#region ClumsySettings

	public static ConfigEntry<float> clumsy_InvertMinTime;
	public static ConfigEntry<float> clumsy_InvertMaxTime;

	#endregion ClumsySettings

	#region NarcolepticSettings

	public static ConfigEntry<float> narco_timeToFullDrowsy;
	public static ConfigEntry<float> narco_passOutDuration;

	#endregion NarcolepticSettings

	public static void AllConfigs(ConfigFile cfg)
	{
		drunk_maxFallInterval = cfg.Bind(
			"DrunkSettings", "MaxFallInterval", 45f,
			"Max interval between falls at lowest drunkenness (in seconds). [Min: 1, Max: 600]");

		drunk_minFallInterval = cfg.Bind(
			"DrunkSettings", "MinFallInterval", 10f,
			"Min interval between falls at peak drunkenness (in seconds). [Min: 1, Max: MaxFallInterval]");

		drunk_passOutDuration = cfg.Bind(
			"DrunkSettings", "PassOutDuration", 5f,
			"Duration the player stays passed out (in seconds). [Min: 1, Max: 30]");

		drunk_timeToMaxDrunkness = cfg.Bind(
			"DrunkSettings", "TimeToMaxDrunkness", 300f,
			"Time to reach full drunkenness (in seconds). [Min: 30, Max: 600]");

		clumsy_InvertMinTime = cfg.Bind(
			"ClumsySettings", "InvertMinTime", 10f,
			"Minimum time before inversion changes (seconds). [Min: 1, Max: 60]");

		clumsy_InvertMaxTime = cfg.Bind(
			"ClumsySettings", "InvertMaxTime", 30f,
			"Maximum time before inversion changes (seconds). Must be >= InvertMinTime. [Min: InvertMinTime, Max: 120]");

		narco_timeToFullDrowsy = cfg.Bind(
			"NarcolepticSettings", "TimeToFullDrowsy", 300f,
			"Seconds it takes for Drowsy status to reach full and cause pass out. [Min: 30, Max: 600]");

		narco_passOutDuration = cfg.Bind(
			"NarcolepticSettings", "PassOutDuration", 5f,
			"Duration the player stays passed out after max Drowsy is reached. [Min: 1, Max: 30]");
	}
}