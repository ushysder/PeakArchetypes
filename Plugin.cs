using System;
using System.Reflection;
using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Logging;
using HarmonyLib;
using KomiChallenge.Scripts;


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
        public static int localID;

        void Awake()
        {
            PatchAllStuff();
            AssignRoles.AppendRoles();

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



        public static bool FoundThisMod(string modID)
        {
            foreach (var plugin in Chainloader.PluginInfos)
            {
                var metadata = plugin.Value.Metadata;
                if (metadata.GUID.Equals(modID))
                {
                    return true;
                }
            }

            return false;
        }
    }
}