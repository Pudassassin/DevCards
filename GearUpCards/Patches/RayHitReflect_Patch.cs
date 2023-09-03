﻿using System.Collections;
using System.Collections.Generic;

using HarmonyLib;

using UnityEngine;
using UnboundLib;

using GearUpCards.MonoBehaviours;
using GearUpCards.Utils;

namespace GearUpCards.Patches
{
    [HarmonyPatch(typeof(RayHitReflect))]
    class RayHitReflect_Patch
    {
        private static float constDisplacement = 0.15f;
        private static float scaledDisplacement = 0.002f;

        [HarmonyPostfix]
        [HarmonyPriority(Priority.First)]
        [HarmonyPatch("DoHitEffect")]
        static void DoHitEffectPostFix(ref MoveTransform ___move, int ___reflects, HitInfo hit)
        {
            if (hit.transform == null && ___reflects > 0)
            {
                // Miscs.Log("void-normal: " + hit.normal);
                // Miscs.Log("void-pos   : " + hit.point);
                // Miscs.Log("position   : " + ___move.transform.position);
                ___move.transform.position += (Vector3)hit.normal * (___move.velocity.magnitude * scaledDisplacement + constDisplacement);
                // Miscs.Log("position2  : " + ___move.transform.position);
            }
        }
    }
}