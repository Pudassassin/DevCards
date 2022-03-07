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
    class SizeNormalizerCard : CustomCard
    {
        public override void SetupCard(CardInfo cardInfo, Gun gun, ApplyCardStats cardStats, CharacterStatModifiers statModifiers, Block block)
        {
            cardInfo.allowMultiple = false;
            cardInfo.categories = new CardCategory[]
            {
                Category.typeSizeMod,
                Category.tagNoRemove
            };
        }
        public override void OnAddCard(Player player, Gun gun, GunAmmo gunAmmo, CharacterData data, HealthHandler health, Gravity gravity, Block block, CharacterStatModifiers characterStats)
        {
            // Resolve card conflicts
            CardHandResolveMono resolver = player.gameObject.GetOrAddComponent<CardHandResolveMono>();
            List<HandCardData> conflictedCards = GetPlayerCardsWithCategory(player, Category.typeSizeMod);

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
            ModdingUtils.Extensions.CharacterStatModifiersExtension.GetAdditionalData(player.data.stats).blacklistedCategories.Add(Category.typeSizeMod);

            // stats
            player.data.maxHealth *= 1.5f;
            characterStats.movementSpeed *= 1.25f;
            characterStats.GetGearData().sizeMod = GearUpConstants.ModType.sizeNormalize;

            // Add Size Normalizer mono
            SizeNormalizerEffect effect = player.gameObject.GetOrAddComponent<SizeNormalizerEffect>();
            effect.Refresh();
        }
        public override void OnRemoveCard(Player player, Gun gun, GunAmmo gunAmmo, CharacterData data, HealthHandler health, Gravity gravity, Block block, CharacterStatModifiers characterStats)
        {
            // black/whitelisting
            List<HandCardData> cardToCheck = GetPlayerCardsWithCategory(player, Category.typeSizeMod);

            if (cardToCheck.Count <= 0)
            {
                ModdingUtils.Extensions.CharacterStatModifiersExtension.GetAdditionalData(player.data.stats).blacklistedCategories.RemoveAll((category) => category == Category.typeSizeMod);
            }

            // temporary
            // player.data.movement.force /= 1.25f;

        }
        protected override string GetTitle()
        {
            return "Size Normalizer";
        }
        protected override string GetDescription()
        {
            return "Set your final player size much closer to default where it started.";
        }
        protected override GameObject GetCardArt()
        {
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
                    stat = "HP",
                    amount = "+50%",
                    simepleAmount = CardInfoStat.SimpleAmount.notAssigned
                },
                new CardInfoStat()
                {
                    positive = true,
                    stat = "Movement Speed",
                    amount = "+25%",
                    simepleAmount = CardInfoStat.SimpleAmount.notAssigned
                },
                new CardInfoStat()
                {
                    positive = true,
                    stat = "Size",
                    amount = "Near normal",
                    simepleAmount = CardInfoStat.SimpleAmount.notAssigned
                },
                new CardInfoStat()
                {
                    positive = false,
                    stat = "GO BEEG",
                    amount = "Cannot",
                    simepleAmount = CardInfoStat.SimpleAmount.notAssigned
                },
                new CardInfoStat()
                {
                    positive = false,
                    stat = "go smol",
                    amount = "Cannot",
                    simepleAmount = CardInfoStat.SimpleAmount.notAssigned
                }
            };
        }
        protected override CardThemeColor.CardThemeColorType GetTheme()
        {
            return CardThemeColor.CardThemeColorType.DestructiveRed;
        }
        public override string GetModName()
        {
            return GearUpCards.ModInitials;
        }
        internal static void callback(CardInfo card)
        {
            card.gameObject.AddComponent<ExtraName>().text = "Size Mod";
        }
    }
}
