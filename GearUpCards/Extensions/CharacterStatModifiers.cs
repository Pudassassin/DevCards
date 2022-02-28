using System;
using System.Runtime.CompilerServices;
using HarmonyLib;


namespace GearUpCards.Extensions
{
    // Add fields to CharacterStatModifiers
    // Using PCE's extension as coding reference

    public class CharacterStatModifiersGearData
    {
        public int hollowLifeStack;
        public int tacticalScannerStack;

        public CharacterStatModifiersGearData()
        {
            hollowLifeStack = 0;
            tacticalScannerStack = 0;
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
        }
    }
}
