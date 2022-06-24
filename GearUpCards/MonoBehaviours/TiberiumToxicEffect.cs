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
    internal class TiberiumToxicEffect : MonoBehaviour
    {
        // private const float procTime = 0.1f;

        private class ToxicStack
        {
            internal const float lethalTriggerFactor = 0.01f;
            internal Player player;

            private float damagePerSecond;
            private float tickInterval;
            private int tickCount;
            private bool isLethal;

            internal float timer;
            internal bool isPermanent;

            public void SetPlayer(Player player)
            {
                this.player = player;
            }

            public ToxicStack(float damagePerSecond, float tickInterval, int tickCount, bool isLethal)
            {
                this.damagePerSecond = damagePerSecond;
                this.tickInterval = tickInterval;
                this.tickCount = tickCount;
                this.isLethal = isLethal;

                timer = 0.0f;
                isPermanent = false;
            }

            public void TickToxic()
            {
                timer += TimeHandler.deltaTime;

                if (timer >= tickInterval)
                {
                    if (CheckToxicActive() && damagePerSecond > 0.0f)
                    {
                        float tickDamage = damagePerSecond * tickInterval;

                        if (isLethal)
                        {
                            player.data.health -= tickDamage * (1.0f - lethalTriggerFactor);
                            player.data.healthHandler.DoDamage(new Vector2(tickDamage * lethalTriggerFactor, 0.0f), player.transform.position, new Color(0.0f, 0.85f, 0.0f, 1.0f));
                        }
                        else
                        {
                            player.data.health -= tickDamage;
                        }
                    }

                    timer -= tickInterval;
                    if (!isPermanent)
                    {
                        tickCount--;
                    }
                }
            }

            public void SetChronicToxic(float damageDelta, float newInterval)
            {
                isPermanent = true;
                damagePerSecond += damageDelta;
                tickInterval = newInterval;
            }

            public void SetLethality(bool value)
            {
                isLethal = value;
            }

            public bool CheckToxicActive()
            {
                return (tickCount > 0 || isPermanent);
            }
        }

        internal Player player;
        internal CharacterStatModifiers stats;

        private List<ToxicStack> toxicStacks;

        private int tiberiumBulletStack = 0;
        private float selfRegenPenalty = 0.0f;

        internal float timer = 0.0f;
        internal bool effectEnabled;
        internal bool effectApplied;
        internal bool wasDeactivated = false;

        public void Awake()
        {
            this.player = this.gameObject.GetComponent<Player>();
            this.stats = this.gameObject.GetComponent<CharacterStatModifiers>();

            this.toxicStacks = new List<ToxicStack>();
            effectEnabled = true;

            GameModeManager.AddHook(GameModeHooks.HookPointStart, OnPointStart);
            GameModeManager.AddHook(GameModeHooks.HookBattleStart, OnBattleStart);
            GameModeManager.AddHook(GameModeHooks.HookPointEnd, OnPointEnd);
        }

        public void Start()
        {

        }

        public void Update()
        {
            // Respawn case
            if (wasDeactivated)
            {
                OnRespawn();

                wasDeactivated = false;
                effectEnabled = true;
            }

            if (effectEnabled)
            {
                foreach (ToxicStack stack in toxicStacks)
                {
                    stack.TickToxic();
                }

                for (int i = toxicStacks.Count - 1; i >= 0; i--)
                {
                    if (!toxicStacks[i].CheckToxicActive())
                    {
                        toxicStacks.RemoveAt(i);
                    }
                }
            }
        }

        public void ApplyChronicStack(float damageDelta, float tickInterval)
        {
            if (toxicStacks.Count == 0)
            {
                toxicStacks.Add(new ToxicStack(0.0f, 1.0f, 1000, false));
                toxicStacks[0].SetPlayer(player);
            }

            toxicStacks[0].SetChronicToxic(damageDelta, tickInterval);
        }

        public void ApplyNewStack(float damagePerSecond, float tickInterval, int tickCount, bool isLethal)
        {
            if (toxicStacks.Count == 0)
            {
                // placeholder for potential Chronic stack zero
                toxicStacks.Add(new ToxicStack(0.0f, 1.0f, 1000, false));
                toxicStacks[0].SetPlayer(player);
                toxicStacks[0].SetChronicToxic(0.0f, 1.0f);
            }

            toxicStacks.Add(new ToxicStack(damagePerSecond, tickInterval, tickCount, isLethal));
            toxicStacks[toxicStacks.Count - 1].SetPlayer(player);
        }

        public void OnRespawn()
        {
            toxicStacks.Clear();

            // recalculate self-regen penalty from tiberium cards
            tiberiumBulletStack = stats.GetGearData().tiberiumBulletStack;
            selfRegenPenalty = 2.5f * tiberiumBulletStack;

            if (selfRegenPenalty > 0.0f)
            {
                ApplyChronicStack(selfRegenPenalty , 0.2f);
            }
        }

        public void OnRevive()
        {
            // remove all burst toxic, keep chronic one
            if (toxicStacks.Count > 1)
            {
                toxicStacks.RemoveRange(1, toxicStacks.Count - 1);
            }
        }

        private IEnumerator OnPointStart(IGameModeHandler gm)
        {
            wasDeactivated = false;
            effectEnabled = false;

            OnRespawn();

            yield break;
        }

        private IEnumerator OnBattleStart(IGameModeHandler gm)
        {
            effectEnabled = true;

            yield break;
        }

        private IEnumerator OnPointEnd(IGameModeHandler gm)
        {
            toxicStacks.Clear();

            effectEnabled = false;

            yield break;
        }

        public void OnDisable()
        {
            bool isRespawning = player.data.healthHandler.isRespawning;
            // UnityEngine.Debug.Log($"[HOLLOW] from player [{player.playerID}] - is resurresting [{isRespawning}]");

            if (isRespawning)
            {
                OnRevive();
                // UnityEngine.Debug.Log($"[HOLLOW] from player [{player.playerID}] - is resurresting!?");
            }
            else
            {
                toxicStacks.Clear();

                wasDeactivated = true;
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
