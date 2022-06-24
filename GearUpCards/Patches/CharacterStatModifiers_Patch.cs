using HarmonyLib;

using UnityEngine;

using UnboundLib;

using GearUpCards.MonoBehaviours;
using GearUpCards.Extensions;

namespace GearUpCards.Patches
{
    [HarmonyPatch(typeof(CharacterStatModifiers))]
    class CharacterStatModifiers_Patch
    {
        [HarmonyPostfix]
        [HarmonyPriority(Priority.Last)]
        [HarmonyPatch("ConfigureMassAndSize")]
        static void ApplySizeAdjustment(CharacterStatModifiers __instance)
        {
            Transform playerTransform = __instance.gameObject.transform;

            switch (__instance.GetGearData().sizeMod)
            {
                case GearUpConstants.ModType.sizeNormalize:
                    playerTransform.localScale = Vector3.one * 1.2f;
                    break;
                case GearUpConstants.ModType.sizeShrinker:
                    break;
                default:
                    break;
            }
        }
    }
}