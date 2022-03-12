using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using HarmonyLib;

using CardChoiceSpawnUniqueCardPatch.CustomCategories;

using GearUpCards.Utils;

namespace GearUpCards.Extensions
{
    public static class GearUpConstants
    {
        public enum ModType
        {
            disabled = -2,
            none = -1,

            crystal = 0,

            crystalNormal = 10,
            crystalTiberium,
            crystalPyro,
            crystalCryo,
            crystalMimic,

            sizeNormalize = 20,
            sizeShrinker,

            magickAntiBullet = 30,
            magickTimeDilution
        }
    }

    // Add fields to CharacterStatModifiers
    // Using PCE's extension as coding reference

    public class CharacterStatModifiersGearData
    {
        public int hollowLifeStack;
        public int tacticalScannerStack;

        public int magickFragmentStack;

        public GearUpConstants.ModType gunMod;
        public GearUpConstants.ModType blockMod;

        public GearUpConstants.ModType sizeMod;
        public GearUpConstants.ModType uniqueMagick;

        public float t_uniqueMagickCooldown;

        public CharacterStatModifiersGearData()
        {
            hollowLifeStack = 0;
            tacticalScannerStack = 0;

            magickFragmentStack = 0;

            gunMod = GearUpConstants.ModType.none;
            blockMod = GearUpConstants.ModType.none;

            sizeMod = GearUpConstants.ModType.none;
            uniqueMagick = GearUpConstants.ModType.none;
        }
    }

    public static class CharacterStatModifiersGearDataExtensions
    {
        public static readonly ConditionalWeakTable<CharacterStatModifiers, CharacterStatModifiersGearData> data =
            new ConditionalWeakTable<CharacterStatModifiers, CharacterStatModifiersGearData>();

        public static CharacterStatModifiersGearData GetGearData(this CharacterStatModifiers characterStat)
        {
            return data.GetOrCreateValue(characterStat);
        }

        public static void AddData(this CharacterStatModifiers characterStat, CharacterStatModifiersGearData value)
        {
            try
            {
                data.Add(characterStat, value);
            }
            catch (Exception) { }
        }
    }

    [HarmonyPatch(typeof(CharacterStatModifiers), "ResetStats")]
    class CharacterStatModifiersPatchResetStats
    {
        private static void Prefix(CharacterStatModifiers __instance)
        {
            __instance.GetGearData().hollowLifeStack = 0;
            __instance.GetGearData().tacticalScannerStack = 0;

            __instance.GetGearData().magickFragmentStack = 0;

            __instance.GetGearData().gunMod = GearUpConstants.ModType.none;
            __instance.GetGearData().blockMod = GearUpConstants.ModType.none;

            __instance.GetGearData().sizeMod = GearUpConstants.ModType.none;
            __instance.GetGearData().uniqueMagick = GearUpConstants.ModType.none;
        }
    }
}
