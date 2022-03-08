using System.Collections.Generic;
using UnboundLib;
using UnityEngine;
using System.Linq;
using UnboundLib.GameModes;
using Photon.Pun;
using System.Reflection;
using ModdingUtils.MonoBehaviours;

using GearUpCards.Extensions;
using GearUpCards.Utils;

using System.Collections;

namespace GearUpCards.MonoBehaviours
{
    internal class SizeNormalizerEffect : MonoBehaviour
    {
        private const float procTime = 0.1f;

        internal Player player;
        internal CharacterStatModifiers stats;

        internal float prevSizeModifier;
        internal float prevMaxHealth;

        internal float totalScaleBefore;
        internal float totalScaleAfter;

        internal float timer = 0.0f;
        internal bool effectEnabled;
        internal bool effectApplied;

        // internal Vector3 scaleVectorBefore;
        // internal Vector3 scaleVectorLock;


        /* DEBUG */
        // internal int proc_count = 0;


        public void Awake()
        {
            this.player = this.gameObject.GetComponent<Player>();
            this.stats = this.gameObject.GetComponent<CharacterStatModifiers>();

            Refresh();

            // totalScaleBefore = this.player.gameObject.transform.localScale.z;
            // scaleVectorBefore = player.gameObject.transform.localScale;
            // scaleVectorLock = Vector3.one * 1.2f; // default scale locking

            // GameModeManager.AddHook(GameModeHooks.HookPickEnd, OnBattleStart);
            GameModeManager.AddHook(GameModeHooks.HookBattleStart, OnBattleStart);
            GameModeManager.AddHook(GameModeHooks.HookPointEnd, OnRoundEnd);
        }

        public void Start()
        {

        }

        // issue with [Pristine Perserverence] and/or [Brawler] when its resize overrules?
        public void Update()
        {
            timer += Time.deltaTime;

            if (timer > procTime)
            {
                if (effectEnabled)
                {
                    if (!StatsMath.ApproxEqual(this.player.data.maxHealth, prevMaxHealth))
                    {
                        effectApplied = false;
                        prevMaxHealth = this.player.data.maxHealth;
                        totalScaleBefore = Mathf.Pow(this.player.data.maxHealth / 100f * 1.2f, 0.2f) * prevSizeModifier;
                    }
                }

                if (effectEnabled && !effectApplied)
                {
                    float targetScale;

                    if (totalScaleBefore >= 1.0f)
                    {
                        targetScale = 1.45f - (0.5f * Mathf.Pow(0.9f, this.totalScaleBefore));
                    }
                    else
                    {
                        targetScale = -0.1f + Mathf.Pow(1.1f, this.totalScaleBefore);
                    }

                    totalScaleAfter = prevSizeModifier * targetScale / totalScaleBefore;

                    // compatibility with [W I D E], [Lanky] or other scaling mods
                    // scaleVectorLock = player.gameObject.transform.localScale * targetScale / this.totalScaleBefore;


                    //player.gameObject.transform.localScale = scaleVectorLock;

                    effectApplied = true;

                    //UnityEngine.Debug.Log($"[SizeNorm] adjusting... [{player.playerID}] [{totalScaleBefore}] >> [{targetScale}]");
                    UnityEngine.Debug.Log($"[SizeNorm] adjusting... [{player.playerID}] [{totalScaleBefore}] >> [{targetScale}] >> [{stats.sizeMultiplier}]");
                }

                if (effectEnabled && effectApplied)
                {
                    this.stats.sizeMultiplier = totalScaleAfter;

                    // player.gameObject.transform.localScale = scaleVectorLock;
                }
                
                if (!effectEnabled && !effectApplied)
                {
                    this.stats.sizeMultiplier = prevSizeModifier;
                    effectApplied = true;
                }

                timer -= procTime;
                // proc_count++;
            }


        }

        private IEnumerator OnBattleStart(IGameModeHandler gm)
        {
            Refresh();
            // if (this.stats.GetGearData().sizeMod == GearUpConstants.ModType.sizeNormalize && effectEnabled)
            // {
            //     Refresh();
            // }

            yield break;
        }

        private IEnumerator OnRoundEnd(IGameModeHandler gm)
        {
            // if (this.isActiveAndEnabled)
            // {
            //     // this.player.gameObject.transform.localScale = this.scaleVectorBefore;
            //     this.stats.sizeMultiplier = prevSizeModifier;
            // }
            // 
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
                effectEnabled = false;
                effectApplied = false;
                // UnityEngine.Debug.Log($"[HOLLOW] from player [{player.playerID}] - dead ded!?");
            }
        }
        public void OnDestroy()
        {
            GameModeManager.RemoveHook(GameModeHooks.HookBattleStart, OnBattleStart);
            GameModeManager.RemoveHook(GameModeHooks.HookRoundEnd, OnRoundEnd);
        }

        public void Refresh()
        {
            if (this.stats.GetGearData().sizeMod == GearUpConstants.ModType.sizeNormalize)
            {
                effectEnabled = true;
            }
            effectApplied = false;

            //return size to normal if it is ever be add mid-battle
            // this.player.gameObject.transform.localScale = this.scaleVectorBefore;

            prevMaxHealth = player.data.maxHealth;
            prevSizeModifier = this.stats.sizeMultiplier;
            totalScaleBefore = Mathf.Pow(this.player.data.maxHealth / 100f * 1.2f, 0.2f) * prevSizeModifier;

            // scaleVectorBefore = player.gameObject.transform.localScale;
            // scaleVectorLock = Vector3.one * 1.2f; // default scale locking
        }
    }
}
