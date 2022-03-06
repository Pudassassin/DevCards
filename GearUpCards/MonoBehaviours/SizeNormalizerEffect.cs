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
        private const float procTime = 1.0f;

        internal Player player;
        internal CharacterStatModifiers stats;

        internal float totalSizeUnmodded = -1.0f;
        internal float timer = 0.0f;
        internal bool effectEnabled;
        internal bool effectApplied;


        /* DEBUG */
        // internal int proc_count = 0;


        public void Awake()
        {
            this.player = this.gameObject.GetComponent<Player>();
            this.stats = this.gameObject.GetComponent<CharacterStatModifiers>();

            effectEnabled = true;
            effectApplied = false;

            GameModeManager.AddHook(GameModeHooks.HookRoundStart, OnRoundStart);
            GameModeManager.AddHook(GameModeHooks.HookRoundEnd, OnRoundEnd);
        }

        public void Start()
        {

        }

        public void Update()
        {
            timer += Time.deltaTime;

            if (timer > procTime)
            {
                if (effectEnabled && !effectApplied)
                {
                    if (this.totalSizeUnmodded < 0.0f)
                    {
                        this.totalSizeUnmodded = this.stats.sizeMultiplier;
                    }
                    if (this.stats.sizeMultiplier >= 1.25f)
                    {
                        this.stats.sizeMultiplier = 1.75f - (0.5f * Mathf.Pow(0.9f, this.totalSizeUnmodded));
                    }
                    else
                    {
                        this.stats.sizeMultiplier = 0.25f + Mathf.Pow(1.1f, this.totalSizeUnmodded);
                    }
                    effectApplied = true;
                    UnityEngine.Debug.Log($"[SizeNorm] adjusting... [{player.playerID}]");
                }
                
                if (!effectEnabled)
                {
                    this.stats.sizeMultiplier = totalSizeUnmodded;
                    effectApplied = false;
                }

                timer -= procTime;
                // proc_count++;
            }
        }

        private IEnumerator OnRoundStart(IGameModeHandler gm)
        {
            this.totalSizeUnmodded = this.stats.sizeMultiplier;

            if (this.stats.GetGearData().sizeMod == GearUpConstants.ModType.sizeNormalize)
            {
                effectEnabled = true;
            }

            yield break;
        }

        private IEnumerator OnRoundEnd(IGameModeHandler gm)
        {
            this.stats.sizeMultiplier = this.totalSizeUnmodded;

            effectEnabled = false;
        
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
                // UnityEngine.Debug.Log($"[HOLLOW] from player [{player.playerID}] - dead ded!?");
            }
        }
        public void OnDestroy()
        {
            GameModeManager.RemoveHook(GameModeHooks.HookPointStart, OnRoundStart);
            GameModeManager.RemoveHook(GameModeHooks.HookPointEnd, OnRoundEnd);
        }

        public void Refresh()
        {
            //return size to normal if it is ever be add mid-battle
            this.Awake();
        }
    }
}
