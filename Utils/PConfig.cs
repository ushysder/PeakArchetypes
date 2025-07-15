
using BepInEx.Configuration;

namespace KomiChallenge.Utils
{
    public class PConfig
    {

        public static ConfigEntry<bool> enableDevLogs;


        public static void AllConfigs(ConfigFile cfg)
        {
            enableDevLogs = cfg.Bind("Misc", "Logging", false);
        }
    }
}