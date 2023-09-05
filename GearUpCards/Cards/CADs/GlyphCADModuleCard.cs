﻿using System;
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
    class GlyphCADModuleCard : CustomCard
    {
        public override void SetupCard(CardInfo cardInfo, Gun gun, ApplyCardStats cardStats, CharacterStatModifiers statModifiers, Block block)
        {
            ModdingUtils.Extensions.CardInfoExtension.GetAdditionalData(cardInfo).canBeReassigned = false;
            cardInfo.allowMultiple = false;
            cardInfo.categories = new CardCategory[]
            {
                GearCategory.typeCadModule,
                GearCategory.tagNoGlitch
            };
        }
        public override void OnAddCard(Player player, Gun gun, GunAmmo gunAmmo, CharacterData data, HealthHandler health, Gravity gravity, Block block, CharacterStatModifiers characterStats)
        {
            // black/whitelisting
            // ModdingUtils.Extensions.CharacterStatModifiersExtension.GetAdditionalData(player.data.stats).blacklistedCategories.Add(GearCategory.typeUniqueGunSpread);

            player.gameObject.GetOrAddComponent<GearUpPreRoundEffects>();
            characterStats.GetGearData().addOnList.Add(GearUpConstants.AddOnType.cadModuleGlyph);

            // give 1 bonus Glyph card
            CardDrawTracker cardDrawTracker = player.gameObject.GetOrAddComponent<CardDrawTracker>();

            CardDrawTracker.ExtraCardDraw extraCardDraw = new CardDrawTracker.ExtraCardDraw(1);
            extraCardDraw.SetWhitelistGearUpCard(new List<CardCategory> { GearCategory.typeGlyph });

            cardDrawTracker.QueueExtraDraw(extraCardDraw);

        }
        public override void OnRemoveCard(Player player, Gun gun, GunAmmo gunAmmo, CharacterData data, HealthHandler health, Gravity gravity, Block block, CharacterStatModifiers characterStats)
        {

        }
        protected override string GetTitle()
        {
            return "Glyph CAD Module";
        }
        protected override string GetDescription()
        {
            return "Glyphs give <color=green>TWICE</color> the effect to your gun and block.\nBoost the chance of finding Spell and even more Glyphs.";
        }
        protected override GameObject GetCardArt()
        {
            return GearUpCards.CardArtBundle.LoadAsset<GameObject>("C_GlyphCADModule");
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
                //     stat = "Projectiles",
                //     amount = "+4",
                //     simepleAmount = CardInfoStat.SimpleAmount.notAssigned
                // },
                // new CardInfoStat()
                // {
                //     positive = false,
                //     stat = "Spread",
                //     amount = "+60 deg",
                //     simepleAmount = CardInfoStat.SimpleAmount.notAssigned
                // },
                new CardInfoStat()
                {
                    positive = true,
                    stat = "Glyph",
                    amount = "+1",
                    simepleAmount = CardInfoStat.SimpleAmount.notAssigned
                }
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
            this.cardInfo.gameObject.AddComponent<ExtraName>().text = "CAD\nModule";
        }
    }
}
