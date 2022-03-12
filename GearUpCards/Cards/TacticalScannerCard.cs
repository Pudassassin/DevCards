using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

using UnboundLib;
using UnboundLib.Utils;
using UnboundLib.Cards;
using UnboundLib.Networking;

using GearUpCards.MonoBehaviours;
using GearUpCards.Extensions;
using static GearUpCards.Utils.CardUtils;

namespace GearUpCards.Cards
{
    class TacticalScannerCard : CustomCard
    {
        internal static GameObject cardArt = GearUpCards.CardArtBundle.LoadAsset<GameObject>("C_TacticalScanner");

        private class ScannerSpawner : MonoBehaviour
        {
            void Start()
            {

            }
        }

        public override void SetupCard(CardInfo cardInfo, Gun gun, ApplyCardStats cardStats, CharacterStatModifiers statModifiers, Block block)
        {
            
        }
        public override void OnAddCard(Player player, Gun gun, GunAmmo gunAmmo, CharacterData data, HealthHandler health, Gravity gravity, Block block, CharacterStatModifiers characterStats)
        {
            block.cdAdd += 0.5f;
            player.gameObject.GetOrAddComponent<TacticalScannerEffect>();
            characterStats.GetGearData().tacticalScannerStack += 1;

            CooldownUIMono cooldownUI = player.gameObject.GetOrAddComponent<CooldownUIMono>();
            // cooldownUI.FetchAbilities();
        }
        public override void OnRemoveCard(Player player, Gun gun, GunAmmo gunAmmo, CharacterData data, HealthHandler health, Gravity gravity, Block block, CharacterStatModifiers characterStats)
        {

            // UnityEngine.Debug.Log($"[{GearUpCards.ModInitials}][Card] {GetTitle()} has been removed to player {player.playerID}.");
        }
        protected override string GetTitle()
        {
            return "Tactical Scanner";
        }
        protected override string GetDescription()
        {
            return "Blocking scans nearby players' power:\n Scanned enemies take more damage.\nScanned friendlies heal for more.";
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
                new CardInfoStat()
                {
                    positive = true,
                    stat = "Scan AMP",
                    amount = "+20%",
                    simepleAmount = CardInfoStat.SimpleAmount.notAssigned
                },
                new CardInfoStat()
                {
                    positive = true,
                    stat = "Duration",
                    amount = "6s",
                    simepleAmount = CardInfoStat.SimpleAmount.notAssigned
                },
                new CardInfoStat()
                {
                    positive = false,
                    stat = "Block CD",
                    amount = "+0.5s",
                    simepleAmount = CardInfoStat.SimpleAmount.notAssigned
                },
                new CardInfoStat()
                {
                    positive = false,
                    stat = "Ability CD",
                    amount = "9s",
                    simepleAmount = CardInfoStat.SimpleAmount.notAssigned
                }
            };
        }
        protected override CardThemeColor.CardThemeColorType GetTheme()
        {
            return CardThemeColor.CardThemeColorType.TechWhite;
        }
        public override string GetModName()
        {
            return GearUpCards.ModInitials;
        }
        internal static void callback(CardInfo card)
        {
            card.gameObject.AddComponent<ExtraName>().text = "Active\nBlock";
        }
    }
}
