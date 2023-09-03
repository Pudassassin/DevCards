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

// using RarityLib.Utils;

namespace GearUpCards.Cards
{
    class ReplicationGlyph : CustomCard
    {
        public override void SetupCard(CardInfo cardInfo, Gun gun, ApplyCardStats cardStats, CharacterStatModifiers statModifiers, Block block)
        {
            cardInfo.categories = new CardCategory[]
            {
                GearCategory.typeGlyph
            };
        }
        public override void OnAddCard(Player player, Gun gun, GunAmmo gunAmmo, CharacterData data, HealthHandler health, Gravity gravity, Block block, CharacterStatModifiers characterStats)
        {
            gun.projectielSimulatonSpeed *= 1.15f;
            gun.projectileSpeed *= 1.25f;

            gun.attackSpeed *= 1.0f / 0.80f;

            // unused stats
            // gun.attackSpeedMultiplier *= 0.85f;

            gun.spread += 30.0f / 360.0f;

            gun.numberOfProjectiles += 3;

            characterStats.GetGearData().glyphReplication += 1;

            // divination is special case to help finding spells MUCH easier
            // RarityUtils.AjustCardRarityModifier
            // (
            //     GetCardInfo(GearUpCards.ModInitials, "Anti-Bullet Magick"),
            //     mul: 2.50f
            // );
            // RarityUtils.AjustCardRarityModifier
            // (
            //     GetCardInfo(GearUpCards.ModInitials, "Orb-literation!"),
            //     mul: 1.50f
            // );
        }
        public override void OnRemoveCard(Player player, Gun gun, GunAmmo gunAmmo, CharacterData data, HealthHandler health, Gravity gravity, Block block, CharacterStatModifiers characterStats)
        {

        }
        protected override string GetTitle()
        {
            return "Replication Glyph";
        }
        protected override string GetDescription()
        {
            return "You fire more bullets and conjure more spell projectiles at once, and these extras are exact copies!";
        }
        protected override GameObject GetCardArt()
        {
            // return GearUpCards.CardArtBundle.LoadAsset<GameObject>("C_GlyphDivination");
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
                new CardInfoStat()
                {
                    positive = true,
                    stat = "Gun Projectiles",
                    amount = "+3",
                    simepleAmount = CardInfoStat.SimpleAmount.notAssigned
                },
                // new CardInfoStat()
                // {
                //     positive = true,
                //     stat = "Spell Proj.",
                //     amount = "+1",
                //     simepleAmount = CardInfoStat.SimpleAmount.notAssigned
                // },
                new CardInfoStat()
                {
                    positive = false,
                    stat = "ATK SPD",
                    amount = "-20%",
                    simepleAmount = CardInfoStat.SimpleAmount.notAssigned
                },
                new CardInfoStat()
                {
                    positive = false,
                    stat = "Spread",
                    amount = "+30 deg",
                    simepleAmount = CardInfoStat.SimpleAmount.notAssigned
                }
            };
        }
        protected override CardThemeColor.CardThemeColorType GetTheme()
        {
            return CardThemeColor.CardThemeColorType.MagicPink;
        }
        public override string GetModName()
        {
            return GearUpCards.ModInitials;
        }
        public override void Callback()
        {
            this.cardInfo.gameObject.AddComponent<ExtraName>().text = "Spell\nGlyph";
        }
    }
}
