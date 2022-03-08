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
    class ShardSlinger : CustomCard
    {
        public override void SetupCard(CardInfo cardInfo, Gun gun, ApplyCardStats cardStats, CharacterStatModifiers statModifiers, Block block)
        {
            cardInfo.allowMultiple = false;
            cardInfo.categories = new CardCategory[]
            {
                GearCategory.typeGunMod,
                GearCategory.typeCrystalGear,
                GearCategory.tagNoRemove
            };
        }
        public override void OnAddCard(Player player, Gun gun, GunAmmo gunAmmo, CharacterData data, HealthHandler health, Gravity gravity, Block block, CharacterStatModifiers characterStats)
        {
            // Resolve card conflict on edge cases
            List<HandCardData> cardToCheck = GetPlayerCardsWithCategory(player, GearCategory.typeGunMod);

            if (cardToCheck.Count > 1) 
            {
                var replacementCard = ModdingUtils.Utils.Cards.all.Where(card => card.name == "Gun Parts").ToArray()[0];

                Unbound.Instance.StartCoroutine(ModdingUtils.Utils.Cards.instance.ReplaceCard
                (
                    player, cardToCheck[cardToCheck.Count - 1].index, replacementCard, "GP", 2.0f, 2.0f, true
                ));
                return;
            }

            // black/whitelisting
            ModdingUtils.Extensions.CharacterStatModifiersExtension.GetAdditionalData(player.data.stats).blacklistedCategories.Add(GearCategory.typeGunMod);
            ModdingUtils.Extensions.CharacterStatModifiersExtension.GetAdditionalData(player.data.stats).blacklistedCategories.RemoveAll((category) => category == GearCategory.typeCrystal);

            cardToCheck = GetPlayerCardsWithCategory(player, GearCategory.typeCrystalMod);
            if (cardToCheck.Count == 0)
            {
                ModdingUtils.Extensions.CharacterStatModifiersExtension.GetAdditionalData(player.data.stats).blacklistedCategories.RemoveAll((category) => category == GearCategory.typeCrystalMod);
            }

            // stats upfront
            gun.damage *= 3.0f;
            gun.attackSpeedMultiplier *= .50f;
            gunAmmo.reloadTimeMultiplier += .50f;

            // those stats will be modified ONCE by card effect mono at round start:
            // 1/5 projectile, 1/3 burst, 1/2 ammo, 1/5 bounce, rounding down

            // add ShardSlingerEffect to player

            // add one stack of bullet modifier to the pool
            List<ObjectsToSpawn> list = gun.objectsToSpawn.ToList<ObjectsToSpawn>();

            // GameObject gameObject = new GameObject("ChompyBulletModifier", new Type[]
            // {
            //     typeof(ChompyBulletModifier)
            // });
            // list.Add(new ObjectsToSpawn
            // {
            //     AddToProjectile = gameObject
            // });

            gun.objectsToSpawn = list.ToArray();
        }
        public override void OnRemoveCard(Player player, Gun gun, GunAmmo gunAmmo, CharacterData data, HealthHandler health, Gravity gravity, Block block, CharacterStatModifiers characterStats)
        {
            // black/whitelisting
            List<HandCardData> cardToCheck = GetPlayerCardsWithCategory(player, GearCategory.typeGunMod);
            if (cardToCheck.Count == 0)
            {
                ModdingUtils.Extensions.CharacterStatModifiersExtension.GetAdditionalData(player.data.stats).blacklistedCategories.RemoveAll((category) => category == GearCategory.typeGunMod);
            }

            cardToCheck = GetPlayerCardsWithCategory(player, GearCategory.typeCrystalGear);
            if (cardToCheck.Count == 0)
            {
                ModdingUtils.Extensions.CharacterStatModifiersExtension.GetAdditionalData(player.data.stats).blacklistedCategories.Add(GearCategory.typeCrystal);
            }


        }
        protected override string GetTitle()
        {
            return "Shard Slinger";
        }
        protected override string GetDescription()
        {
            return "Modify your gun to shoot condensed shards of crystal. " +
                "Shard leaves behind part of itself on surface(s) it hit, " +
                "damaging anyone that come into contact for 50% bullet damage\n\n" +
                "+200% DMG, -50% ATKSPD, +0.5 Reload Time\n" +
                "1/5 bullets, 1/3 bursts, 1/2 ammo, 1/5 bounce";
        }
        protected override GameObject GetCardArt()
        {
            return null;
        }
        protected override CardInfo.Rarity GetRarity()
        {
            return CardInfo.Rarity.Rare;
        }
        protected override CardInfoStat[] GetStats()
        {
            return new CardInfoStat[]
            {

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
            card.gameObject.AddComponent<ExtraName>().text = "GunMod Crystal";
        }
    }
}
