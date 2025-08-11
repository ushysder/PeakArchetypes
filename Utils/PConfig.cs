using BepInEx.Configuration;
using KomiChallenge.Shared;

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
			"ClumsySettings", "InvertMinTime", Const.clumsy_InvertMinTime,
			$"Minimum time before inversion changes (seconds). [Min: {Const.clumsy_InvertMinTime_Min}, Max: {Const.clumsy_InvertMinTime_Max}]");

		clumsy_InvertMaxTime = cfg.Bind(
			"ClumsySettings", "InvertMaxTime", Const.clumsy_InvertMaxTime,
			$"Maximum time before inversion changes (seconds). Must be >= InvertMinTime. [Min: InvertMinTime, Max: {Const.clumsy_InvertMaxTime_Max}]");

		clumsy_ItemDropChancePercent = cfg.Bind(
			"ClumsySettings", "ItemDropChancePercent", Const.clumsy_ItemDropChancePercent,
			"Chance (in percent) to drop a random item when inversion changes. [0 = never, 100 = always]");

		drunk_maxFallInterval = cfg.Bind(
			"DrunkSettings", "MaxFallInterval", Const.drunk_maxFallInterval,
			$"Max interval between falls (in seconds). [Min: {Const.drunk_maxFallInterval_Min}, Max: {Const.drunk_maxFallInterval_Max}]");

		drunk_minFallInterval = cfg.Bind(
			"DrunkSettings", "MinFallInterval", Const.drunk_minFallInterval,
			$"Min interval between falls (in seconds). [Min: {Const.drunk_minFallInterval_Min}, Max: MaxFallInterval]");

		drunk_passOutDuration = cfg.Bind(
			"DrunkSettings", "PassOutDuration", Const.drunk_passOutDuration,
			$"Duration the player stays passed out (in seconds). [Min: {Const.drunk_passOutDuration_Min}, Max: {Const.drunk_passOutDuration_Max}]");

		drunk_timeToMaxDrunkness = cfg.Bind(
			"DrunkSettings", "TimeToMaxDrunkness", Const.drunk_timeToMaxDrunkness,
			$"Time to reach full drunkenness (in seconds). [Min: {Const.drunk_timeToMaxDrunkness_Min}, Max: {Const.drunk_timeToMaxDrunkness_Max}]");

		drugs_timeToFullPoison = cfg.Bind(
			"DrugsSettings", "TimeToFullPoison", Const.drugs_timeToFullPoison,
			$"Seconds it takes for Poison status to reach full overdose. [Min: {Const.drugs_timeToFullPoison_Min}, Max: {Const.drugs_timeToFullPoison_Max}]");

		narco_timeToFullDrowsy = cfg.Bind(
			"NarcolepticSettings", "TimeToFullDrowsy", Const.narco_timeToFullDrowsy,
			$"Seconds it takes for Drowsy status to reach full and cause pass out. [Min: {Const.narco_timeToFullDrowsy_Min}, Max: {Const.narco_timeToFullDrowsy_Max}]");

		narco_passOutDuration = cfg.Bind(
			"NarcolepticSettings", "PassOutDuration", Const.narco_passOutDuration,
			$"Duration the player stays passed out after max Drowsy is reached. [Min: {Const.narco_passOutDuration_Min}, Max: {Const.narco_passOutDuration_Max}]");

		oneEyed_targetInjuryPercent = cfg.Bind(
			"OneEyedSettings", "TargetInjuryPercent", Const.oneEyed_targetInjuryPercent,
			$"Percentage of injury to maintain for one-eyed effect. [Min: {Const.oneEyed_targetInjuryPercent_Min}, Max: {Const.oneEyed_targetInjuryPercent_Max}]");
	}
}