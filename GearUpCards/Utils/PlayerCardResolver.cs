using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnboundLib.GameModes;
using UnityEngine;
using static GearUpCards.Utils.CardUtils;
using CardChoiceSpawnUniqueCardPatch.CustomCategories;

namespace GearUpCards.Utils
{
    internal class PlayerCardResolver : MonoBehaviour
    {
        public static IEnumerator Resolve(Player player)
        {
            yield return ResolveCardCategory(player, GearCategory.typeSizeMod, "Medical Parts");
            yield return ResolveCardCategory(player, GearCategory.typeUniqueMagick, "Magick Fragments");
            yield return ResolveCardCategory(player, GearCategory.typeUniqueGunSpread, "Gun Parts");

            yield return UpdateCategoryBlacklist(player, GearCategory.typeSizeMod);
            yield return UpdateCategoryBlacklist(player, GearCategory.typeUniqueMagick);
            yield return UpdateCategoryBlacklist(player, GearCategory.typeUniqueGunSpread);

            yield break;
        }

        public static IEnumerator ResolveCardCategory(Player player, CardCategory category, string cardNameToAdd)
        {
            // Resolve card conflicts
            List<PlayerCardData> conflictedCards = GetPlayerCardsWithCategory(player, category);
            foreach (var item in conflictedCards)
            {
                UnityEngine.Debug.Log($"[{item.cardInfo.cardName}] - [{item.index}] - [{item.owner.playerID}]");
            }

            if (conflictedCards.Count >= 1)
            {
                var replacementCard = ModdingUtils.Utils.Cards.all.Where(card => card.name == cardNameToAdd).ToArray()[0];

                for (int i = conflictedCards.Count - 1; i > 0; i--)
                {
                    yield return ModdingUtils.Utils.Cards.instance.ReplaceCard
                    (
                        player, conflictedCards[i].index, replacementCard, cardNameToAdd[..2], 2.0f, 2.0f, true
                    );

                    yield return new WaitForSecondsRealtime(0.1f);
                }
            }
        }

        public static IEnumerator UpdateCategoryBlacklist(Player player, CardCategory categoryToCheck)
        {
            // mutally exclusives, desinated unique card, etc.
            List<PlayerCardData> cardToCheck = GetPlayerCardsWithCategory(player, categoryToCheck);
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
            List<PlayerCardData> cardToCheck = GetPlayerCardsWithCategory(player, categoryToCheck);

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
