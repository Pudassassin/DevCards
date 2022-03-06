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

namespace GearUpCards
{
    // These are the mods required for our mod to work
    [BepInDependency("com.willis.rounds.unbound", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("pykess.rounds.plugins.moddingutils", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("pykess.rounds.plugins.cardchoicespawnuniquecardpatch", BepInDependency.DependencyFlags.HardDependency)]

    // Declares our mod to Bepin
    [BepInPlugin(ModId, ModName, Version)]

    // The game our mod is associated with
    [BepInProcess("Rounds.exe")]

    public class GearUpCards : BaseUnityPlugin
    {
        private const string ModId = "com.pudassassin.rounds.GearUpCards";
        private const string ModName = "GearUpCards";
        public const string Version = "0.0.34";

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
            CustomCard.BuildCard<TacticalScannerCard>(TacticalScannerCard.callback);
            CustomCard.BuildCard<SizeNormalizerCard>(SizeNormalizerCard.callback);

            // Crystal card series

            // Passives + consolations cards
            CustomCard.BuildCard<GunPartsCard>(GunPartsCard.callback);
            CustomCard.BuildCard<MedicalPartsCard>(MedicalPartsCard.callback);

            // Hooks
            GameModeManager.AddHook(GameModeHooks.HookGameStart, GameStart);
        }

        IEnumerator GameStart(IGameModeHandler gm)
        {
            foreach (var player in PlayerManager.instance.players)
            {
                if (!ModdingUtils.Extensions.CharacterStatModifiersExtension.GetAdditionalData(player.data.stats).blacklistedCategories.Contains(Category.typeCrystalMod))
                {
                    ModdingUtils.Extensions.CharacterStatModifiersExtension.GetAdditionalData(player.data.stats).blacklistedCategories.Add(Category.typeCrystalMod);
                }
                if (!ModdingUtils.Extensions.CharacterStatModifiersExtension.GetAdditionalData(player.data.stats).blacklistedCategories.Contains(Category.typeCrystal))
                {
                    ModdingUtils.Extensions.CharacterStatModifiersExtension.GetAdditionalData(player.data.stats).blacklistedCategories.Add(Category.typeCrystal);
                }
            }
            yield break;
        }

        // Assets loader
        public static readonly AssetBundle VFXBundle = Jotunn.Utils.AssetUtils.LoadAssetBundleFromResources("gearup_asset", typeof(GearUpCards).Assembly);
    }
}
