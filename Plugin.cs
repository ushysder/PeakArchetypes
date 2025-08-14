using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using PeakArchetypes.Scripts;
using PeakArchetypes.Utils;

namespace PeakArchetypes;

[BepInPlugin(modGUID, modName, modVersion)]
public class Plugin : BaseUnityPlugin
{
	public const string modGUID = "ushysder.PeakArchetypes";
	public const string modName = "PeakArchetypes";
	public const string modVersion = "0.5.1";
	internal static ManualLogSource mls = BepInEx.Logging.Logger.CreateLogSource(modGUID);
	public static int localID;
	Harmony _harmony;

	void Awake()
	{
		PConfig.AllConfigs(Config);
		mls.LogInfo($"[{modName}] Plugin initializing...");
		_harmony = new Harmony(modGUID);
		_harmony.PatchAll(Assembly.GetExecutingAssembly());
		RoleManager.AppendRoles();
		mls.LogInfo($"[{modName}] Plugin initialized.");
	}
	
	void OnDestroy()
	{
		_harmony.UnpatchSelf();
		mls.LogInfo($"[{modName}] Plugin unloaded.");
	}
}