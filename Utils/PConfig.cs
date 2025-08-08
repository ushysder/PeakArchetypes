using BepInEx.Configuration;

namespace KomiChallenge.Utils;

public class PConfig
{
	#region ClumsySettings

	public static ConfigEntry<float> clumsy_InvertMaxTime;
	public static ConfigEntry<float> clumsy_InvertMinTime;
	public static ConfigEntry<int> clumsy_ItemDropChancePercent;

	#endregion ClumsySettings

	#region DrunkSettings

	public static ConfigEntry<float> drunk_maxFallInterval;
	public static ConfigEntry<float> drunk_minFallInterval;
	public static ConfigEntry<float> drunk_passOutDuration;
	public static ConfigEntry<float> drunk_timeToMaxDrunkness;

	#endregion DrunkSettings

	#region DrugsSettings

	public static ConfigEntry<float> drugs_timeToFullPoison;

	#endregion DrugsSettings

	#region NarcolepticSettings

	public static ConfigEntry<float> narco_passOutDuration;
	public static ConfigEntry<float> narco_timeToFullDrowsy;

	#endregion NarcolepticSettings

	#region OneEyedSettings

	public static ConfigEntry<float> oneEyed_targetInjuryPercent;

	#endregion OneEyedSettings

	public static void AllConfigs(ConfigFile cfg)
	{
		clumsy_InvertMinTime = cfg.Bind(
			"ClumsySettings", "InvertMinTime", 10f,
			"Minimum time before inversion changes (seconds). [Min: 1, Max: 60]");

		clumsy_InvertMaxTime = cfg.Bind(
			"ClumsySettings", "InvertMaxTime", 30f,
			"Maximum time before inversion changes (seconds). Must be >= InvertMinTime. [Min: InvertMinTime, Max: 120]");

		clumsy_ItemDropChancePercent = cfg.Bind(
			"ClumsySettings", "ItemDropChancePercent", 50,
			"Chance (in percent) to drop a random item when inversion changes. [0 = never, 100 = always]");

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

		drugs_timeToFullPoison = cfg.Bind(
			"DrugsSettings", "TimeToFullPoison", 900f,
			"Seconds it takes for Poison status to reach full overdose (poison = 1). [Min: 600, Max: 1800]");

		narco_timeToFullDrowsy = cfg.Bind(
			"NarcolepticSettings", "TimeToFullDrowsy", 300f,
			"Seconds it takes for Drowsy status to reach full and cause pass out. [Min: 30, Max: 600]");

		narco_passOutDuration = cfg.Bind(
			"NarcolepticSettings", "PassOutDuration", 5f,
			"Duration the player stays passed out after max Drowsy is reached. [Min: 1, Max: 30]");

		oneEyed_targetInjuryPercent = cfg.Bind(
			"OneEyedSettings", "TargetInjuryPercent", 50f,
			"Percentage of injury to maintain for one-eyed effect. [Min: 10, Max: 90]");
	}
}