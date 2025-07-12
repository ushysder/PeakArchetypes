using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;


namespace KomiChallenge
{

    [BepInPlugin(modGUID, modName, modVersion)]
    public class Plugin : BaseUnityPlugin
    {
        public const string modGUID = "KomiChallenge.zeeblo.dev";
        public const string modName = "KomiChallenge";
        public const string modVersion = "0.1.0";
        private readonly Harmony _harmony = new(modGUID);
        internal static ManualLogSource mls = BepInEx.Logging.Logger.CreateLogSource(modGUID);
        public static ulong localID;

        void Awake()
        {
            PatchAllStuff();

        }

        public static void ResetAllVariables()
        {

        }


        private void PatchAllStuff()
        {
            _harmony.PatchAll(Assembly.GetExecutingAssembly());
        }

        public static void SendLog(string msg)
        {
            mls.LogInfo(msg);
        }
    }
}