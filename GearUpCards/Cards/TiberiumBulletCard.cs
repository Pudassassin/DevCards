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
    class TiberiumBulletCard : CustomCard
    {
        public static GameObject objectToSpawn = null;

        public override void SetupCard(CardInfo cardInfo, Gun gun, ApplyCardStats cardStats, CharacterStatModifiers statModifiers, Block block)
        {
            // gun.attackSpeed = 1.0f / 0.85f;
            cardInfo.categories = new CardCategory[]
            {
                GearCategory.noType
            };
        }

        // "attackSpeed" is technically a gunfire cooldown between shots >> Less is more rapid firing
        // 'attackSpeedMultiplier' works as intended >> More is more rapid firing
        public override void OnAddCard(Player player, Gun gun, GunAmmo gunAmmo, CharacterData data, HealthHandler health, Gravity gravity, Block block, CharacterStatModifiers characterStats)
        {
            gun.projectileColor = new Color(0.0f, 0.85f, 0.0f, 1f);
            gun.bulletDamageMultiplier *= 0.75f;

            data.maxHealth *= 0.75f;

            // add ONLY one stack of bullet mono
            if (characterStats.GetGearData().tiberiumBulletStack == 0)
            {
                if (objectToSpawn == null)
                {
                    objectToSpawn = new GameObject("TiberiumBulletModifier", new Type[]
                    {
                        typeof(TiberiumBulletModifier)
                    });
                    DontDestroyOnLoad(objectToSpawn);
                }

                List<ObjectsToSpawn> list = gun.objectsToSpawn.ToList<ObjectsToSpawn>();
                list.Add(new ObjectsToSpawn
                {
                    AddToProjectile = objectToSpawn
                });

                gun.objectsToSpawn = list.ToArray();
            }

            player.gameObject.GetOrAddComponent<TiberiumToxicEffect>();
            characterStats.GetGearData().tiberiumBulletStack += 1;
        }
        public override void OnRemoveCard(Player player, Gun gun, GunAmmo gunAmmo, CharacterData data, HealthHandler health, Gravity gravity, Block block, CharacterStatModifiers characterStats)
        {
            // bullet modifier pool auto-reset on card removal, simply let it do its jobs
            // UnityEngine.Debug.Log($"[{GearUpCards.ModInitials}][Card] {GetTitle()} has been removed to player {player.playerID}.");
        }
        protected override string GetTitle()
        {
            return "Tiberium Bullet";
        }
        protected override string GetDescription()
        {
            return "Bullets continuously drain life from the victims they hit, with initial burst!";
            // return "Bullet deal +100% DMG as -HP over 4s, then +5% DMG -HP/s until the victim's true death";
            // return "Bullets cause target to lose\nHP over time until death!\n(Stackable Additively)";
        }
        protected override GameObject GetCardArt()
        {
            return GearUpCards.CardArtBundle.LoadAsset<GameObject>("C_TiberiumBullets");
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
                //     stat = "HP Removal",
                //     amount = "+100% dmg, 4s",
                //     simepleAmount = CardInfoStat.SimpleAmount.notAssigned
                // },
                // new CardInfoStat()
                // {
                //     positive = true,
                //     stat = "Chronic HP...",
                //     amount = "+5% dmg +0.35",
                //     simepleAmount = CardInfoStat.SimpleAmount.notAssigned
                // },
                new CardInfoStat()
                {
                    positive = true,
                    stat = "Life Drain",
                    amount = "+15% dmg/s",
                    simepleAmount = CardInfoStat.SimpleAmount.notAssigned
                },
                new CardInfoStat()
                {
                    positive = false,
                    stat = "HP & DMG",
                    amount = "-25%",
                    simepleAmount = CardInfoStat.SimpleAmount.notAssigned
                },
                new CardInfoStat()
                {
                    positive = false,
                    stat = "Self HP",
                    amount = "-2.5/s",
                    simepleAmount = CardInfoStat.SimpleAmount.notAssigned
                }

            };
        }
        protected override CardThemeColor.CardThemeColorType GetTheme()
        {
            return CardThemeColor.CardThemeColorType.PoisonGreen;
        }
        public override string GetModName()
        {
            return GearUpCards.ModInitials;
        }
        public override void Callback()
        {
            this.cardInfo.gameObject.AddComponent<ExtraName>().text = "Tiberium\nProphecy";
        }
    }
}
