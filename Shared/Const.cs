using UnityEngine;

namespace KomiChallenge.Shared;

class Const
{
	#region ClumsySettings

	public const float clumsy_InvertMinTime = 10f; // Default 10 seconds
	public const float clumsy_InvertMinTime_Min = 1f; // Minimum 1 second
	public const float clumsy_InvertMinTime_Max = 60f; // Maximum 60 seconds

	public const float clumsy_InvertMaxTime = 30f; // Default 30 seconds
	public const float clumsy_InvertMaxTime_Max = 120f; // Maximum 120 seconds

	public const int clumsy_ItemDropChancePercent = 50; // Default 50% chance to drop an item
	public const int clumsy_ItemDropChancePercent_Min = 0; // Minimum 0% chance (never drops)
	public const int clumsy_ItemDropChancePercent_Max = 100; // Maximum 100% chance (always drops)

	#endregion ClumsySettings

	#region DrunkSettings

	public const float drunk_minFallInterval = 10f; // Default 10 seconds
	public const float drunk_minFallInterval_Min = 1f; // Minimum 1 second

	public const float drunk_maxFallInterval = 45f; // Default 45 seconds
	public const float drunk_maxFallInterval_Min = 1f; // Minimum 1 second
	public const float drunk_maxFallInterval_Max = 600f; // Maximum 600 seconds (10 minutes)
	
	public const float drunk_passOutDuration = 5f; // Default 5 seconds
	public const float drunk_passOutDuration_Min = 1f; // Minimum 1 second
	public const float drunk_passOutDuration_Max = 30f; // Maximum 30 seconds

	public const float drunk_timeToMaxDrunkness = 300f; // Default 300 seconds (5 minutes)
	public const float drunk_timeToMaxDrunkness_Min = 60f; // Minimum 60 seconds (1 minute)
	public const float drunk_timeToMaxDrunkness_Max = 1800f; // Maximum 1800 seconds (30 minutes)

	#endregion DrunkSettings

	#region DrugsSettings

	public const float drugs_timeToFullPoison = 900f; // 15 minutes
	public const float drugs_timeToFullPoison_Min = 600f; // 10 minutes
	public const float drugs_timeToFullPoison_Max = 2700f; // 45 minutes

	#endregion DrugsSettings

	#region NarcolepticSettings

	public const float narco_passOutDuration = 5f; // Default 5 seconds
	public const float narco_passOutDuration_Min = 1f; // Minimum 1 second
	public const float narco_passOutDuration_Max = 30f; // Maximum 30 seconds

	public const float narco_timeToFullDrowsy = 300f; // Default 300 seconds (5 minutes)
	public const float narco_timeToFullDrowsy_Min = 30f; // Minimum 30 seconds
	public const float narco_timeToFullDrowsy_Max = 600f; // Maximum 600 seconds (10 minutes)

	#endregion NarcolepticSettings

	#region OneEyedSettings

	public const float oneEyed_targetInjuryPercent = 50f; // Default 50% HP substracted
	public const float oneEyed_targetInjuryPercent_Min = 10f; // Minimum 10% HP substracted
	public const float oneEyed_targetInjuryPercent_Max = 90f; // Maximum 90% HP substracted

	#endregion OneEyedSettings

	#region Medic

	public const string medic_SkillIconBase64 = "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";
	
	public const float medic_HealCooldownTime = 900f; // default 15 minutes
	public const float medic_HealCooldownTime_Min = 0f;
	public const float medic_HealCooldownTime_Max = 3600f;

	public const float medic_HealRadius = 5f;
	public const float medic_HealRadius_Min = 0.1f;
	public const float medic_HealRadius_Max = 20f;

	public const float medic_HealAmountPercent = 30f;
	public const float medic_HealAmount_Min = 1f;
	public const float medic_HealAmount_Max = 100f;

	public const float medic_HoldDuration = 5f;
	public const float medic_HoldDuration_Min = 0.1f;
	public const float medic_HoldDuration_Max = 30f;

	public const KeyCode medic_HealKey = KeyCode.H;

	#endregion
}
