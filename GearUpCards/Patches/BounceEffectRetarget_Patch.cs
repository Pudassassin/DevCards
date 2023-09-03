using System.Collections;
using System.Collections.Generic;

using HarmonyLib;

using UnityEngine;
using UnboundLib;

using GearUpCards.MonoBehaviours;
using GearUpCards.Utils;

namespace GearUpCards.Patches
{
    [HarmonyPatch(typeof(BounceEffectRetarget))]
    class BounceEffectRetarget_Patch
    {
        // private static float constDisplacement = 0.1f;
        // private static float scaledDisplacement = 0.1f;
        // private static float constDisplaceFromVoid = 1.0f;

        // [HarmonyPrefix]
        // [HarmonyPriority(Priority.First)]
        // [HarmonyPatch("DoBounce")]
        // static void DoBouncePreFix(ref MoveTransform ___move, ref HitInfo hit)
        // {
        //     // relocate it so it didn't bury itself inside stuffs and/or void; it's already bounced and reflected
        //     // ___move.transform.position += ___move.velocity.normalized * (___move.velocity.magnitude * scaledDisplacement + constDisplacement);
        // 
        //     if (hit.transform == null)
        //     {
        //         Miscs.Log("void-normal: " + hit.normal);
        //         Miscs.Log("void-pos   : " + hit.point);
        //         Miscs.Log("position   : " + ___move.transform.position);
        //         ___move.transform.position += (Vector3)hit.normal * (___move.velocity.magnitude * scaledDisplacement + constDisplaceFromVoid);
        //         Miscs.Log("position2  : " + ___move.transform.position);
        //     }
        // }

        [HarmonyPostfix]
        [HarmonyPriority(Priority.First)]
        [HarmonyPatch("FindTarget")]
        static void FindTargetRework(MoveTransform ___move, ref Player __result, ref HitInfo hit)
        {
            Miscs.Log("position3  : " + ___move.transform.position);
            List<Player> candidatePlayers = new List<Player>(PlayerManager.instance.players);
            Vector3 refPosition = ___move.transform.position + (Vector3)hit.normal * 0.25f;
            // Miscs.Log(refPosition);
        
            __result = null;
            float candidateDistance = Mathf.Infinity;
        
            if (hit.transform != null)
            {
                if (hit.transform.GetComponent<Player>())
                {
                    candidatePlayers.Remove(hit.transform.GetComponent<Player>());
                }
            }
        
            foreach (var player in candidatePlayers)
            {
                if (PlayerManager.instance.CanSeePlayer(refPosition, player).canSee &&
                    ModdingUtils.Utils.PlayerStatus.PlayerAliveAndSimulated(player) &&
                    !player.data.healthHandler.isRespawning)
                {
                    float distance = (refPosition - player.transform.position).magnitude;
                    if (__result == null)
                    {
                        __result = player;
                        candidateDistance = distance;
                    }
                    else if (distance < candidateDistance)
                    {
                        __result = player;
                        candidateDistance = distance;
                    }
                }
            }
        
            // return false;
        }
    }
}