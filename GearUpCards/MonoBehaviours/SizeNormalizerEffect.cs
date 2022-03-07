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

        internal float totalScaleBefore;
        internal float timer = 0.0f;
        internal bool effectEnabled;
        internal bool effectApplied;

        internal Vector3 scaleVectorBefore;
        internal Vector3 scaleVectorLock;


        /* DEBUG */
        // internal int proc_count = 0;


        public void Awake()
        {
            this.player = this.gameObject.GetComponent<Player>();
            this.stats = this.gameObject.GetComponent<CharacterStatModifiers>();

            effectEnabled = true;
            effectApplied = false;

            totalScaleBefore = this.player.gameObject.transform.localScale.z;
            scaleVectorBefore = player.gameObject.transform.localScale;
            scaleVectorLock = Vector3.one * 1.2f; // default scale locking

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
                    float targetScale;

                    if (totalScaleBefore >= 1.2f)
                    {
                        targetScale = 1.65f - (0.5f * Mathf.Pow(0.9f, this.totalScaleBefore));
                    }
                    else
                    {
                        targetScale = 0.1f + Mathf.Pow(1.1f, this.totalScaleBefore);
                    }

                    // targetScale /= Mathf.Pow(this.player.data.maxHealth / 100f * 0.6f, 0.2f);

                    // compatibility with [W I D E], [Lanky] or other scaling mods
                    scaleVectorLock = player.gameObject.transform.localScale * targetScale / this.totalScaleBefore;

                    player.gameObject.transform.localScale = scaleVectorLock;
                    effectApplied = true;

                    UnityEngine.Debug.Log($"[SizeNorm] adjusting... [{player.playerID}] [{totalScaleBefore}] >> [{targetScale}]");
                }

                if (effectEnabled)
                {
                    player.gameObject.transform.localScale = scaleVectorLock;
                }
                
                if (!effectEnabled)
                {
                    this.stats.sizeMultiplier = totalScaleBefore;
                    effectApplied = false;
                }

                timer -= procTime;
                // proc_count++;
            }


        }

        private IEnumerator OnRoundStart(IGameModeHandler gm)
        {
            if (this.stats.GetGearData().sizeMod == GearUpConstants.ModType.sizeNormalize && effectEnabled)
            {
                Refresh();
            }

            yield break;
        }

        private IEnumerator OnRoundEnd(IGameModeHandler gm)
        {
            if (this.isActiveAndEnabled)
            {
                this.player.gameObject.transform.localScale = this.scaleVectorBefore;
            }

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
            this.player.gameObject.transform.localScale = this.scaleVectorBefore;

            effectEnabled = true;
            effectApplied = false;

            totalScaleBefore = this.player.gameObject.transform.localScale.z;
            scaleVectorBefore = player.gameObject.transform.localScale;
            scaleVectorLock = Vector3.one * 1.2f; // default scale locking
        }
    }
}
