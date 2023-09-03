﻿using System;
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
    class MysticMissileCard : CustomCard
    {
        public static GameObject objectToSpawn = null;

        public override void SetupCard(CardInfo cardInfo, Gun gun, ApplyCardStats cardStats, CharacterStatModifiers statModifiers, Block block)
        {
            cardInfo.categories = new CardCategory[]
            {
                GearCategory.typeSpell
            };
            // gun.attackSpeed = 1.0f / 0.85f;
        }

        // "attackSpeed" is technically a gunfire cooldown between shots >> Less is more rapid firing
        // 'attackSpeedMultiplier' works as intended >> More is more rapid firing
        public override void OnAddCard(Player player, Gun gun, GunAmmo gunAmmo, CharacterData data, HealthHandler health, Gravity gravity, Block block, CharacterStatModifiers characterStats)
        {
            ModdingUtils.Extensions.CharacterStatModifiersExtension.GetAdditionalData(player.data.stats).blacklistedCategories.Remove(GearCategory.tagSpellOnlyAugment);

            // gun.attackSpeed *= 1.0f / 0.85f;
            // gun.damage *= .50f;
            if (gun.bulletDamageMultiplier > 1.0f)
            {
                gun.bulletDamageMultiplier -= 0.50f;
            }
            else
            {
                gun.bulletDamageMultiplier *= 0.50f;
            }

            gunAmmo.reloadTimeMultiplier += .25f;
            gun.projectileColor = new Color(1f, 0.0f, 0.85f, 1f);

            // add ONLY one copy of the object to the projectile
            if (characterStats.GetGearData().mysticMissileStack == 0)
            {
                if (objectToSpawn == null)
                {
                    objectToSpawn = new GameObject("MysticMissileModifier", new Type[]
                    {
                        typeof(MysticMissileModifier)
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

            characterStats.GetGearData().mysticMissileStack += 1;
        }
        public override void OnRemoveCard(Player player, Gun gun, GunAmmo gunAmmo, CharacterData data, HealthHandler health, Gravity gravity, Block block, CharacterStatModifiers characterStats)
        {
            // bullet modifier pool auto-reset on card removal, simply let it do its jobs
            // UnityEngine.Debug.Log($"[{GearUpCards.ModInitials}][Card] {GetTitle()} has been removed to player {player.playerID}.");
        }
        protected override string GetTitle()
        {
            return "Mystic Missile!";
        }
        protected override string GetDescription()
        {
            return "Enchant the bullets with glyphs and make it explode on contact with scalable magic energy!";
        }
        protected override GameObject GetCardArt()
        {
            // return GearUpCards.CardArtBundle.LoadAsset<GameObject>("C_ChompyBullet");
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
                    stat = "DMG",
                    amount = "-50%",
                    simepleAmount = CardInfoStat.SimpleAmount.notAssigned
                },
                new CardInfoStat()
                {
                    positive = false,
                    stat = "Reload Time",
                    amount = "+25%",
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
            this.cardInfo.gameObject.AddComponent<ExtraName>().text = "Passive\nSpell";
        }
    }
}
