using System;
using System.Collections.Generic;
using System.Linq;

using GearUpCards.MonoBehaviours;
using UnboundLib;
using UnboundLib.Cards;
using UnityEngine;

using GearUpCards.Extensions;
using static GearUpCards.Utils.CardUtils;
using UnboundLib.Utils;

namespace GearUpCards.Cards
{
    class LifeforceDualityOrbCard : CustomCard
    {
        // private static string targetBounceCardName = "Target BOUNCE";
        // private static GameObject screenEdgePrefab = null;

        public override void SetupCard(CardInfo cardInfo, Gun gun, ApplyCardStats cardStats, CharacterStatModifiers statModifiers, Block block)
        {
            cardInfo.categories = new CardCategory[]
            {
                GearCategory.typeSpell
            };
        }

        // "attackSpeed" is technically a gunfire cooldown between shots >> Less is more rapid firing
        // 'attackSpeedMultiplier' works as intended >> More is more rapid firing
        public override void OnAddCard(Player player, Gun gun, GunAmmo gunAmmo, CharacterData data, HealthHandler health, Gravity gravity, Block block, CharacterStatModifiers characterStats)
        {
            ModdingUtils.Extensions.CharacterStatModifiersExtension.GetAdditionalData(player.data.stats).blacklistedCategories.Remove(GearCategory.tagSpellOnlyAugment);

            block.cdAdd += 1.0f;

            characterStats.GetGearData().orbLifeforceDualityStack += 1;
            player.gameObject.GetOrAddComponent<OrbSpellsMono>();

            CooldownUIMono cooldownUI = player.gameObject.GetOrAddComponent<CooldownUIMono>();

            // if (screenEdgePrefab == null)
            // {
            //     CardInfo cardInfo = CardManager.cards.Values.First(card => card.cardInfo.cardName == targetBounceCardName).cardInfo;
            //     Gun cardGun = cardInfo.gameObject.GetComponent<Gun>();
            //     screenEdgePrefab = cardGun.objectsToSpawn[1].AddToProjectile;
            // }
            // 
            // // temp for testing
            // List<ObjectsToSpawn> list = gun.objectsToSpawn.ToList<ObjectsToSpawn>();
            // 
            // GameObject gameObject = new GameObject("TestModifier", new Type[]
            // {
            //     typeof(OrbLifeforceDualityModifier)
            // });
            // list.Add(new ObjectsToSpawn
            // {
            //     AddToProjectile = gameObject
            // });
            // list.Add(new ObjectsToSpawn
            // {
            //     AddToProjectile = screenEdgePrefab
            // });
            // 
            // gun.objectsToSpawn = list.ToArray();

            // gun.projectileSpeed = 0.05f;
            // gun.gravity = 0.0f;
            // gun.reflects = 30;
        }
        public override void OnRemoveCard(Player player, Gun gun, GunAmmo gunAmmo, CharacterData data, HealthHandler health, Gravity gravity, Block block, CharacterStatModifiers characterStats)
        {
            // bullet modifier pool auto-reset on card removal, simply let it do its jobs
            // UnityEngine.Debug.Log($"[{GearUpCards.ModInitials}][Card] {GetTitle()} has been removed to player {player.playerID}.");
        }
        protected override string GetTitle()
        {
            return "Lifeforce Duorbity";
        }
        protected override string GetDescription()
        {
            return "Blocking cast the bouncing orb that heals friends and drains foes' lives close to it.";
        }
        protected override GameObject GetCardArt()
        {
            return GearUpCards.CardArtBundle.LoadAsset<GameObject>("C_LifeforceDuorbity");
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
                    amount = "8s",
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
