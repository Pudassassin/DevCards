using System;
using System.Collections.Generic;
using System.Text;

using UnityEngine;
using UnboundLib.Networking;
using UnboundLib;
using GearUpCards.Utils;
using UnboundLib.GameModes;
using System.Collections;
using System.Linq;
using ExitGames.Client.Photon.StructWrapping;
using static GearUpCards.Utils.CardUtils;

namespace GearUpCards.MonoBehaviours
{
    public class BountyDamageTracker : MonoBehaviour
    {
        // RPC Sync
        public static string RPCKey = GearUpCards.ModId + ":BountyScoreSync";

        // earn 1 point for dealing 1% of victim's Max Health in battle, up to this limit
        public static float totalPercentPointPerBattle = 1000.0f;
        public static float killingBlowPoint = 250.0f;

        // dict of dicts: BountyScores[playerSource][playerTarget] >> score;
        // Round >>> Match point, progression toward game winning
        // Point >>> Round point, progression toward earning match point
        public static Dictionary<int, Dictionary<int, float>> BountyRoundScores, BountyPointScores;
        public static List<int> teamIDs;
        public static Dictionary<int, Player> playerDict;

        // lastSourceIDs[victimPlayer][lastHitPlayer]
        public static Dictionary<int, int> lastSourceIDs;

        internal static bool hasSheriffBounty = false;
        internal static CardDrawTracker.ExtraCardDraw sheriffBounty = null;
        internal static int leadingTeamID = -1;

        internal static int[] winnerIDs;

        public void Awake()
        {
            GameModeManager.AddHook(GameModeHooks.HookGameStart, GameStartSetup);
            GameModeManager.AddHook(GameModeHooks.HookRoundStart, RoundStartSetup);
            GameModeManager.AddHook(GameModeHooks.HookPointStart, PointStartSetup);
            GameModeManager.AddHook(GameModeHooks.HookPointEnd, PointEndSetup);
            GameModeManager.AddHook(GameModeHooks.HookRoundEnd, RoundEndSetup);

        }

        public static IEnumerator GameStartSetup(IGameModeHandler gm)
        {
            // define current teams
            teamIDs = new List<int>();
            foreach (Player player in PlayerManager.instance.players)
            {
                if (!teamIDs.Contains(player.teamID))
                {
                    teamIDs.Add(player.teamID);
                }
            }

            // define all current players
            playerDict = new Dictionary<int, Player>();
            foreach (Player player in PlayerManager.instance.players)
            {
                playerDict.Add(player.playerID, player);
            }

            // prepare score boards (A)
            BountyRoundScores = new Dictionary<int, Dictionary<int, float>>();
            foreach (Player player in PlayerManager.instance.players)
            {
                BountyRoundScores.Add(player.playerID, new Dictionary<int, float>());
                foreach (Player player2 in PlayerManager.instance.players)
                {
                    BountyRoundScores[player.playerID].Add(player2.playerID, 0.0f);
                }
            }
            BountyRoundScores.Add(-1, new Dictionary<int, float>());
            foreach (Player player in PlayerManager.instance.players)
            {
                BountyRoundScores[-1][player.playerID] = 0.0f;
            }

            // prepare score boards (B)
            BountyPointScores = new Dictionary<int, Dictionary<int, float>>();
            foreach (Player player in PlayerManager.instance.players)
            {
                BountyPointScores.Add(player.playerID, new Dictionary<int, float>());
                foreach (Player player2 in PlayerManager.instance.players)
                {
                    BountyPointScores[player.playerID].Add(player2.playerID, 0.0f);
                }
            }
            BountyPointScores.Add(-1, new Dictionary<int, float>());
            foreach (Player player in PlayerManager.instance.players)
            {
                BountyPointScores[-1][player.playerID] = 0.0f;
            }

            // prepare last hit tracker
            lastSourceIDs = new Dictionary<int, int>();
            foreach (Player player in PlayerManager.instance.players)
            {
                lastSourceIDs[player.playerID] = -1;
            }

            // clean up and prepare future Sheriff's bounty
            leadingTeamID = -1;
            sheriffBounty = null;
            if (hasSheriffBounty)
            {
                foreach (Player gamePlayer in PlayerManager.instance.players)
                {
                    ModdingUtils.Extensions.CharacterStatModifiersExtension.GetAdditionalData(gamePlayer.data.stats).blacklistedCategories.Remove(GearCategory.uniqueCardSheriff);
                }
                hasSheriffBounty = false;
            }

            yield break;
        }

        // !! // translating damage dealt / killing blows toward scoring
        public static void ScoreFromDamage(int sourceID, int targetID, float damage, bool isLethal = true)
        {
            if (BountyPointScores == null)
            {
                return;
            }
            if (!ModdingUtils.Utils.PlayerStatus.PlayerAliveAndSimulated(playerDict[targetID]) || playerDict[targetID].data.healthHandler.isRespawning)
            {
                return;
            }

            // ignore all negative-heals >> hard to clarify the source
            if (sourceID < 0)
            {
                sourceID = lastSourceIDs[targetID];
            }
            if (targetID < 0 || damage <= 0.0f)
            {
                return;
            }

            float score = damage / playerDict[targetID].data.maxHealth * 100.0f;
            if (playerDict[targetID].data.health - damage < 0.0f)
            {
                // no overkill bonus, going thru revives is gonna be rewarding!
                score = Mathf.Max(playerDict[targetID].data.health, 0.0f) / playerDict[targetID].data.maxHealth * 100.0f;

                if (isLethal && sourceID >= 0)
                {
                    score += killingBlowPoint;
                }
            }

            BountyPointScores[sourceID][targetID] += score;
            if (sourceID >= 0)
            {
                lastSourceIDs[targetID] = sourceID;
            }
        }

        // !! // game hooks methods
        public static IEnumerator RoundStartSetup(IGameModeHandler gm)
        {
            List<Player> players = PlayerManager.instance.players;

            for (int i = 0; i < players.Count; i++)
            {
                for (int j = 0; j < players.Count; j++)
                {
                    BountyRoundScores[players[i].playerID][players[j].playerID] = 0.0f;
                }
            }

            // setup sheriff's bounty
            if (hasSheriffBounty)
            {
                leadingTeamID = -1;
                int leaderScore = -1;
                int leaderPlayerCount = 0;

                foreach (int teamId in PlayerManager.instance.players.Select(p => p.teamID).Distinct())
                {
                    int tempScore = gm.GetTeamScore(teamId).rounds;

                    if (tempScore > leaderScore)
                    {
                        leaderPlayerCount = 1;
                        leaderScore = tempScore;
                        leadingTeamID = teamId;
                    }
                    else if (tempScore == leaderScore)
                    {
                        leaderPlayerCount++;
                    }
                }

                if (leaderPlayerCount == PlayerManager.instance.players.Count)
                {
                    // no proper leader(s)
                    leadingTeamID = -1;
                }
            }

            yield break;
        }

        public static IEnumerator PointStartSetup(IGameModeHandler gm)
        {
            List<Player> players = PlayerManager.instance.players;

            foreach (Player player in players)
            {
                lastSourceIDs[player.playerID] = -1;
            }

            for (int i = 0; i < players.Count; i++)
            {
                for (int j = 0; j < players.Count; j++)
                {
                    BountyPointScores[players[i].playerID][players[j].playerID] = 0.0f;
                }
            }

            yield break;
        }

        public static IEnumerator PointEndSetup(IGameModeHandler gm)
        {
            List<Player> players = PlayerManager.instance.players;

            for (int i = 0; i < players.Count; i++)
            {
                Miscs.Log($"Player [{players[i].playerID}]'s scores:");

                for (int j = 0; j < players.Count; j++)
                {
                    NetworkingManager.RPC(typeof(BountyDamageTracker), "SyncBountyScore", players[i].playerID, players[j].playerID, BountyPointScores[players[i].playerID][players[j].playerID]);

                    Miscs.Log($"   VS   Player [{players[j].playerID}] : {BountyRoundScores[players[i].playerID][players[j].playerID]}");
                }
            }

            yield break;
        }

        public static IEnumerator RoundEndSetup(IGameModeHandler gm)
        {
            winnerIDs = gm.GetRoundWinners();
            
            // check bounty rewards and hand out consequences
            if (hasSheriffBounty && leadingTeamID >= 0)
            {
                List<Player> wantedPlayers = new List<Player>();
                foreach (Player player in PlayerManager.instance.players)
                {
                    if (player.teamID == leadingTeamID)
                    {
                        wantedPlayers.Add(player);
                    }
                }

                foreach (Player wantedPlayer in wantedPlayers)
                {
                    if (winnerIDs.Contains(wantedPlayer.playerID))
                    {
                        // skip unfulfilled manhunts
                        continue;
                    }

                    float tempScore = 0.0f;
                    Player rewardedPlayer = null;

                    // for each wanted player, check who is their prime nemesis
                    foreach (Player hunterPlayer in PlayerManager.instance.players)
                    {
                        if (hunterPlayer == wantedPlayer)
                        {
                            // no escaping manhunt!
                            continue;
                        }

                        if (BountyRoundScores[hunterPlayer.playerID][wantedPlayer.playerID] > tempScore)
                        {
                            tempScore = BountyRoundScores[hunterPlayer.playerID][wantedPlayer.playerID];
                            rewardedPlayer = hunterPlayer;
                        }
                    }

                    // hand out rewards or punishments
                    if (rewardedPlayer != null)
                    {
                        if (rewardedPlayer.teamID != wantedPlayer.teamID && tempScore >= 100.0f)
                        {
                            // proper reward
                            CardDrawTracker playerCardDraw = rewardedPlayer.gameObject.GetComponent<CardDrawTracker>();
                            playerCardDraw.QueueExtraDraw(sheriffBounty.Clone());
                        }
                        else 
                        {
                            // same team incidents
                            if (rewardedPlayer != wantedPlayer)
                            {
                                // pending traitor punishment
                            }
                            else
                            {
                                // pending suicidal punishment
                            }
                        }
                    }
                    
                }
            }

            yield break;
        }

        // receive and sum scores from all players in the game
        [UnboundRPC]
        public static void SyncBountyScore(int sourcePlayerID, int targetPlayerID, float bountyScore)
        {
            BountyRoundScores[sourcePlayerID][targetPlayerID] += bountyScore;
        }

    }
}
