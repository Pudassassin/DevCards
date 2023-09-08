using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;

using UnityEngine;
using SoundImplementation;

using UnboundLib;
using UnboundLib.GameModes;
using Photon.Pun;
using ModdingUtils.MonoBehaviours;

using GearUpCards;
using GearUpCards.Extensions;
using GearUpCards.Utils;

using HarmonyLib;
using UnboundLib.Utils;
using RarityLib.Utils;
using ModdingUtils.Extensions;

namespace GearUpCards.MonoBehaviours
{
    internal class CardDrawTracker : MonoBehaviour
    {
        // private static GameObject empowerShotVFX = GearUpCards.VFXBundle.LoadAsset<GameObject>("VFX_EmpowerShot");
        // internal bool addShotVFX = false;
        public static List<Player> extraDrawPlayerQueue = new List<Player>();

        public class ExtraCardDraw
        {
            public int count;
            public int roundDelay;
            public List<CardCategory> whitelistTags = new List<CardCategory>();
            public List<CardCategory> whitelistPacks = new List<CardCategory>();
            public List<Rarity> whitelistRarities = new List<Rarity>();

            // for overriding blacklisted category to temporary show up
            public List<CardCategory> undoBlacklistCategories = new List<CardCategory>();

            public List<CardCategory> whitelistRarityCats = new List<CardCategory>();
            public List<CardCategory> blacklistCategories = new List<CardCategory>();

            // simple extra draws
            public ExtraCardDraw(int count, int roundDelay = 0)
            {
                this.count = count;
                this.roundDelay = roundDelay;

                blacklistCategories.Add(CardUtils.GearCategory.typeBoosterPack);
                blacklistCategories.Add(CardUtils.GearCategory.tagCardManipulation);
            }

            // set extra draws from specific rarities
            public void SetWhitelistRarities(List<Rarity> exactRarities)
            {
                this.whitelistRarities.AddRange(exactRarities);
                foreach (var item in whitelistRarities)
                {
                    whitelistRarityCats.Add(CardUtils.rarityCategories[item]);
                }

                SetBlacklistRarity();
            }

            // set extra draw from rarity range
            public void SetWhitelistRarityRange(Rarity rarity, bool includeLower = false, bool includeHigher = false)
            {
                SetWhitelistRarities(GetRarityRange(rarity, includeLower, includeHigher));
            }

            // set extra draw from specific card packs
            public void SetWhitelistCardPacks(List<string> cardPacks)
            {
                CardCategory packCategory;
                foreach (string cardPack in cardPacks)
                {
                    if (CardUtils.packCategories.TryGetValue(cardPack, out packCategory))
                    {
                        this.whitelistPacks.Add(packCategory);
                    }
                }

                SetBlacklistPack();
            }

            // set extra draw from specific type of GearUP cards
            public void SetWhitelistGearUpCard(List<CardCategory> gearCategories)
            {
                this.whitelistPacks.Add(CardUtils.packCategories[GearUpCards.ModInitials]);
                this.whitelistTags.AddRange(gearCategories);

                foreach (var item in CardUtils.GearCategory.GearCategories)
                {
                    if (!whitelistTags.Contains(item))
                    {
                        blacklistCategories.Add(item);
                    }
                }

                SetBlacklistPack();
            }

            // !! override category blacklist, to be used last !!
            public void OverrideCategories(List<CardCategory> cardCategories, bool allow = true)
            {
                foreach (var item in cardCategories)
                {
                    if (allow)
                    {
                        if (!undoBlacklistCategories.Contains(item))
                        {
                            undoBlacklistCategories.Add(item);
                        }
                        if (blacklistCategories.Contains(item))
                        {
                            blacklistCategories.Remove(item);
                        }
                    }
                    else
                    {
                        if (undoBlacklistCategories.Contains(item))
                        {
                            undoBlacklistCategories.Remove(item);
                        }
                        if (!blacklistCategories.Contains(item))
                        {
                            blacklistCategories.Add(item);
                        }
                    }
                }
            }

            // Methods
            private static List<Rarity> GetRarityRange(Rarity rarity, bool includeLower = false, bool includeHigher = false)
            {
                // List<CardCategory> resultCategories = new List<CardCategory>();
                List<Rarity> rarityWhitelist = new List<Rarity>();

                // default: Common is 1, Rare is 0.1
                float targetRelativeRarity = rarity.relativeRarity;

                // resultCategories.Add(CardUtils.rarityCategories[rarity]);
                rarityWhitelist.Add(rarity);
                if (includeLower)
                {
                    foreach (Rarity item in CardUtils.rarityCategories.Keys)
                    {
                        if (item.relativeRarity > targetRelativeRarity)
                        {
                            // resultCategories.Add(CardUtils.rarityCategories[item]);
                            rarityWhitelist.Add(item);
                        }
                    }
                }
                if (includeHigher)
                {
                    foreach (Rarity item in CardUtils.rarityCategories.Keys)
                    {
                        if (item.relativeRarity < targetRelativeRarity)
                        {
                            // resultCategories.Add(CardUtils.rarityCategories[item]);
                            rarityWhitelist.Add(item);
                        }
                    }
                }

                return rarityWhitelist;
            }

            private void SetBlacklistRarity()
            {
                foreach (var item in CardUtils.rarityCategories.Values)
                {
                    if (!whitelistRarityCats.Contains(item))
                    {
                        blacklistCategories.Add(item);
                    }
                }
            }

            private void SetBlacklistPack()
            {
                foreach (var item in CardUtils.packCategories.Values)
                {
                    if (!whitelistPacks.Contains(item))
                    {
                        blacklistCategories.Add(item);
                    }
                }
            }
        }

        public List<ExtraCardDraw> extraCardDraws = new List<ExtraCardDraw>();
        public List<ExtraCardDraw> extraCardDrawsDelayed = new List<ExtraCardDraw>();

        // internals
        private const float procTickTime = .10f;
        private bool isResolving = false;

        internal float procTimer = 0.0f;
        internal bool effectEnabled = false;
        // internal int proc_count = 0;

        internal Player player;
        internal CharacterStatModifiers stats;

        // extra card draws

        public void Awake()
        {
            player = gameObject.GetComponent<Player>();
            stats = gameObject.GetComponent<CharacterStatModifiers>();

            GameModeManager.AddHook(GameModeHooks.HookPointStart, OnPointStart);
            GameModeManager.AddHook(GameModeHooks.HookPointEnd, OnPointEnd);

            GameModeManager.AddHook(GameModeHooks.HookPickEnd, OnPickEnd);
            GameModeManager.AddHook(GameModeHooks.HookPickStart, OnPickStart);

            GameModeManager.AddHook(GameModeHooks.HookGameStart, OnRematch);
        }

        public void Start()
        {

        }

        public void Update()
        {

        }

        //public void RefreshStatsPreRound()
        //{
        //
        //}

        // Methods
        public void QueueExtraDraw(ExtraCardDraw extraCardDraw)
        {
            if (extraCardDraw.roundDelay > 0)
            {
                extraCardDrawsDelayed.Add(extraCardDraw);
            }
            else
            {
                extraCardDraws.Add(extraCardDraw);

                if (extraDrawPlayerQueue.Count == 0)
                {
                    extraDrawPlayerQueue.Add(this.player);
                }
                else if (extraDrawPlayerQueue[extraDrawPlayerQueue.Count-1] != player)
                {
                    extraDrawPlayerQueue.Add(this.player);
                }
            }
        }

        public IEnumerator ResolveExtraDraws()
        {
            Miscs.Log("[GearUpCard] CardDrawTracker.ResolveExtraDraws()");
            if (extraCardDraws.Count <= 0 || isResolving)
            {
                yield break;
            }

            this.isResolving = true;
            // store what category was blacklisted (or not) prior to this; if run into pre-existing blacklists
            // true  => it was blacklisted and got removed  | add back in when restoring
            // false => it was not blacklisted and is added | remove it when restoring
            Dictionary<CardCategory, bool> blacklistDelta;
            List<CardCategory> playerBlacklist = player.data.stats.GetAdditionalData().blacklistedCategories;

            // while (Extensions.CharacterStatModifiersExtension.GetAdditionalData(player.data.stats).markovChoice > 0)
            for (int drawQueue = 0; drawQueue < extraCardDraws.Count; drawQueue++)
            {
                blacklistDelta = new Dictionary<CardCategory, bool>();

                Miscs.Log("[GearUpCard] CardDrawTracker.ResolveExtraDraws() : player's blacklist before VVV");
                string logBlacklist = "";
                foreach (var item in playerBlacklist)
                {
                    logBlacklist += item + ", ";
                }
                Miscs.Log(logBlacklist);

                // A) add to blacklist temporarily
                Miscs.Log("[GearUpCard] CardDrawTracker.ResolveExtraDraws() : edit blacklist");
                foreach (var item in extraCardDraws[drawQueue].blacklistCategories)
                {
                    // check if already blacklisted
                    if (playerBlacklist.Contains(item))
                    {
                        // it's there, no changes
                        continue;
                    }
                    else
                    {
                        // it's not there, add it and save the changes
                        blacklistDelta.TryAdd(item, false);
                        playerBlacklist.Add(item);
                    }
                }

                // undo (remove from) blacklist temporarily
                foreach (var item in extraCardDraws[drawQueue].undoBlacklistCategories)
                {
                    // check if already blacklisted
                    if (playerBlacklist.Contains(item))
                    {
                        // it's there, remove it and save the changes
                        blacklistDelta.TryAdd(item, true);
                        playerBlacklist.Remove(item);
                        continue;
                    }
                    else
                    {
                        // it's not there, no change
                    }
                }

                while (extraCardDraws[drawQueue].count > 0)
                {
                    extraCardDraws[drawQueue].count--;

                    yield return GameModeManager.TriggerHook(GameModeHooks.HookPlayerPickStart);

                    // A //

                    Miscs.Log("[GearUpCard] CardDrawTracker.ResolveExtraDraws() : show card");
                    CardChoiceVisuals.instance.Show(Enumerable.Range(0, PlayerManager.instance.players.Count).Where(i => PlayerManager.instance.players[i].playerID == player.playerID).First(), true);
                    yield return CardChoice.instance.DoPick(1, player.playerID, PickerType.Player);
                    yield return new WaitForSecondsRealtime(0.1f);

                    yield return GameModeManager.TriggerHook(GameModeHooks.HookPlayerPickEnd);

                    // B //

                    yield return new WaitForSecondsRealtime(0.1f);
                }

                // B) restore original blacklist that had changed via this method; unlisted one will not be changed
                Miscs.Log("[GearUpCard] CardDrawTracker.ResolveExtraDraws() : undo blacklist");
                foreach (var item in blacklistDelta.Keys)
                {
                    // true  => it was blacklisted and got removed  | add back in when restoring
                    if (blacklistDelta[item] == true)
                    {
                        if (!playerBlacklist.Contains(item))
                        {
                            playerBlacklist.Add(item);
                        };
                    }
                    // false => it was not blacklisted and is added | remove it when restoring
                    else
                    {
                        playerBlacklist.Remove(item);
                    }
                }

                Miscs.Log("[GearUpCard] CardDrawTracker.ResolveExtraDraws() : player's blacklist after VVV");
                logBlacklist = "";
                foreach (var item in playerBlacklist)
                {
                    logBlacklist += item + ", ";
                }
                Miscs.Log(logBlacklist);
            }

            Miscs.Log("[GearUpCard] CardDrawTracker.ResolveExtraDraws() : finishing");
            extraCardDraws.Clear();

            this.isResolving = false;
            yield break;
        }

        // Event methods
        private IEnumerator OnPickEnd(IGameModeHandler gm)
        {
            

            yield break;
        }

        private IEnumerator OnPickStart(IGameModeHandler gm)
        {
            

            yield break;
        }

        private IEnumerator OnPointStart(IGameModeHandler gm)
        {
            // effectEnabled = true;
            // procTimer = 0.0f;

            foreach (var item in extraCardDrawsDelayed)
            {
                item.roundDelay -= 1;
                if (item.roundDelay <= 0)
                {
                    extraCardDraws.Add(item);
                    if (!extraDrawPlayerQueue.Contains(this.player))
                    {
                        extraDrawPlayerQueue.Add(this.player);
                    }
                }
            }
            foreach (var item in extraCardDraws)
            {
                if (extraCardDrawsDelayed.Contains(item))
                {
                    extraCardDrawsDelayed.Remove(item);
                }
            }

            yield break;
        }

        private IEnumerator OnPointEnd(IGameModeHandler gm)
        {
            // effectEnabled = false;

            yield break;
        }

        private IEnumerator OnRematch(IGameModeHandler gm)
        {
            Destroy(this);
            yield break;
        }

        public void OnDisable()
        {

        }

        public void OnDestroy()
        {
            // This effect should persist between rounds, and at 0 stack it should do nothing mechanically
            // UnityEngine.Debug.Log($"Destroying Scanner  [{this.player.playerID}]");

            GameModeManager.RemoveHook(GameModeHooks.HookPointStart, OnPointStart);
            GameModeManager.RemoveHook(GameModeHooks.HookPointEnd, OnPointEnd);

            GameModeManager.RemoveHook(GameModeHooks.HookPickEnd, OnPickEnd);
            GameModeManager.RemoveHook(GameModeHooks.HookPickStart, OnPickStart);

            GameModeManager.RemoveHook(GameModeHooks.HookGameStart, OnRematch);
        }
    }
}
