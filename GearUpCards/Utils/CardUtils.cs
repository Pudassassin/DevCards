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
