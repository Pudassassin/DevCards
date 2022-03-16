using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnboundLib.GameModes;
using UnityEngine;
using static GearUpCards.Utils.CardUtils;
using CardChoiceSpawnUniqueCardPatch.CustomCategories;

namespace GearUpCards.MonoBehaviours
{
    internal class CardHandResolveMono : MonoBehaviour
    {
        // private const float procTime = 1.0f;
        // private const float resolveDelay = 0.5f;
        
        // internal Player player;
        // internal CharacterStatModifiers stats;
        
        // internal float timer = 0.0f;
        // internal float timeResolveCalled = 0.0f;
        // internal bool manualResolve = false;

        /* DEBUG */
        // internal int proc_count = 0;


        // public void Awake()
        // {
        //     this.player = this.gameObject.GetComponent<Player>();
        //     this.stats = this.gameObject.GetComponent<CharacterStatModifiers>();
        // 
        //     GameModeManager.AddHook(GameModeHooks.HookRoundStart, OnRoundStart);
        //     // GameModeManager.AddHook(GameModeHooks.HookPointEnd, OnPointEnd);
        // }

        // public void Start()
        // {
        // 
        // }

        // public void Update()
        // {
        //     if (manualResolve && Time.time > timeResolveCalled + resolveDelay)
        //     {
        //         StartCoroutine(ResolveHandCards());
        //         manualResolve = false;
        //     }
        // }

        // private IEnumerator OnPointEnd(IGameModeHandler gm)
        // {
        //     yield break;
        // }

        // public void OnDisable()
        // {
        //     bool isRespawning = player.data.healthHandler.isRespawning;
        //     // UnityEngine.Debug.Log($"[HOLLOW] from player [{player.playerID}] - is resurresting [{isRespawning}]");
        // 
        //     if (isRespawning)
        //     {
        //         // does nothing
        //         // UnityEngine.Debug.Log($"[HOLLOW] from player [{player.playerID}] - is resurresting!?");
        //     }
        //     else
        //     {
        //         // UnityEngine.Debug.Log($"[HOLLOW] from player [{player.playerID}] - dead ded!?");
        //     }
        // }

        // public void OnDestroy()
        // {
        //     GameModeManager.RemoveHook(GameModeHooks.HookPointStart, OnRoundStart);
        //     // GameModeManager.RemoveHook(GameModeHooks.HookPointEnd, OnPointEnd);
        // }

        // public void Refresh()
        // {
        //     this.Awake();
        // }

        // public void TriggerResolve()
        // {
        //     manualResolve = true;
        //     timeResolveCalled = Time.time;
        // }

        // private IEnumerator OnRoundStart(IGameModeHandler gm)
        // {
        //     UnityEngine.Debug.Log($"Player[{player.playerID}] RoundStart Call");
        // 
        //     yield return new WaitForSecondsRealtime(.2f);
        //     StartCoroutine(ResolveHandCards());
        //     yield break;
        // }

        public static IEnumerator ResolveHandCards(Player player)
        {
            yield return ResolveCardCategory(player, GearCategory.typeSizeMod, "Medical Parts");
            yield return ResolveCardCategory(player, GearCategory.typeUniqueMagick, "Magick Fragments");

            yield return UpdateCategoryBlacklist(player, GearCategory.typeSizeMod);
            yield return UpdateCategoryBlacklist(player, GearCategory.typeUniqueMagick);

            yield break;
        }

        public static IEnumerator ResolveCardCategory(Player player, CardCategory category, string cardNameToAdd)
        {
            // Resolve card conflicts
            List<HandCardData> conflictedCards = GetPlayerCardsWithCategory(player, category);
            foreach (var item in conflictedCards)
            {
                UnityEngine.Debug.Log($"[{item.cardInfo.cardName}] - [{item.index}] - [{item.owner.playerID}]");
            }

            if (conflictedCards.Count >= 1)
            {
                var replacementCard = ModdingUtils.Utils.Cards.all.Where(card => card.name == cardNameToAdd).ToArray()[0];

                for (int i = conflictedCards.Count - 1; i > 0; i--)
                {
                    // ModdingUtils.Utils.Cards.instance.AddCardToPlayer
                    // (
                    //     player, replacementCard, false, cardNameToAdd[..2], 0.0f, 0.0f
                    // );
                    // 
                    // yield return new WaitForSecondsRealtime(0.2f);
                    // 
                    // ModdingUtils.Utils.Cards.instance.RemoveCardFromPlayer
                    // (
                    //     player, conflictedCards[i].index, true
                    // );
                    // 
                    // yield return new WaitForSecondsRealtime(0.2f);

                    yield return ModdingUtils.Utils.Cards.instance.ReplaceCard
                    (
                        player, conflictedCards[i].index, replacementCard, cardNameToAdd[..2], 2.0f, 2.0f, true
                    );

                    yield return new WaitForSecondsRealtime(0.2f);

                }
            }
        }

        public static IEnumerator UpdateCategoryBlacklist(Player player, CardCategory categoryToCheck)
        {
            // mutally exclusives, desinated unique card, etc.
            List<HandCardData> cardToCheck = GetPlayerCardsWithCategory(player, categoryToCheck);
            UnityEngine.Debug.Log($"Player[{player.playerID}] Check blacklist for [{categoryToCheck.name}] >> found [{cardToCheck.Count}]");
            if (cardToCheck.Count == 0)
            {
                ModdingUtils.Extensions.CharacterStatModifiersExtension.GetAdditionalData(player.data.stats).blacklistedCategories.RemoveAll
                (
                    (category) => category == categoryToCheck
                );
            }
            else
            {
                ModdingUtils.Extensions.CharacterStatModifiersExtension.GetAdditionalData(player.data.stats).blacklistedCategories.Add(categoryToCheck);
            }

            yield break;
        }

        public static IEnumerator UpdateCategoryWhitelist(Player player, CardCategory categoryToCheck)
        {
            // unlocking condition, etc.
            List<HandCardData> cardToCheck = GetPlayerCardsWithCategory(player, categoryToCheck);

            if (cardToCheck.Count > 0)
            {
                ModdingUtils.Extensions.CharacterStatModifiersExtension.GetAdditionalData(player.data.stats).blacklistedCategories.RemoveAll
                (
                    (category) => category == categoryToCheck
                );
            }
            else
            {
                ModdingUtils.Extensions.CharacterStatModifiersExtension.GetAdditionalData(player.data.stats).blacklistedCategories.Add(categoryToCheck);
            }

            yield break;
        }
    }
}
