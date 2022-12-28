using System.Collections.Generic;
using System.Collections;
using System;
using System.Reflection;

using UnboundLib;
using UnityEngine;
using System.Linq;
using UnboundLib.GameModes;
using Photon.Pun;
using ModdingUtils.MonoBehaviours;

using GearUpCards.Extensions;
using GearUpCards.Utils;

namespace GearUpCards.MonoBehaviours
{
    internal class SizeNormalizerEffect : MonoBehaviour
    {
        private const float procTime = 0.1f;

        internal Player player;
        internal CharacterStatModifiers stats;

        internal float prevSizeModifier;
        internal float prevMaxHealth;

        internal float prevTotalScale;
        internal float totalScaleTarget;
        internal float targetScale;

        internal float timer = 0.0f;
        internal bool effectEnabled;
        internal bool effectApplied;
        internal bool wasDeactivated = false;

        public void Awake()
        {
            this.player = this.gameObject.GetComponent<Player>();
            this.stats = this.gameObject.GetComponent<CharacterStatModifiers>();

            Refresh();

            GameModeManager.AddHook(GameModeHooks.HookPointStart, OnPointStart);
            GameModeManager.AddHook(GameModeHooks.HookRoundEnd, OnRoundEnd);
        }

        public void Start()
        {

        }

        // [*] [Brawler] and [Pristine Perserverence] will make player size temporary bigger than what [Size Norm.] supposed to keep due to the MAX HP increases
        // [*] potential issue with [Grow Others] and similar where it won't respond to the forced size increases
        // [!] severely reduce [Overpower] knockback force due to being locked down to default size (still do proper damage)

        public void Update()
        {
            timer += TimeHandler.deltaTime;

            // Respawn case
            if (wasDeactivated)
            {
                Refresh();

                wasDeactivated = false;
            }

            if (timer > procTime)
            {
                if (effectEnabled)
                {
                    // if (!StatsMath.ApproxEqual(this.player.data.maxHealth, prevMaxHealth))
                    // {
                    //     effectApplied = false;
                    //     prevMaxHealth = this.player.data.maxHealth;
                    //     totalScaleBefore = Mathf.Pow(this.player.data.maxHealth / 100f * 1.2f, 0.2f) * prevSizeModifier;
                    // }

                    float currentTotalScale = Mathf.Pow(player.data.maxHealth / 100f * 1.2f, 0.2f) * stats.sizeMultiplier;
                    if (!StatsMath.ApproxEqual(currentTotalScale, prevTotalScale))
                    {
                        Refresh();
                    }
                }

                if (effectEnabled && !effectApplied)
                {
                    if (prevTotalScale >= 1.2f)
                    {
                        targetScale = 1.65f - (0.5f * Mathf.Pow(0.9f, this.prevTotalScale));
                    }
                    else
                    {
                        targetScale = 0.1f + Mathf.Pow(1.1f, this.prevTotalScale);
                    }

                    totalScaleTarget = prevSizeModifier * targetScale / prevTotalScale;

                    this.player.gameObject.GetOrAddComponent<SizeNormalizerStatus>().SetSize(targetScale / prevTotalScale);
                    effectApplied = true;

                    // UnityEngine.Debug.Log($"[SizeNorm] adjusting... [{player.playerID}] [{totalScaleBefore}] >> [{targetScale}] == [{totalScaleAfter}] >> [{stats.sizeMultiplier}]");
                }

                if (effectEnabled && effectApplied)
                {
                    // this.player.gameObject.GetOrAddComponent<SizeNormalizerStatus>().SetSize(targetScale / totalScaleBefore);
                }

                if (!effectEnabled && !effectApplied)
                {
                    this.player.gameObject.GetOrAddComponent<SizeNormalizerStatus>().SetSize(1.0f);
                    effectApplied = true;
                }
                timer -= procTime;
            }


        }

        private IEnumerator OnPointStart(IGameModeHandler gm)
        {
            wasDeactivated = false;

            Refresh();

            yield break;
        }

        private IEnumerator OnRoundEnd(IGameModeHandler gm)
        {
            effectEnabled = false;
            effectApplied = false;

            yield break;
        }

        public void OnDisable()
        {
            bool isRespawning = player.data.healthHandler.isRespawning;
            // UnityEngine.Debug.Log($"[HOLLOW] from player [{player.playerID}] - is resurresting [{isRespawning}]");

            if (isRespawning)
            {
                // does nothing
                // UnityEngine.Debug.Log($"[HOLLOW] from player [{player.playerID}] - is resurresting!?");
            }
            else
            {
                wasDeactivated = true;
                effectEnabled = false;
                // UnityEngine.Debug.Log($"[HOLLOW] from player [{player.playerID}] - dead ded!?");
            }
        }

        public void OnDestroy()
        {
            GameModeManager.RemoveHook(GameModeHooks.HookPointStart, OnPointStart);
            GameModeManager.RemoveHook(GameModeHooks.HookRoundEnd, OnRoundEnd);
        }

        public void Refresh()
        {
            if (this.stats.GetGearData().sizeMod == GearUpConstants.ModType.sizeNormalize)
            {
                effectEnabled = true;
            }
            effectApplied = false;

            prevMaxHealth = player.data.maxHealth;
            prevSizeModifier = this.stats.sizeMultiplier;
            prevTotalScale = Mathf.Pow(prevMaxHealth / 100f * 1.2f, 0.2f) * prevSizeModifier;
        }
    }

    class SizeNormalizerStatus : ReversibleEffect
    {
        public override void OnAwake()
        {
            this.SetLivesToEffect(999);
        }

        public void SetSize(float size)
        {
            characterStatModifiersModifier.sizeMultiplier_mult = size;

            try
            {
                ApplyModifiers();
            }
            catch (Exception exception)
            {
                Miscs.LogWarn("[GearUp] SizeNormStatus: caught an exception!");
                Miscs.LogWarn(exception);
            }
        }
    }
}
