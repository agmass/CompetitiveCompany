using System.Collections.Generic;
using System.Collections.ObjectModel;
using GameNetcodeStuff;
using HarmonyLib;

namespace CompetitiveCompany.Util {
    public class Team {

        public string teamName = "";
        public string teamColorCode = "";
        
        public int score = 0;
        public int playerCount = 0;
        public Collection<PlayerControllerB> playersinTeam = [];

        public Team(string name, string code) {
            teamName = name;
            teamColorCode = code;
        }

        public void joinTeam(PlayerControllerB pcb) {
            playersinTeam.Add(pcb);
        }
        public void leaveTeam(PlayerControllerB pcb) {
            playersinTeam.Remove(pcb);
        }
        
    }
}