using System;
using System.Collections.Generic;
using System.Linq;

using UnboundLib;
using UnboundLib.Cards;
using UnityEngine;

using GearUpCards.MonoBehaviours;
using GearUpCards.Extensions;
using static GearUpCards.Utils.CardUtils;

namespace GearUpCards.Cards
{
    class ChompyBulletCard : CustomCard
    {
        internal static GameObject cardArt = GearUpCards.CardArtBundle.LoadAsset<GameObject>("C_ChompyBullet");

        public override void SetupCard(CardInfo cardInfo, Gun gun, ApplyCardStats cardStats, CharacterStatModifiers statModifiers, Block block)
        {
            gun.attackSpeed = 1.0f / 0.85f;
        }

        // "attackSpeed" is technically a gunfire cooldown between shots >> Less is more rapid firing
        // 'attackSpeedMultiplier' works as intended >> More is more rapid firing
        public override void OnAddCard(Player player, Gun gun, GunAmmo gunAmmo, CharacterData data, HealthHandler health, Gravity gravity, Block block, CharacterStatModifiers characterStats)
        {
            gun.damage *= .85f;
            gunAmmo.reloadTimeMultiplier += .15f;
            gun.projectileColor = new Color(1f, 0.0f, 0.0f, 1f);

            // add ONLY one stack of ChompyBulletEffect to the bullet modifier pool
            if (characterStats.GetGearData().chompyBulletStack == 0)
            {
                List<ObjectsToSpawn> list = gun.objectsToSpawn.ToList<ObjectsToSpawn>();

                GameObject gameObject = new GameObject("ChompyBulletModifier", new Type[]
                {
                    typeof(ChompyBulletModifier)
                });
                list.Add(new ObjectsToSpawn
                {
                    AddToProjectile = gameObject
                });

                gun.objectsToSpawn = list.ToArray();
            }

            characterStats.GetGearData().chompyBulletStack += 1;
        }
        public override void OnRemoveCard(Player player, Gun gun, GunAmmo gunAmmo, CharacterData data, HealthHandler health, Gravity gravity, Block block, CharacterStatModifiers characterStats)
        {
            // bullet modifier pool auto-reset on card removal, simply let it do its jobs
            // UnityEngine.Debug.Log($"[{GearUpCards.ModInitials}][Card] {GetTitle()} has been removed to player {player.playerID}.");
        }
        protected override string GetTitle()
        {
            return "Chompy Bullet";
        }
        protected override string GetDescription()
        {
            return "Bullets deal more damage based on target's current HP. Scales inversely with firerate.";
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
                //     stat = "HP Culling",
                //     amount = "+20%",
                //     simepleAmount = CardInfoStat.SimpleAmount.notAssigned
                // },
                // new CardInfoStat()
                // {
                //     positive = false,
                //     stat = "DMG",
                //     amount = "-15%",
                //     simepleAmount = CardInfoStat.SimpleAmount.notAssigned
                // },
                new CardInfoStat()
                {
                    positive = false,
                    stat = "DMG, AtkSPD",
                    amount = "-15%",
                    simepleAmount = CardInfoStat.SimpleAmount.notAssigned
                },
                new CardInfoStat()
                {
                    positive = false,
                    stat = "Reload SPD",
                    amount = "-15%",
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
        public override void Callback()
        {
            this.cardInfo.gameObject.AddComponent<ExtraName>().text = "Passive\nBullet";
        }
    }
}
