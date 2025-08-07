using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using KomiChallenge.Scripts;
using KomiChallenge.Utils;

namespace KomiChallenge;

[BepInPlugin(modGUID, modName, modVersion)]
public class Plugin : BaseUnityPlugin
{
	public const string modGUID = "KomiChallenge.ushysder";
	public const string modName = "KomiChallenge";
	public const string modVersion = "0.2.0";
	readonly Harmony _harmony = new(modGUID);
	internal static ManualLogSource mls = BepInEx.Logging.Logger.CreateLogSource(modGUID);
	public static int localID;

	void Awake()
	{
		PConfig.AllConfigs(Config);
		PatchAllStuff();
		RoleManager.AppendRoles();
	}

	void PatchAllStuff() => _harmony.PatchAll(Assembly.GetExecutingAssembly());
}