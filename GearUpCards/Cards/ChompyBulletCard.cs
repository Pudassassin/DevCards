using System;
using System.Collections.Generic;
using System.Linq;

using GearUpCards.MonoBehaviours;
using UnboundLib;
using UnboundLib.Cards;
using UnityEngine;

namespace GearUpCards.Cards
{
    class ChompyBulletCard : CustomCard
    {
        private GameObject chompyBulletModifier = null;

        public override void SetupCard(CardInfo cardInfo, Gun gun, ApplyCardStats cardStats, CharacterStatModifiers statModifiers, Block block)
        {
            
        }
        public override void OnAddCard(Player player, Gun gun, GunAmmo gunAmmo, CharacterData data, HealthHandler health, Gravity gravity, Block block, CharacterStatModifiers characterStats)
        {
            gun.damage *= .75f;
            gun.attackSpeedMultiplier *= .75f;
            gunAmmo.reloadTimeMultiplier += .25f;

            // add ChompyBulletEffect and/or +1 stack
            if (chompyBulletModifier == null)
            {
                List<ObjectsToSpawn> list = gun.objectsToSpawn.ToList<ObjectsToSpawn>();

                GameObject chompyBulletModifier = new GameObject("A_Sugar", new Type[]
                    {
                        typeof(ChompyBulletEffect)
                    });
                list.Add(new ObjectsToSpawn
                    {
                        AddToProjectile = chompyBulletModifier
                    });

                gun.objectsToSpawn = list.ToArray();

                chompyBulletModifier.GetComponent<ChompyBulletEffect>().Setup(player, gun);
            }

            chompyBulletModifier.GetComponent<ChompyBulletEffect>().AddStack();
        }
        public override void OnRemoveCard(Player player, Gun gun, GunAmmo gunAmmo, CharacterData data, HealthHandler health, Gravity gravity, Block block, CharacterStatModifiers characterStats)
        {
            // -1 stack and potentially remove the effect
            chompyBulletModifier.GetComponent<ChompyBulletEffect>().RemoveStack();

            UnityEngine.Debug.Log($"[{GearUpCards.ModInitials}][Card] {GetTitle()} has been removed to player {player.playerID}.");
        }
        protected override string GetTitle()
        {
            return "Chompy Bullet";
        }
        protected override string GetDescription()
        {
            return "Bullets deal bonus damage based on victim's current health. Reduced effect the more bullets you shoot at once.";
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
                    stat = "HP Culling",
                    amount = "~ +%",
                    simepleAmount = CardInfoStat.SimpleAmount.notAssigned
                },
                new CardInfoStat()
                {
                    positive = false,
                    stat = "Damage",
                    amount = "-xx%",
                    simepleAmount = CardInfoStat.SimpleAmount.notAssigned
                },
                new CardInfoStat()
                {
                    positive = false,
                    stat = "Fire Rate",
                    amount = "-xx%",
                    simepleAmount = CardInfoStat.SimpleAmount.notAssigned
                },
                new CardInfoStat()
                {
                    positive = false,
                    stat = "Reload Speed",
                    amount = "-xx%",
                    simepleAmount = CardInfoStat.SimpleAmount.notAssigned
                }

            };
        }
        protected override CardThemeColor.CardThemeColorType GetTheme()
        {
            return CardThemeColor.CardThemeColorType.ColdBlue;
        }
        public override string GetModName()
        {
            return GearUpCards.ModInitials;
        }
    }
}
