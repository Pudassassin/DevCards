using System.Collections;

using BepInEx;
using UnboundLib;
using UnboundLib.Cards;
using UnboundLib.GameModes;
using HarmonyLib;

using CardChoiceSpawnUniqueCardPatch.CustomCategories;

using UnityEngine;

using Jotunn;

using GearUpCards.Cards;
using static GearUpCards.Utils.CardUtils;
using UnboundLib.Utils;
using System.Linq;
using System.Collections.Generic;

using GearUpCards.Utils;

namespace GearUpCards
{
    // These are the mods required for our mod to work
    [BepInDependency("com.willis.rounds.unbound", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("pykess.rounds.plugins.moddingutils", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("pykess.rounds.plugins.cardchoicespawnuniquecardpatch", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("pykess.rounds.plugins.rayhitreflectpatch", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("com.willuwontu.rounds.evenspreadpatch", BepInDependency.DependencyFlags.HardDependency)]

    // Declares our mod to Bepin
    [BepInPlugin(ModId, ModName, Version)]

    // The game our mod is associated with
    [BepInProcess("Rounds.exe")]

    public class GearUpCards : BaseUnityPlugin
    {
        private const string ModId = "com.pudassassin.rounds.GearUpCards";
        private const string ModName = "GearUpCards";
        public const string Version = "0.1.68"; //build #140 / Release 0-2-0

        public const string ModInitials = "GearUP";

        public static GearUpCards Instance { get; private set; }

        void Awake()
        {
            // Use this to call any harmony patch files your mod may have
            var harmony = new Harmony(ModId);
            harmony.PatchAll();
        }
        void Start()
        {
            Instance = this;

            // Random idea cards
            CustomCard.BuildCard<HollowLifeCard>();
            CustomCard.BuildCard<ChompyBulletCard>();
            CustomCard.BuildCard<TacticalScannerCard>();
            CustomCard.BuildCard<SizeNormalizerCard>();

            // Tiberium card series
            CustomCard.BuildCard<TiberiumBulletCard>();

            // Unique Gun Spread
            CustomCard.BuildCard<ArcOfBulletsCard>();
            CustomCard.BuildCard<ParallelBulletsCard>();

            // Prototype cards
            CustomCard.BuildCard<ShieldBatteryCard>();

            // Unique Magick series (powerful on-block "spell" abilities)
            CustomCard.BuildCard<AntiBulletMagickCard>();

            // Orb Spells
            CustomCard.BuildCard<ObliterationOrbCard>();

            // Crystal card series

            // Passives + consolations cards
            CustomCard.BuildCard<GunPartsCard>();
            CustomCard.BuildCard<MedicalPartsCard>();

            // Spell Glyphs
            CustomCard.BuildCard<MagickFragmentsCard>();
            CustomCard.BuildCard<DivinationGlyphCard>();
            CustomCard.BuildCard<InfluenceGlyphCard>();
            CustomCard.BuildCard<GeometricGlyphCard>();
            CustomCard.BuildCard<PotencyGlyphCard>();

            // Adding hooks
            GameModeManager.AddHook(GameModeHooks.HookGameStart, GameStart);
            // GameModeManager.AddHook(GameModeHooks.HookRoundStart, CardPickEnd);
            GameModeManager.AddHook(GameModeHooks.HookPointEnd, PointEnd);
            // GameModeManager.AddHook(GameModeHooks.HookPointStart, PointStart);

            // make cards mutually exclusive
            this.ExecuteAfterSeconds(1.5f, () =>
            {
                if (CardManager.cards.Values.Any(card => card.cardInfo.cardName == "Size Difference"))
                {
                    CardInfo otherCard = CardManager.cards.Values.First(card => card.cardInfo.cardName == "Size Difference").cardInfo;

                    // CustomCardCategories.instance.MakeCardsExclusive(
                    //     otherCard,
                    //     CardManager.cards.Values.First(card => card.cardInfo.cardName == "Size Normalizer").cardInfo);

                    List<CardCategory> newList = otherCard.categories.ToList();
                    newList.Add(GearCategory.typeSizeMod);
                    otherCard.categories = newList.ToArray();
                }
                if (CardManager.cards.Values.Any(card => card.cardInfo.cardName == "Size Matters"))
                {
                    CardInfo otherCard = CardManager.cards.Values.First(card => card.cardInfo.cardName == "Size Matters").cardInfo;

                    // CustomCardCategories.instance.MakeCardsExclusive(
                    //     otherCard,
                    //     CardManager.cards.Values.First(card => card.cardInfo.cardName == "Size Normalizer").cardInfo);

                    List<CardCategory> newList = otherCard.categories.ToList();
                    newList.Add(GearCategory.typeSizeMod);
                    otherCard.categories = newList.ToArray();
                }
            });
        }

        // initial card blacklist/whitelist at game start

        IEnumerator GameStart(IGameModeHandler gm)
        {
            foreach (var player in PlayerManager.instance.players)
            {
                ModdingUtils.Extensions.CharacterStatModifiersExtension.GetAdditionalData(player.data.stats).blacklistedCategories.Clear();

                // if (!ModdingUtils.Extensions.CharacterStatModifiersExtension.GetAdditionalData(player.data.stats).blacklistedCategories.Contains(GearCategory.typeCrystalMod))
                // {
                //     ModdingUtils.Extensions.CharacterStatModifiersExtension.GetAdditionalData(player.data.stats).blacklistedCategories.Add(GearCategory.typeCrystalMod);
                // }
                // 
                // if (!ModdingUtils.Extensions.CharacterStatModifiersExtension.GetAdditionalData(player.data.stats).blacklistedCategories.Contains(GearCategory.typeCrystal))
                // {
                //     ModdingUtils.Extensions.CharacterStatModifiersExtension.GetAdditionalData(player.data.stats).blacklistedCategories.Add(GearCategory.typeCrystal);
                // }

                if (!ModdingUtils.Extensions.CharacterStatModifiersExtension.GetAdditionalData(player.data.stats).blacklistedCategories.Contains(GearCategory.tagSpellOnlyAugment))
                {
                    ModdingUtils.Extensions.CharacterStatModifiersExtension.GetAdditionalData(player.data.stats).blacklistedCategories.Add(GearCategory.tagSpellOnlyAugment);
                }

                // player.gameObject.GetOrAddComponent<CardHandResolveMono>();
            }
            yield break;
        }

        // I'd love to have this redundancy running but it can make thing worse, leave it for later lel
        IEnumerator CardPickEnd(IGameModeHandler gm)
        {
            // UnityEngine.Debug.Log($"[GearUp Main] CardPickEnd Call");

            yield return new WaitForSecondsRealtime(.25f);

            foreach (var player in PlayerManager.instance.players)
            {
                // UnityEngine.Debug.Log($"[GearUp Main] Resolving player[{player.playerID}]");
                StartCoroutine(PlayerCardResolver.Resolve(player));
                yield return new WaitForSecondsRealtime(.1f);
            }

            yield break;
        }

        IEnumerator PointEnd(IGameModeHandler gm)
        {
            MapUtils.ClearMapObjectsList();

            yield break;
        }

        // IEnumerator PointStart(IGameModeHandler gm)
        // {
        //     MapUtils.ClearMapObjectsList();
        // 
        //     yield break;
        // }

        // Assets loader
        public static readonly AssetBundle VFXBundle = Jotunn.Utils.AssetUtils.LoadAssetBundleFromResources("gearup_asset", typeof(GearUpCards).Assembly);
        public static readonly AssetBundle CardArtBundle = Jotunn.Utils.AssetUtils.LoadAssetBundleFromResources("gearup_cardarts", typeof(GearUpCards).Assembly);
    }
}
