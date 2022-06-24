using HarmonyLib;

using UnityEngine;

using UnboundLib;

using GearUpCards.MonoBehaviours;

namespace GearUpCards.Patches
{
    [HarmonyPatch(typeof(HealthHandler))]
    class HealthHandler_Patch
    {
        [HarmonyPrefix]
        [HarmonyPriority(Priority.First)]
        [HarmonyPatch("Heal")]
        static void ApplyHealMultiplier(Player ___player, ref float healAmount)
        {
            float healMuliplier = 1.0f;

            TacticalScannerStatus scannerStatus = ___player.GetComponent<TacticalScannerStatus>();
            if (scannerStatus != null)
            {
                healMuliplier *= scannerStatus.GetHealMultiplier();
            }

            HollowLifeEffect hollowLifeEffect = ___player.GetComponent<HollowLifeEffect>();
            if (hollowLifeEffect != null)
            {
                healMuliplier *= hollowLifeEffect.GetHealMultiplier();
            }


            healAmount *= healMuliplier;
        }

        [HarmonyPrefix]
        [HarmonyPriority(Priority.First)]
        [HarmonyPatch("DoDamage")]
        static void ApplyDamageMultiplier(HealthHandler __instance, ref Vector2 damage, Player ___player)
        {
            float damageMuliplier = 1.0f;

            TacticalScannerStatus scannerStatus = ___player.GetComponent<TacticalScannerStatus>();
            if (scannerStatus != null)
            {
                damageMuliplier *= scannerStatus.GetDamageMultiplier();
            }


            damage *= damageMuliplier;
        }

        // [HarmonyPostfix]
        // [HarmonyPriority(Priority.First)]
        // [HarmonyPatch("CallTakeDamage")]
        // static void BleedEffect(HealthHandler __instance, Vector2 damage, Vector2 position, Player damagingPlayer, Player ___player)
        // {
        //     var bleed = ___player.data.stats.GetAdditionalData().Bleed;
        //     if (bleed > 0f)
        //     {
        //         __instance.TakeDamageOverTime(damage * bleed, position, 3f - 0.5f / 4f + bleed / 4f, 0.25f, Color.red, null, damagingPlayer, true);
        //     }
        // }

        //[HarmonyPrefix]
        //[HarmonyPatch("SomeMethod")]
        //static void MyMethodName()
        //{

        //}

        //[HarmonyPostfix]
        //[HarmonyPatch("SomeMethod")]
        //static void MyMethodName()
        //{

        //}
    }
}