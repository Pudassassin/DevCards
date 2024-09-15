using System.Linq;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using UnboundLib;
using UnboundLib.GameModes;
using Photon.Pun;
using ModdingUtils.MonoBehaviours;

using GearUpCards.Extensions;
using GearUpCards.Utils;

namespace GearUpCards.MonoBehaviours
{
    internal class HollowLifeEffect : MonoBehaviour
    {
        private const float hollowHpCapFactor = .70f;
        private const float hollowHealFactor = .75f;

        private const float hyperRegenHpCapFactor = 0.85f;

        // private const float healthCullRate = .05f;
        private const float procTime = .10f;

        internal int hollowLifeStack = 0;
        internal int hyperRegenStack = 0;

        internal float healthCapPercentage = 1.0f;
        internal float healingEffectPercentage = 1.0f;

        internal float tempHealthCapPercentage = 1.0f;
        internal bool tempHealthCapFlag = false;

        internal float previousHealth = -1.0f;
        internal float previousMaxHealth = -1.0f;

        internal float timer = 0.0f;
        internal bool effectWarmup = false;
        internal bool effectEnabled = true;
        internal bool wasDeactivated = false;

        internal Player player;
        internal CharacterStatModifiers stats;

        /* DEBUG */
        internal int proc_count = 0;


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
            if (effectEnabled && player.data.HealthPercentage > .80f && hollowLifeStack + hyperRegenStack > 0)
            {
                // hard cap to prevent [Pristine Perserverence] to become active and mess things up
                player.data.health = player.data.maxHealth * .85f;
                previousHealth = player.data.health;
            }
        }

        public void Update()
        {
            timer += TimeHandler.deltaTime;

            if (effectWarmup && hollowLifeStack + hyperRegenStack > 0)
            {
                // resolving with [Pristine Perserverence] at point start
                this.player.data.health = this.player.data.maxHealth * healthCapPercentage;
            }

            // if the script was active and was deactivated before (excluding reviving), presuming this as gamemode respawning cases
            if (!effectWarmup && wasDeactivated)
            {
                OnRespawn();
                wasDeactivated = false;
            }

            // handle Max HP changes and scalings
            if (!StatsMath.ApproxEqual(player.data.maxHealth, previousMaxHealth, 0.1f))
            {
                player.data.health = previousHealth * player.data.maxHealth / previousMaxHealth;
            }
            previousHealth = player.data.health;
            previousMaxHealth = player.data.maxHealth;

            if (timer > procTime)
            {
                // Check whether the player's health is above the certain caps, then adjust it accordingly
                // [?] EXC's [Second Wind] implement it differently and seem to either look for health removal or it tries its best to heal up to said health point
                // [!] HDC's [Holy Light] will accumulate damage charges on each ebb and flow of health now incurred by [Hollow Life]
                // [!!] This card is dis-synergistic with [Pristine Perserverance] and any issue with it is considered as edge cases
                CalculateEffects();

                // if (proc_count >= 10)
                // {
                //     // Miscs.Log($"[GearUp] HollowLife [{player.playerID}] HP Cap: [{stackCount}] x [{tempHealthCapPercentage}] = [{healthCapPercentage}]");
                // 
                //     proc_count = 0;
                // }

                if ((effectEnabled && hollowLifeStack + hyperRegenStack > 0 ) ||
                    tempHealthCapFlag)
                {
                    if (player.data.HealthPercentage > healthCapPercentage)
                    {
                        // float healthCullPercentage = Mathf.Clamp(player.data.HealthPercentage - healthCapPercentage, 0.0f, healthCullRate * (hollowLifeStack + 1));
                        // player.data.health -= player.data.maxHealth * healthCullPercentage;
                        player.data.health = player.data.maxHealth * healthCapPercentage;
                    }
                }

                timer -= procTime;
                // proc_count++;
            }
        }

        public void CalculateEffects()
        {
            hollowLifeStack = stats.GetGearData().hollowLifeStack;
            hyperRegenStack = stats.GetGearData().hyperRegenerationStack;

            float healthCapMul = 1.0f;
            healthCapMul *= Mathf.Pow(hollowHpCapFactor, hollowLifeStack);
            healthCapMul *= Mathf.Pow(hyperRegenHpCapFactor, hyperRegenStack);
            this.healthCapPercentage = healthCapMul;

            this.healingEffectPercentage = Mathf.Pow(hollowHealFactor, hollowLifeStack);

            if (tempHealthCapFlag)
            {
                this.healthCapPercentage *= this.tempHealthCapPercentage;
            }
        }

        public void ApplyTempHealthCap(float percentage)
        {
            tempHealthCapPercentage *= percentage;
            effectEnabled = true;
            tempHealthCapFlag = true;
        }

        public void ResetTempHealthCap()
        {
            tempHealthCapPercentage = 1.0f;
            tempHealthCapFlag = false;
        }

        public float GetHealthCapMultiplier()
        {
            return healthCapPercentage;
        }

        public float GetHealMultiplier()
        {
            // Let the patch handle heal multiplier
            return healingEffectPercentage;
        }

        private IEnumerator OnPointStart(IGameModeHandler gm)
        {
            wasDeactivated = false;

            ResetTempHealthCap();
            this.player.data.health = this.player.data.maxHealth;

            effectWarmup = true;
            effectEnabled = false;

            yield break;
        }

        private IEnumerator OnBattleStart(IGameModeHandler gm)
        {
            ResetTempHealthCap();

            previousHealth = this.player.data.health;
            previousMaxHealth = this.player.data.maxHealth;

            CalculateEffects();

            effectWarmup = false;
            if (hollowLifeStack + hyperRegenStack > 0)
            {
                effectEnabled = true;
            }

            this.player.data.health = this.player.data.maxHealth * healthCapPercentage;
            // UnityEngine.Debug.Log($"[HOLLOW] from player [{player.playerID}] - Battle Start");

            yield break;
        }

        private IEnumerator OnPointEnd(IGameModeHandler gm)
        {
            effectEnabled = false;
        
            // UnityEngine.Debug.Log($"[HOLLOW] from player [{player.playerID}] - Point End");
        
            yield break;
        }

        private void OnRespawn()
        {
            ResetTempHealthCap();

            previousHealth = this.player.data.health;
            previousMaxHealth = this.player.data.maxHealth;

            if (hollowLifeStack + hyperRegenStack > 0)
            {
                effectEnabled = true;
            }
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
                ResetTempHealthCap();
                effectEnabled = false;

                wasDeactivated = true;
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
