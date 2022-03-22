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
        private const float healthCapFactor = .75f;
        private const float healingFactor = .85f;

        private const float healthCullRate = .05f;
        private const float procTime = .10f;

        internal int stackCount = 0;
        internal float healthCapPercentage = 1.0f;
        internal float healingEffectPercentage = 1.0f;

        internal float previousHealth;
        internal float previousMaxHealth;

        internal float timer = 0.0f;
        internal bool effectWarmup = false;
        internal bool effectEnabled = true;

        internal float healingTotal;

        internal Player player;
        internal CharacterStatModifiers stats;

        /* DEBUG */
        // internal int proc_count = 0;


        public void Awake()
        {
            this.player = this.gameObject.GetComponent<Player>();
            this.stats = this.gameObject.GetComponent<CharacterStatModifiers>();

            previousHealth = this.player.data.health;
            previousMaxHealth = this.player.data.maxHealth;

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
            timer += TimeHandler.deltaTime;

            if (effectWarmup && stackCount > 0)
            {
                // resolving with [Pristine Perserverence] at point start
                player.data.health = player.data.maxHealth * .80f;
            }

            // Relaying heal multiplier to patches

            // if (effectEnabled && stackCount > 0)
            // {
            //     // catching all the healing gained
            //     float healthDelta = player.data.health - previousHealth;
            //     previousHealth = player.data.health;
            // 
            //     float flagPristineGain = (player.data.maxHealth) / previousMaxHealth;
            // 
            //     if (healthDelta > 0)
            //     {
            //         // allowing health gains via Max HP increases
            //         if (flagPristineGain >= 2.5f)
            //         {
            //             healthDelta = 0.0f;
            //         }
            //         healingTotal += healthDelta;
            //     }
            // 
            //     previousMaxHealth = player.data.maxHealth;
            // }

            if (timer > procTime)
            {
                // Check whether the player's health is above the certain caps, then adjust it accordingly
                // [?] EXC's [Second Wind] implement it differently and seem to either look for health removal or it tries its best to heal up to said health point
                // [!] HDC's [Holy Light] will accumulate damage charges on each ebb and flow of health now incurred by [Hollow Life]
                // [!!] This card is dis-synergistic with [Pristine Perserverance] and any issue with it is considered as edge cases
                stackCount = stats.GetGearData().hollowLifeStack;
                CalculateEffects();

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
                    // else if (healingTotal > 0.0f)
                    // {
                    //     player.data.health -= healingTotal * (1 - healingEffectPercentage);
                    // }
                    // 
                    // healingTotal = 0.0f;

                    // UnityEngine.Debug.Log($"[HOLLOW] culled player [{player.playerID}] HP");
                }

                timer -= procTime;
                // proc_count++;
            }
        }

        public void CalculateEffects()
        {
            this.healthCapPercentage = Mathf.Pow(healthCapFactor, stackCount);
            this.healingEffectPercentage = Mathf.Pow(healingFactor, stackCount);
        }

        public float GetHealMultiplier()
        {
            return healingEffectPercentage;
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

            previousHealth = this.player.data.health;
            stackCount = stats.GetGearData().hollowLifeStack;
            CalculateEffects();

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
            GameModeManager.RemoveHook(GameModeHooks.HookPointStart, OnPointStart);
            GameModeManager.RemoveHook(GameModeHooks.HookBattleStart, OnBattleStart);
            GameModeManager.RemoveHook(GameModeHooks.HookPointEnd, OnPointEnd);
        }
    }
}
