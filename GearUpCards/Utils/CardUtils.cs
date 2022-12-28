using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Linq;
using HarmonyLib;

using UnboundLib;
using UnboundLib.Cards;
using CardChoiceSpawnUniqueCardPatch.CustomCategories;

using UnityEngine;
using TMPro;
using RarityLib.Utils;
using GearUpCards.Extensions;

namespace GearUpCards.Utils
{
    public class CardUtils
    {
        public static class GearCategory
        {
            public static CardCategory typeGunMod = CustomCardCategories.instance.CardCategory("GearUp_GunMod");
            public static CardCategory typeBlockMod = CustomCardCategories.instance.CardCategory("GearUp_BlockMod");

            public static CardCategory typeCrystal = CustomCardCategories.instance.CardCategory("GearUp_Crystal");
            public static CardCategory typeCrystalGear = CustomCardCategories.instance.CardCategory("GearUp_CrystalUser");
            public static CardCategory typeCrystalMod = CustomCardCategories.instance.CardCategory("GearUp_CrystalMod");

            public static CardCategory typeSizeMod = CustomCardCategories.instance.CardCategory("GearUp_SizeMod");

            public static CardCategory typeUniqueMagick = CustomCardCategories.instance.CardCategory("GearUp_UniqueMagick");

            public static CardCategory typeUniqueGunSpread = CustomCardCategories.instance.CardCategory("GearUp_UniqueGunSpread");

            public static CardCategory tagNoRemove = CustomCardCategories.instance.CardCategory("NoRemove");
            public static CardCategory tagSpell = CustomCardCategories.instance.CardCategory("GearUp_Spell");
            public static CardCategory tagSpellOnlyAugment = CustomCardCategories.instance.CardCategory("GearUp_SpellOnlyAugment");
        }

        public class PlayerCardData
        {
            public CardInfo cardInfo;
            public Player owner;
            public int index;

            public PlayerCardData(CardInfo cardInfo, Player owner, int index)
            {
                this.cardInfo = cardInfo;
                this.owner = owner;
                this.index = index;
            }
        }

        public static bool PlayerHasCard(Player player, string cardName)
        {
            List<PlayerCardData> candidate = GetPlayerCardsWithName(player, cardName);
            return candidate.Count > 0;
        }

        public static bool PlayerHasCardCategory(Player player, CardCategory cardCategory)
        {
            List<PlayerCardData> candidate = GetPlayerCardsWithCategory(player, cardCategory);
            return candidate.Count > 0;
        }

        public static List<PlayerCardData> GetPlayerCardsWithName(Player player, string targetCardName)
        {
            targetCardName = targetCardName.ToUpper();
            string checkCardName;

            List<PlayerCardData> candidates = new List<PlayerCardData>();
            List<CardInfo> playerCards = player.data.currentCards;

            for (int i = 0; i < playerCards.Count; i++)
            {
                checkCardName = playerCards[i].cardName.ToUpper();

                if (checkCardName.Equals(targetCardName))
                {
                    candidates.Add(new PlayerCardData(playerCards[i], player, i));
                }
            }
            return candidates;
        }

        public static List<PlayerCardData> GetPlayerCardsWithCategory(Player player, CardCategory targetCategory)
        {
            List<PlayerCardData> candidates = new List<PlayerCardData>();
            List<CardInfo> playerCards = player.data.currentCards;

            for (int i = 0; i < playerCards.Count; i++)
            {
                bool match = false;
                CardCategory[] thisCardCat = CustomCardCategories.instance.GetCategoriesFromCard(playerCards[i]);
                foreach (CardCategory category in thisCardCat)
                {
                    if (targetCategory == category)
                    {
                        match = true;
                        break;
                    }
                }
                if (match)
                {
                    candidates.Add(new PlayerCardData(playerCards[i], player, i));
                }
            }
            return candidates;
        }

        public static CardInfo GetCardInfo(string modInitial, string cardNameExact)
        {
            string queryText = $"__{modInitial}__{cardNameExact}";
            List<CardInfo> cardInfoList = CardChoice.instance.cards.ToList();

            // Miscs.Log("GetCardInfo(exact) " + queryText);
            CardInfo result = null;
            foreach (CardInfo item in cardInfoList)
            {
                if (item.gameObject.name.Contains(queryText))
                {
                    result = item;
                    break;
                }
            }

            if (result == null)
            {
                Miscs.LogWarn("Cannot find: " + queryText);
            }
            else
            {
                Miscs.Log("Found: " + result.name);
            }
            return result;
        }

        public static CardInfo GetCardInfo(string query, bool searchVanillaOnly = false)
        {
            //would return the first hit in list of cards
            if (query.Contains("@"))
            {
                List<string> splitQuery = Miscs.StringSplit(query, '@');

                // foreach (var item in splitQuery)
                // {
                //     Miscs.Log("#> " + item);
                // }

                if (splitQuery.Count == 2)
                {
                    if (splitQuery[0] == "")
                    {
                        return GetCardInfo(splitQuery[1]);
                    }
                    else if (splitQuery[0] == "Vanilla")
                    {
                        return GetCardInfo(splitQuery[1], true);
                    }
                    return GetCardInfo(splitQuery[0], splitQuery[1]);
                }
                else
                {
                    Miscs.LogWarn(">> query splitting failed");
                    query = splitQuery[0];
                }
            }

            query = query.Replace(" ", "").ToLower();
            string cardName;
            List<CardInfo> cardInfoList = CardChoice.instance.cards.ToList();

            // if (!query.Contains("__"))
            // {
            //     searchVanillaOnly = true;
            // }

            Miscs.Log("GetCardInfo(query) " + query);
            CardInfo result = null;
            int matchScore = 0;
            foreach (CardInfo item in cardInfoList)
            {
                cardName = item.gameObject.name.Replace(" ", "").ToLower();
                if (searchVanillaOnly && cardName.Contains("__")) continue;
                // Logging here is process time-intensive
                // Miscs.Log($"> Check for [{query}] <-> [{cardName}]");

                if (cardName.Equals(query))
                {
                    result = item;
                    matchScore = 9999;
                    break;
                }
                else if (cardName.Contains(query))
                {
                    int newMatch = Miscs.ValidateStringQuery(cardName, query);
                    if (newMatch > matchScore)
                    {
                        matchScore = newMatch;
                        result = item;
                    }
                }
            }

            if (result == null)
            {
                Miscs.LogWarn("Cannot find: " + query);
            }
            else
            {
                Miscs.Log($"Found: {result.gameObject.name} [{matchScore}]");
            }
            return result;
        }

        public static List<PlayerCardData> GetPlayerCardsWithCardInfo(Player player, CardInfo cardInfo)
        {
            string targetCardName = cardInfo.gameObject.name.ToUpper();
            string checkCardName;

            List<PlayerCardData> candidates = new List<PlayerCardData>();
            List<CardInfo> playerCards = player.data.currentCards;

            for (int i = 0; i < playerCards.Count; i++)
            {
                checkCardName = playerCards[i].gameObject.name.ToUpper();

                if (checkCardName.Equals(targetCardName))
                {
                    candidates.Add(new PlayerCardData(playerCards[i], player, i));
                }
            }
            return candidates;
        }

        public static List<PlayerCardData> GetPlayerCardsWithStringList(Player player, List<string> checkList)
        {
            List<PlayerCardData> candidates = new List<PlayerCardData>();
            List<CardInfo> playerCards = player.data.currentCards;
            CardInfo tempCardInfo;

            foreach (string item in checkList)
            {
                tempCardInfo = GetCardInfo(item);
                if (tempCardInfo == null) continue;

                candidates = candidates.Concat(GetPlayerCardsWithCardInfo(player, tempCardInfo)).ToList();
            }

            return candidates;
        }

        // adjusting per-player card rarity modifiers
        public static List<string> gearUpRarityChecklist = new List<string>()
        {
            // spells
            "Anti-Bullet Magick",
            "Orb-literation!",

            // glyphs
            "Divination Glyph",
            "Geometric Glyph",
            "Influence Glyph",
            "Magick Fragments",
            "Potency Glyph",

            // C&C cards
            "Tiberium Bullet",

            // others
            "Glyph CAD Module",
            "Shield Battery"
        };
        public static Dictionary<string, float> raritySnapshot = new Dictionary<string, float>();

        public static List<string> cardListSpells = new List<string>()
        {
            // GearUp spells
            "Anti-Bullet Magick",
            "Orb-literation!"
        };
        public static List<string> cardListGlyph = new List<string>()
        {
            // glyphs
            "Divination Glyph",
            "Geometric Glyph",
            "Influence Glyph",
            "Magick Fragments",
            "Potency Glyph"
        };

        public static List<string> cardListVanillaBlocks = new List<string>()
        {
            "Vanilla@Empower",

            "Vanilla@Bombs Away",
            "Vanilla@Defender",
            "Vanilla@EMP",
            "Vanilla@Frost Slam",
            "Vanilla@Healing Field",
            "Vanilla@Implode",
            "Vanilla@Overpower",
            "Vanilla@Radar Shot",
            "Vanilla@Saw",
            "Vanilla@Shield Charge",
            "Vanilla@Shockwave",
            "Vanilla@Silence",
            "Vanilla@Static Field",
            "Vanilla@Supernova",
            "Vanilla@Teleport"
        };
        public static List<string> cardListModdedBlocks = new List<string>()
        {
            // GearUp Cards - GearUP
            "GearUP@Magick Fragments",
            "GearUP@Shield Battery",
            "GearUP@Tactical Scanner",

            // Willis' Cards Plus - Cards+
            "Cards+@Turtle",

            // Cosmic Rounds - CR
            "CR@Aqua Ring",
            "CR@Barrier",
            "CR@Gravity",
            "CR@Halo",
            "CR@Heartition",
            "CR@Hive",
            "CR@Holster",
            "CR@Ignite",
            "CR@Mitosis",
            "CR@Ping",
            "CR@Speed Up",
            //* "Taser",

            // HatchetDaddy's Cards - HDC
            "HDC@Divine Blessing",
            "HDC@Lil Defensive",

            // Root's Cards - Root
            "Root@Quick Shield",

            // Pykess's Cards Extended - PCE
            "PCE@Discombobulate",
            "PCE@Super Jump"
        };

        public static void SaveCardRarity()
        {
            Miscs.Log("[GearUp] SaveGearUpCardRarity()");
            CardInfo tempCardInfo;
            string tempCardName;

            // Miscs.Log("[GearUp] SaveGearUpCardRarity - GearUP");
            foreach (string item in gearUpRarityChecklist)
            {
                tempCardName = GearUpCards.ModInitials + "@" + item;
                tempCardInfo = GetCardInfo(tempCardName);
                if (tempCardInfo == null) continue;

                if (!raritySnapshot.ContainsKey(item))
                {
                    raritySnapshot.TryAdd
                    (
                        tempCardName,
                        RarityUtils.GetCardRarityModifier(tempCardInfo)
                    );
                }
                else
                {
                    raritySnapshot[tempCardName] = RarityUtils.GetCardRarityModifier(tempCardInfo);
                }
            }

            // Miscs.Log("[GearUp] SaveGearUpCardRarity - Vanilla Blocks");
            foreach (var item in cardListVanillaBlocks)
            {
                // Miscs.Log("> " + item);
                tempCardInfo = GetCardInfo(item);
                if (tempCardInfo == null) continue;

                if (!raritySnapshot.ContainsKey(item))
                {
                    raritySnapshot.TryAdd
                    (
                        item,
                        RarityUtils.GetCardRarityModifier(tempCardInfo)
                    );
                }
                else
                {
                    raritySnapshot[item] = RarityUtils.GetCardRarityModifier(tempCardInfo);
                }
            }

            // Miscs.Log("[GearUp] SaveGearUpCardRarity - Modded Blocks");
            foreach (var item in cardListModdedBlocks)
            {
                // Miscs.Log("> " + item);
                tempCardInfo = GetCardInfo(item);
                if (tempCardInfo == null) continue;

                if (!raritySnapshot.ContainsKey(item))
                {
                    raritySnapshot.TryAdd
                    (
                        item,
                        RarityUtils.GetCardRarityModifier(tempCardInfo)
                    );
                }
                else
                {
                    raritySnapshot[item] = RarityUtils.GetCardRarityModifier(tempCardInfo);
                }
            }
        }

        public static void RestoreGearUpCardRarity()
        {
            Miscs.Log("[GearUp] RestoreGearUpCardRarity()");
            CardInfo tempCardInfo;
            float tempMul;

            foreach (string item in raritySnapshot.Keys)
            {
                // Miscs.Log("> " + item);
                tempCardInfo = GetCardInfo(item);
                if (tempCardInfo == null) continue;

                // RarityUtils.SetCardRarityModifier
                // (
                //     tempCardInfo,
                //     raritySnapshot[item]
                // );

                tempMul = RarityUtils.GetCardRarityModifier(tempCardInfo);
                RarityUtils.AjustCardRarityModifier(tempCardInfo, mul: -1.0f * ((tempMul / raritySnapshot[item]) - 1.0f));

                // Miscs.Log("Restored! - " + raritySnapshot[item]);
                // Miscs.Log("Rarity set to: " + RarityUtils.GetCardRarityModifier(tempCardInfo));
            }
        }

        public static void BatchAdjustCardRarity(List<string> cardList, float add = 0.0f, float mul = 0.0f)
        {
            CardInfo tempCardInfo;

            foreach (string item in cardList)
            {
                // Miscs.Log($"> {item}");
                tempCardInfo = GetCardInfo(item);
                if (tempCardInfo == null) continue;

                RarityUtils.AjustCardRarityModifier(tempCardInfo, add, mul);
                // Miscs.Log("Got it");
            }
        }

        public static void ModifyPerPlayerCardRarity(int playerID)
        {
            // Miscs.Log($"[GearUp] ModifyPerPlayerCardRarity({playerID})");
            Player targerPlayer = null;
            if (playerID <= -1) return;

            foreach (Player item in PlayerManager.instance.players)
            {
                if (item.playerID == playerID)
                {
                    targerPlayer = item;
                    break;
                }
            }

            if (targerPlayer == null) return;
            CharacterStatModifiers targetStats = targerPlayer.gameObject.GetComponent<CharacterStatModifiers>();
            CharacterStatModifiersGearData gearData = targetStats.GetGearData();

            // spells and glyphs
            float tempModifier = 0.0f;

            tempModifier += gearData.glyphDivination * 2.00f;
            tempModifier += gearData.glyphGeometric * 0.50f;
            tempModifier += gearData.glyphInfluence * 1.00f;
            tempModifier += gearData.glyphPotency * 0.50f;
            tempModifier += gearData.magickFragmentStack * 0.50f;

            // Miscs.Log(">.<");
            // Miscs.Log("> Glyph base modifier: " + tempModifier);
            if (gearData.addOnList.Contains(GearUpConstants.AddOnType.cadModuleGlyph))
            {
                tempModifier *= 1.25f;
                BatchAdjustCardRarity(cardListGlyph, mul: 1.00f);
            }
            else
            {
                RarityUtils.AjustCardRarityModifier
                (
                    GetCardInfo(GearUpCards.ModInitials, "Glyph CAD Module"),
                    mul: tempModifier * 1.50f
                );
            }
            BatchAdjustCardRarity(cardListSpells, mul: tempModifier);

            // [Empower] + [Shield Battery] boosting>> block abilities find chance
            tempModifier = (float)(gearData.shieldBatteryStack) * 0.25f;
            tempModifier += (float)(GetPlayerCardsWithName(targerPlayer, "Empower").Count) * 0.25f;
            tempModifier += gearData.magickFragmentStack * 0.50f;

            // Miscs.Log(">.<");
            // Miscs.Log("> Block base modifier: " + tempModifier);
            RarityUtils.AjustCardRarityModifier
            (
                GetCardInfo(GearUpCards.ModInitials, "Shield Battery"),
                mul: tempModifier * 2.0f
            );
            RarityUtils.AjustCardRarityModifier
            (
                GetCardInfo("Empower", true),
                mul: tempModifier * 2.0f
            );
            BatchAdjustCardRarity(cardListVanillaBlocks, mul: tempModifier * 2.0f);
            BatchAdjustCardRarity(cardListModdedBlocks, mul: tempModifier);

            // blocking ability boosting>> [Empower] + [Shield Battery] find chance
            List<PlayerCardData> tempList = GetPlayerCardsWithStringList(targerPlayer, cardListVanillaBlocks);

            Miscs.Log($"Player has vanilla blocks:");
            foreach (PlayerCardData item in tempList)
            {
                Miscs.Log($"> {item.cardInfo.gameObject.name} @ {item.index}");
            }

            tempModifier = (float)(tempList.Count) * 0.35f;

            tempList = GetPlayerCardsWithStringList(targerPlayer, cardListModdedBlocks);

            Miscs.Log($"Player has modded blocks:");
            foreach (PlayerCardData item in tempList)
            {
                Miscs.Log($"> {item.cardInfo.gameObject.name} @ {item.index}");
            }

            tempModifier += (float)(tempList.Count) * 0.20f;

            RarityUtils.AjustCardRarityModifier
            (
                GetCardInfo(GearUpCards.ModInitials, "Shield Battery"),
                mul: tempModifier
            );
            RarityUtils.AjustCardRarityModifier
            (
                GetCardInfo("Empower", true),
                mul: tempModifier
            );
        }

        // extra text (bottom-right) of the card, credit to Pykess
        public class DestroyOnUnparent : MonoBehaviour
        {
            void LateUpdate()
            {
                if (this.gameObject.transform.parent == null) { Destroy(this.gameObject); }
            }
        }

        internal class ExtraName : MonoBehaviour
        {
            private static GameObject _extraTextObj = null;
            internal static GameObject extraTextObj
            {
                get
                {
                    if (_extraTextObj != null) { return _extraTextObj; }

                    _extraTextObj = new GameObject("ExtraCardText", typeof(TextMeshProUGUI), typeof(DestroyOnUnparent));
                    DontDestroyOnLoad(_extraTextObj);
                    return _extraTextObj;


                }
                private set { }
            }

            public string text = "";

            public void Start()
            {
                // add extra text to bottom right
                // create blank object for text, and attach it to the canvas
                // find bottom right edge object
                RectTransform[] allChildrenRecursive = this.gameObject.GetComponentsInChildren<RectTransform>();
                GameObject BottomLeftCorner = allChildrenRecursive.Where(obj => obj.gameObject.name == "EdgePart (1)").FirstOrDefault().gameObject;
                GameObject modNameObj = UnityEngine.GameObject.Instantiate(extraTextObj, BottomLeftCorner.transform.position, BottomLeftCorner.transform.rotation, BottomLeftCorner.transform);
                TextMeshProUGUI modText = modNameObj.gameObject.GetComponent<TextMeshProUGUI>();
                modText.text = text;
                modText.enableWordWrapping = false;
                modNameObj.transform.Rotate(0f, 0f, 135f);
                modNameObj.transform.localScale = new Vector3(1f, 1f, 1f);
                modNameObj.transform.localPosition = new Vector3(-50f, -50f, 0f);
                modText.alignment = TextAlignmentOptions.Bottom;
                modText.alpha = 0.1f;
                modText.fontSize = 50;
            }
        }
    }
}
