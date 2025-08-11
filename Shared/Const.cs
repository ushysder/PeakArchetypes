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
}
