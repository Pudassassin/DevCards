using System.Collections.Generic;
using UnboundLib;
using UnityEngine;
using System.Linq;
using UnboundLib.GameModes;
using Photon.Pun;
using System.Reflection;
using ModdingUtils.MonoBehaviours;

using GearUpCards.Extensions;
using System.Collections;

namespace GearUpCards.MonoBehaviours
{
    internal class HollowLifeEffect : MonoBehaviour
    {
        private const float healthCapFactor = .70f;

        private const float healthCullRate = .05f;
        private const float procTime = .10f;

        internal int stackCount = 0;
        internal float healthCapPercentage = 1.0f;
        internal float timer = 0.0f;
        internal bool effectWarmup = true;
        internal bool effectEnabled = false;

        internal Player player;
        internal CharacterStatModifiers stats;

        /* DEBUG */
        // internal int proc_count = 0;


        public void Awake()
        {
            this.player = this.gameObject.GetComponent<Player>();
            this.stats = this.gameObject.GetComponent<CharacterStatModifiers>();

            GameModeManager.AddHook(GameModeHooks.HookPointStart, OnPointStart);
            GameModeManager.AddHook(GameModeHooks.HookBattleStart, OnBattleStart);
            GameModeManager.AddHook(GameModeHooks.HookPointEnd, OnPointEnd);
        }

        public void Start()
        {
        }

        public void FixedUpdate()
        {
            // Attempt to make hard HP Capped reliable and at foremost of the updates
            if (effectEnabled && player.data.HealthPercentage > .80f && stackCount > 0)
            {
                // hard cap to prevent [Pristine Perserverence] to become active and mess things up
                player.data.health = player.data.maxHealth * .80f;
            }
        }

        public void Update()
        {
            timer += Time.deltaTime;

            if (timer > procTime)
            {
                // Check whether the player's health is above the certain caps, then adjust it accordingly
                // [?] EXC's [Second Wind] implement it differently and seem to either look for health removal or it tries its best to heal up to said health point
                // [!!] HDC's [Holy Light] directly compare current health with previous health each Update() calls and 'charge' on positive gains
                //      leading to it having massive charge at round starts, and will accumulate on each of ebb and flow of health now incurred by [Hollow Life]
                // [!!] This card is dis-synergistic with [Pristine Perserverence] and any issue with it is considered as edge cases
                stackCount = stats.GetGearData().hollowLifeStack;
                RecalculateHealthCap();

                // if (proc_count >= 10)
                // {
                //     UnityEngine.Debug.Log($"[HOLLOW] running on player [{player.playerID}] with [{stackCount}] stacks");
                //     proc_count = 0;
                // }

                if (effectEnabled && stackCount > 0)
                {
                    if (player.data.HealthPercentage > healthCapPercentage)
                    {
                        float healthCullPercentage = Mathf.Clamp(player.data.HealthPercentage - healthCapPercentage, 0.0f, healthCullRate * stackCount);
                        player.data.health -= player.data.maxHealth * healthCullPercentage;
                    }

                    // UnityEngine.Debug.Log($"[HOLLOW] culled player [{player.playerID}] HP");
                }

                timer -= procTime;
                // proc_count++;
            }

            if (effectWarmup && stackCount > 0)
            {
                // resolving with [Pristine Perserverence] at point start
                player.data.health = player.data.maxHealth * .80f;
            }
        }

        private IEnumerator OnPointStart(IGameModeHandler gm)
        {
            effectWarmup = true;
            effectEnabled = false;

            yield break;
        }

        private IEnumerator OnBattleStart(IGameModeHandler gm)
        {
            effectWarmup = false;
            effectEnabled = true;

            stackCount = stats.GetGearData().hollowLifeStack;
            RecalculateHealthCap();

            // UnityEngine.Debug.Log($"[HOLLOW] from player [{player.playerID}] - Battle Start");

            yield break;
        }

        private IEnumerator OnPointEnd(IGameModeHandler gm)
        {
            effectEnabled = false;
        
            // UnityEngine.Debug.Log($"[HOLLOW] from player [{player.playerID}] - Point End");
        
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
            // This effect should persist between rounds, and at 0 stack it should do nothing mechanically

            GameModeManager.RemoveHook(GameModeHooks.HookPointStart, OnPointStart);
            GameModeManager.RemoveHook(GameModeHooks.HookBattleStart, OnBattleStart);
            GameModeManager.RemoveHook(GameModeHooks.HookPointEnd, OnPointEnd);
        }

        public void RecalculateHealthCap()
        {
            this.healthCapPercentage = Mathf.Pow(healthCapFactor, stackCount);
        }
    }
}
