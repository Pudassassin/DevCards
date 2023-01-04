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
    class RollingBulwarkOrbCard : CustomCard
    {
        internal static GameObject cardArt = null; // GearUpCards.CardArtBundle.LoadAsset<GameObject>("C_ArcOfBullets");

        public override void SetupCard(CardInfo cardInfo, Gun gun, ApplyCardStats cardStats, CharacterStatModifiers statModifiers, Block block)
        {
            cardInfo.categories = new CardCategory[]
            {
                GearCategory.tagSpell
            };
        }
        public override void OnAddCard(Player player, Gun gun, GunAmmo gunAmmo, CharacterData data, HealthHandler health, Gravity gravity, Block block, CharacterStatModifiers characterStats)
        {
            block.cdAdd += 1.0f;

            ModdingUtils.Extensions.CharacterStatModifiersExtension.GetAdditionalData(player.data.stats).blacklistedCategories.Remove(GearCategory.tagSpellOnlyAugment);

            characterStats.GetGearData().orbRollingBulwarkStack += 1;
            player.gameObject.GetOrAddComponent<OrbSpellsMono>();

            CooldownUIMono cooldownUI = player.gameObject.GetOrAddComponent<CooldownUIMono>();

            // temp prototype
            // List<ObjectsToSpawn> list = gun.objectsToSpawn.ToList<ObjectsToSpawn>();
            // 
            // GameObject gameObject = new GameObject("RollingBorbwarkModifier", new Type[]
            // {
            //     typeof(BulletNoClipModifier),
            //     typeof(CustomEmpowerShotModifier)
            // });
            // 
            // list.Add(new ObjectsToSpawn
            // {
            //     AddToProjectile = gameObject
            // });
            // 
            // gun.objectsToSpawn = list.ToArray();
        }
        public override void OnRemoveCard(Player player, Gun gun, GunAmmo gunAmmo, CharacterData data, HealthHandler health, Gravity gravity, Block block, CharacterStatModifiers characterStats)
        {

        }
        protected override string GetTitle()
        {
            return "Rolling Borbwark";
        }
        protected override string GetDescription()
        {
            return "Blocking cast the ricocheting orb that trigger block abilities every bounce!";
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
                //     stat = "Max HP Culling",
                //     amount = "10~15%",
                //     simepleAmount = CardInfoStat.SimpleAmount.notAssigned
                // },
                new CardInfoStat()
                {
                    positive = false,
                    stat = "Block CD",
                    amount = "+1s",
                    simepleAmount = CardInfoStat.SimpleAmount.notAssigned
                },
                new CardInfoStat()
                {
                    positive = false,
                    stat = "Spell CD",
                    amount = "6s",
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
            this.cardInfo.gameObject.AddComponent<ExtraName>().text = "Orb\nSpell";
        }
    }
}
