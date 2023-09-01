using System;
using System.Collections.Generic;
using System.Linq;

using UnboundLib;
using UnboundLib.Cards;
using CardChoiceSpawnUniqueCardPatch.CustomCategories;

using UnityEngine;

using GearUpCards.MonoBehaviours;
using GearUpCards.Utils;
using GearUpCards.Extensions;
using static GearUpCards.Utils.CardUtils;
using RarityLib.Utils;

namespace GearUpCards.Cards
{
    class SupplyDropCard : CustomCard
    {
        public override void SetupCard(CardInfo cardInfo, Gun gun, ApplyCardStats cardStats, CharacterStatModifiers statModifiers, Block block)
        {
            cardInfo.categories = new CardCategory[]
            {
                GearCategory.typeBoosterPack,
                GearCategory.tagNoGlitch,
                GearCategory.tagNoRemove,
                GearCategory.tagNoTableFlip,
                GearCategory.tagCardManipulation
            };

            // gun.attackSpeed = 1.0f / 1.20f;
        }
        public override void OnAddCard(Player player, Gun gun, GunAmmo gunAmmo, CharacterData data, HealthHandler health, Gravity gravity, Block block, CharacterStatModifiers characterStats)
        {
            // gun.damage *= 1.20f;
            // gun.attackSpeedMultiplier += 0.25f;
            // gunAmmo.maxAmmo += 3;

            CardDrawTracker cardDrawTracker = player.gameObject.GetOrAddComponent<CardDrawTracker>();

            CardDrawTracker.ExtraCardDraw extraCardDraw = new CardDrawTracker.ExtraCardDraw(3, 1);
            Rarity rarity = CardUtils.TryQueryRarity("Uncommon", "Uncommon");
            extraCardDraw.SetWhitelistRarityRange(rarity, includeLower: true);

            cardDrawTracker.extraCardDrawsDelayed.Add(extraCardDraw);
        }
        public override void OnRemoveCard(Player player, Gun gun, GunAmmo gunAmmo, CharacterData data, HealthHandler health, Gravity gravity, Block block, CharacterStatModifiers characterStats)
        {

        }
        protected override string GetTitle()
        {
            return "Supply Drop!";
        }
        protected override string GetDescription()
        {
            return "You get to pick <color=green>THREE</color> more <color=#2CADFFff>Uncommon</color> or lower rarity cards in the next draw phase.";
        }
        protected override GameObject GetCardArt()
        {
            // return GearUpCards.CardArtBundle.LoadAsset<GameObject>("C_GunParts");
            return null;
        }
        protected override CardInfo.Rarity GetRarity()
        {
            return CardInfo.Rarity.Uncommon;
        }
        protected override CardInfoStat[] GetStats()
        {
            return new CardInfoStat[]
            {
                // new CardInfoStat()
                // {
                //     positive = true,
                //     stat = "DMG",
                //     amount = "+20%",
                //     simepleAmount = CardInfoStat.SimpleAmount.notAssigned
                // },
                // new CardInfoStat()
                // {
                //     positive = true,
                //     stat = "ATK SPD",
                //     amount = "+20%",
                //     simepleAmount = CardInfoStat.SimpleAmount.notAssigned
                // },
                // new CardInfoStat()
                // {
                //     positive = true,
                //     stat = "Max Ammo",
                //     amount = "+3",
                //     simepleAmount = CardInfoStat.SimpleAmount.notAssigned
                // }
            };
        }
        protected override CardThemeColor.CardThemeColorType GetTheme()
        {
            return CardThemeColor.CardThemeColorType.TechWhite;
        }
        public override string GetModName()
        {
            return GearUpCards.ModInitials;
        }
        public override void Callback()
        {
            this.cardInfo.gameObject.AddComponent<ExtraName>().text = "Booster\nPack";
        }
    }
}
