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

namespace GearUpCards.Cards
{
    class AntiBulletMagickCard : CustomCard
    {
        internal static GameObject cardArt = GearUpCards.CardArtBundle.LoadAsset<GameObject>("C_AntiBulletMagick");

        public override void SetupCard(CardInfo cardInfo, Gun gun, ApplyCardStats cardStats, CharacterStatModifiers statModifiers, Block block)
        {
            cardInfo.allowMultiple = false;
            cardInfo.categories = new CardCategory[]
            {
                GearCategory.typeUniqueMagick,
                GearCategory.tagNoRemove
            };
        }
        public override void OnAddCard(Player player, Gun gun, GunAmmo gunAmmo, CharacterData data, HealthHandler health, Gravity gravity, Block block, CharacterStatModifiers characterStats)
        {
            // Resolve card conflicts
            CardHandResolveMono resolver = player.gameObject.GetOrAddComponent<CardHandResolveMono>();
            List<HandCardData> conflictedCards = GetPlayerCardsWithCategory(player, GearCategory.typeUniqueMagick);

            // foreach (var item in conflictedCards)
            // {
            //     UnityEngine.Debug.Log($"[{item.cardInfo.cardName}] - [{item.index}] - [{item.owner.playerID}]");
            // }

            if (conflictedCards.Count >= 1)
            {
                resolver.TriggerResolve();
                return;
            }

            // black/whitelisting
            ModdingUtils.Extensions.CharacterStatModifiersExtension.GetAdditionalData(player.data.stats).blacklistedCategories.Add(GearCategory.typeUniqueMagick);

            // stats
            block.cdAdd += 1.5f;
            characterStats.GetGearData().uniqueMagick = GearUpConstants.ModType.magickAntiBullet;

            // Add effect mono
            player.gameObject.GetOrAddComponent<AntiBulletMagickEffect>();

            CooldownUIMono cooldownUI = player.gameObject.GetOrAddComponent<CooldownUIMono>();
            // cooldownUI.FetchAbilities();
        }

        public override void OnRemoveCard(Player player, Gun gun, GunAmmo gunAmmo, CharacterData data, HealthHandler health, Gravity gravity, Block block, CharacterStatModifiers characterStats)
        {
            // black/whitelisting here are too finicky

        }
        protected override string GetTitle()
        {
            return "Anti-Bullet Magick";
        }
        protected override string GetDescription()
        {
            return "Blocking casts the spell that deletes all nearby bullets in flight and also in guns' magazines, including yours!";
        }
        protected override GameObject GetCardArt()
        {
            return cardArt;
        }
        protected override CardInfo.Rarity GetRarity()
        {
            return CardInfo.Rarity.Rare;
        }
        protected override CardInfoStat[] GetStats()
        {
            return new CardInfoStat[]
            {
                new CardInfoStat()
                {
                    positive = true,
                    stat = "Bullets",
                    amount = "Deletes",
                    simepleAmount = CardInfoStat.SimpleAmount.notAssigned
                },
                new CardInfoStat()
                {
                    positive = true,
                    stat = "Forced Reload",
                    amount = "+3s",
                    simepleAmount = CardInfoStat.SimpleAmount.notAssigned
                },
                new CardInfoStat()
                {
                    positive = false,
                    stat = "Block CD",
                    amount = "+1.5s",
                    simepleAmount = CardInfoStat.SimpleAmount.notAssigned
                },
                new CardInfoStat()
                {
                    positive = false,
                    stat = "Spell CD",
                    amount = "15s",
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
        internal static void callback(CardInfo card)
        {
            card.gameObject.AddComponent<ExtraName>().text = "Unique\nMagick";
        }
    }
}
