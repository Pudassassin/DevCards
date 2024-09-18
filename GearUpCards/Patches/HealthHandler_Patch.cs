using HarmonyLib;

using UnityEngine;

using UnboundLib;

using GearUpCards.MonoBehaviours;
using GearUpCards.Extensions;
// using GearUpCards.Extensions;

namespace GearUpCards.Patches
{
    [HarmonyPatch(typeof(HealthHandler))]
    class HealthHandler_Patch
    {
        static float GetHealMultiplier(Player ___player)
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

            LifeforceBlastStatus lifeforceBlastStatus = ___player.GetComponent<LifeforceBlastStatus>();
            if (lifeforceBlastStatus != null)
            {
                healMuliplier *= lifeforceBlastStatus.GetHealMultiplier();
            }

            CharacterStatModifiers stats = ___player.gameObject.GetComponent<CharacterStatModifiers>();

            int medicCheckupStack = stats.GetGearData().medicCheckupStack;
            healMuliplier *= Mathf.Pow(0.90f, medicCheckupStack);

            int medicalPartStack = stats.GetGearData().medicalPartStack;
            healMuliplier *= Mathf.Pow(1.10f, medicalPartStack);

            return healMuliplier;
        }

        static float GetDamageMultiplier(Player ___player)
        {
            float damageMuliplier = 1.0f;
            // CharacterStatModifiers stats = ___player.gameObject.GetComponent<CharacterStatModifiers>();

            TacticalScannerStatus scannerStatus = ___player.GetComponent<TacticalScannerStatus>();
            if (scannerStatus != null)
            {
                damageMuliplier *= scannerStatus.GetDamageMultiplier();
            }

            ArcaneSunStatus arcaneSunStatus = ___player.GetComponent<ArcaneSunStatus>();
            if (arcaneSunStatus != null)
            {
                damageMuliplier *= arcaneSunStatus.GetDamageMultiplier();
            }

            ArcaneSunEffect arcaneSunEffect = ___player.GetComponent<ArcaneSunEffect>();
            if (arcaneSunEffect != null)
            {
                damageMuliplier *= Mathf.Pow(1.15f, arcaneSunEffect.stackCount);
            }

            return damageMuliplier;
        }

        [HarmonyPrefix]
        [HarmonyPriority(Priority.First)]
        [HarmonyPatch("Heal")]
        static void ApplyHealMultiplier(Player ___player, ref float healAmount)
        {
            // positive healing
            if (healAmount > 0.0f)
            {
                healAmount *= GetHealMultiplier(___player);
            }
            // negative 'healing' -- magick damage, life drains, etc.
            else if (healAmount < 0.0f)
            {
                healAmount *= GetDamageMultiplier(___player);
            }
        }

        [HarmonyPrefix]
        [HarmonyPriority(Priority.Last)]
        [HarmonyPatch("Heal")]
        static bool NegativeHealRework(Player ___player, ref float healAmount)
        {
            if (healAmount >= 0.0f)
            {
                return true;
            }

            ___player.data.health += healAmount;
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPriority(Priority.First)]
        [HarmonyPatch("DoDamage")]
        static void ApplyDamageMultiplier(HealthHandler __instance, ref Vector2 damage, Player ___player)
        {
            damage *= GetDamageMultiplier(___player);
        }


        [HarmonyPrefix]
        [HarmonyPriority(Priority.Last)]
        [HarmonyPatch("DoDamage")]
        static void BountyScoreFromDamage(HealthHandler __instance, ref Vector2 damage, ref Player damagingPlayer, ref bool lethal, Player ___player)
        {
            int sourceID = -1;
            if (damagingPlayer != null)
            {
                sourceID = damagingPlayer.playerID;
            }

            int targetID = ___player.playerID;
            BountyDamageTracker.ScoreFromDamage(sourceID, targetID, damage.magnitude, lethal);
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