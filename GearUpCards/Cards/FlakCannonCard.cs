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
    class FlakCannonCard : CustomCard
    {
        internal static GameObject cardArt = null; // GearUpCards.CardArtBundle.LoadAsset<GameObject>("C_ArcOfBullets");

        public override void SetupCard(CardInfo cardInfo, Gun gun, ApplyCardStats cardStats, CharacterStatModifiers statModifiers, Block block)
        {
            cardInfo.allowMultiple = false;
            cardInfo.categories = new CardCategory[]
            {
                GearCategory.typeUniqueGunSpread
            };
        }
        public override void OnAddCard(Player player, Gun gun, GunAmmo gunAmmo, CharacterData data, HealthHandler health, Gravity gravity, Block block, CharacterStatModifiers characterStats)
        {
            // black/whitelisting
            ModdingUtils.Extensions.CharacterStatModifiersExtension.GetAdditionalData(player.data.stats).blacklistedCategories.Add(GearCategory.typeUniqueGunSpread);

            // gun.damage *= 0.65f;
            // 
            // // about half of [Buckshot]'s spread but this is for mono's calculation
            // gun.spread += 60.0f / 360.0f;
            // gun.evenSpread += 1.0f;
            // gun.numberOfProjectiles += 4;

            UniqueGunSpreadMono mono = player.gameObject.GetOrAddComponent<UniqueGunSpreadMono>();
            // mono.enabled = true;
            characterStats.GetGearData().gunSpreadMod = GearUpConstants.ModType.gunSpreadFlak;

            // add modifier to bullet
            List<ObjectsToSpawn> list = gun.objectsToSpawn.ToList<ObjectsToSpawn>();

            GameObject gameObject = new GameObject("FlakCannonModifier", new Type[]
            {
                // typeof(FlakShellModifier),
                typeof(BulletNoClipModifier)
            });
            list.Add(new ObjectsToSpawn
            {
                AddToProjectile = gameObject
            });

            gun.objectsToSpawn = list.ToArray();
        }
        public override void OnRemoveCard(Player player, Gun gun, GunAmmo gunAmmo, CharacterData data, HealthHandler health, Gravity gravity, Block block, CharacterStatModifiers characterStats)
        {

        }
        protected override string GetTitle()
        {
            return "Flak Cannon";
        }
        protected override string GetDescription()
        {
            return "Your gun now fire bundled shell that detonate into shrapnels after 1.5s, or immediately after <i>directly hitting</i> someone.";
        }
        protected override GameObject GetCardArt()
        {
            return cardArt;
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
                // new CardInfoStat()
                // {
                //     positive = false,
                //     stat = "DMG",
                //     amount = "-35%",
                //     simepleAmount = CardInfoStat.SimpleAmount.notAssigned
                // }
            };
        }
        protected override CardThemeColor.CardThemeColorType GetTheme()
        {
            return CardThemeColor.CardThemeColorType.FirepowerYellow;
        }
        public override string GetModName()
        {
            return GearUpCards.ModInitials;
        }
        public override void Callback()
        {
            this.cardInfo.gameObject.AddComponent<ExtraName>().text = "Unique\nSpread";
        }
    }
}
