//using System;
using System;
using System.Collections.Generic;
using KomiChallenge.Comps;
using Photon.Voice.PUN;
using UnityEngine;
using Random = UnityEngine.Random;


namespace KomiChallenge.Scripts
{
    public class AssignRoles
    {

        public enum RoleType
        {
            blind,
            deaf,
            mute
        }

        public static List<Role> defaultTypes = new List<Role>();
        public static Dictionary<int, Role> players = new Dictionary<int, Role>();



        public static void RemoveDebuffs()
        {
            AudioListener.volume = 1;
            Character.localCharacter.GetComponent<PhotonVoiceView>().RecorderInUse.TransmitEnabled = true;
            Role plrRole = Character.localCharacter.GetComponent<Role>();
            if (plrRole != null)
            {
                Plugin.Destroy(plrRole.blackScreen);
            }

        }


        public static void AppendRoles()
        {

            defaultTypes.Add(new Role(
            "Blind",
            "Listen to the voices and remember you can still talk",
            RoleType.blind)
            );

            
            defaultTypes.Add(new Role(
            "Deaf",
            "People can hear you but you can't hear them.",
            RoleType.deaf)
            );


            defaultTypes.Add(new Role(
            "Mute",
            "Kill them all. No survivors.",
            RoleType.mute
            ));
            

        }

        // Split the players list based on how many available roleTypes there are.
        // Eg. If there are 3 roleTypes, the player list will assign roles to the
        // first 3 players, then ignore the rest and recall the function to do the
        // same thing until everyone has a role.
        // This'll make sure that every role is assigned and if there are extra
        // players, they will be given a role as well
        public static void SplitList(List<int> currentPlayers)
        {
            List<int> raw_newPlayers = new List<int>();
            List<Role> ogRoleTypes = new List<Role>();

            foreach (Role rl in defaultTypes)
            {
                ogRoleTypes.Add(rl);
                Plugin.SendLog($">>> Role: {rl.roleName}");
            }


            // Add 3 players to raw list 
            for (int i = 0; i < ogRoleTypes.Count; i++)
            {
                Plugin.SendLog($">>> Before before");
                if (i > currentPlayers.Count - 1) break;
                raw_newPlayers.Add(currentPlayers[i]);

                Plugin.SendLog($">>> Added {i}");
            }


            List<int> newPlayers = new List<int>();

            // Filter out duplicates
            foreach (int dupli in raw_newPlayers)
            {
                if (newPlayers.Contains(dupli)) continue;
                newPlayers.Add(dupli);
            }


            // Remove players stored in raw_newPlayers
            foreach (int plr in newPlayers)
            {
                if (currentPlayers.Contains(plr))
                {
                    currentPlayers.Remove(plr);
                }
            }

            // Assign Roles

            foreach (int p in newPlayers)
            {
                long raw_utcSecs = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                string utc_str = $"{raw_utcSecs}";
                string second_to_last_nums = utc_str.Substring(7, 2);
                int.TryParse(second_to_last_nums, out int seed);

                //int seed = (int)second_to_last_nums;
                Random.InitState(seed);

                Plugin.SendLog($">>> Rnd Seed: {seed}");

                int result = Random.Range(0, ogRoleTypes.Count);
                players.Add(p, ogRoleTypes[result]);

                Plugin.SendLog($">>> Rnd Num: {result}");

                ogRoleTypes.Remove(ogRoleTypes[result]);
            }

            if (currentPlayers.Count >= 1)
            {
                SplitList(currentPlayers);
            }

        }
    }
}
