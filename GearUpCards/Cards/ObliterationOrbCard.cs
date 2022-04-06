using System;
using System.Collections.Generic;
using System.Linq;

using GearUpCards.MonoBehaviours;
using UnboundLib;
using UnboundLib.Cards;
using UnityEngine;
using static GearUpCards.Utils.CardUtils;

namespace GearUpCards.Cards
{
    class ObliterationOrbCard : CustomCard
    {
        // internal static GameObject cardArt = GearUpCards.CardArtBundle.LoadAsset<GameObject>("C_ChompyBullet");

        public override void SetupCard(CardInfo cardInfo, Gun gun, ApplyCardStats cardStats, CharacterStatModifiers statModifiers, Block block)
        {
            
        }

        // "attackSpeed" is technically a gunfire cooldown between shots >> Less is more rapid firing
        // 'attackSpeedMultiplier' works as intended >> More is more rapid firing
        public override void OnAddCard(Player player, Gun gun, GunAmmo gunAmmo, CharacterData data, HealthHandler health, Gravity gravity, Block block, CharacterStatModifiers characterStats)
        {
            // gun.projectileColor = new Color(1f, 0.0f, 0.0f, 1f);
            // add one stack of ChompyBulletEffect to the bullet modifier pool

            List<ObjectsToSpawn> list = gun.objectsToSpawn.ToList();

            GameObject gameObject = new GameObject("ObliterationModifier", new Type[]
            {
                typeof(ObliterationModifier)
            });
            list.Add(new ObjectsToSpawn
            {
                AddToProjectile = gameObject
            });

            gun.objectsToSpawn = list.ToArray();
        }
        public override void OnRemoveCard(Player player, Gun gun, GunAmmo gunAmmo, CharacterData data, HealthHandler health, Gravity gravity, Block block, CharacterStatModifiers characterStats)
        {
            // bullet modifier pool auto-reset on card removal, simply let it do its jobs
            // UnityEngine.Debug.Log($"[{GearUpCards.ModInitials}][Card] {GetTitle()} has been removed to player {player.playerID}.");
        }
        protected override string GetTitle()
        {
            return "Orb-literation!";
        }
        protected override string GetDescription()
        {
            return "Blocking casts a spell orb that remove a portion of the map at impact point. Affected player loses a chunk of their max HP until round ends.\n!WIP WIP WIP!";
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
                // new CardInfoStat()
                // {
                //     positive = true,
                //     stat = "HP Culling",
                //     amount = "+15%",
                //     simepleAmount = CardInfoStat.SimpleAmount.notAssigned
                // },
                // new CardInfoStat()
                // {
                //     positive = false,
                //     stat = "DMG",
                //     amount = "-25%",
                //     simepleAmount = CardInfoStat.SimpleAmount.notAssigned
                // },
                // new CardInfoStat()
                // {
                //     positive = false,
                //     stat = "ATK SPD",
                //     amount = "-25%",
                //     simepleAmount = CardInfoStat.SimpleAmount.notAssigned
                // },
                // new CardInfoStat()
                // {
                //     positive = false,
                //     stat = "Reload SPD",
                //     amount = "-25%",
                //     simepleAmount = CardInfoStat.SimpleAmount.notAssigned
                // }
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
            card.gameObject.AddComponent<ExtraName>().text = "Orb\nSpell";
        }
    }
}
