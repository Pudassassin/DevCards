using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;

using UnityEngine;
using SoundImplementation;

using UnboundLib;
using UnboundLib.GameModes;
using Photon.Pun;
using ModdingUtils.MonoBehaviours;

using GearUpCards.Extensions;

namespace GearUpCards.MonoBehaviours
{
    internal class TacticalScannerEffect : MonoBehaviour
    {
        private const float scannerStatusAmpFactor = .20f;

        private const float scannerStatusBaseDuration = 7.0f;
        private const float scannerStatusStackDuration = 3.0f;

        private const float scannerBaseRange = 5.0f;
        private const float scannerStackRange = 1.0f;

        private const float scannerBaseCooldown = 10.0f;
        private const float scannerStackCooldown = -1.0f;

        private const float procTime = .10f;

        internal Action<BlockTrigger.BlockTriggerType> scannerAction;

        internal int stackCount = 0;
        internal float scannerStatusAmp;
        internal float scannerStatusDuration;
        internal float scannerRange;
        internal float scannerCooldown;

        internal bool scannerReady = false;
        internal bool empowerCharged = false;

        internal float timeLastUsed = 0.0f;

        internal float timer = 0.0f;
        internal bool effectWarmUp = false;
        // internal int proc_count = 0;

        internal Player player;
        internal Block block;
        internal CharacterStatModifiers stats;


        public void Awake()
        {
            this.player = this.gameObject.GetComponent<Player>();
            this.block = this.gameObject.GetComponent<Block>();
            this.stats = this.gameObject.GetComponent<CharacterStatModifiers>();

            GameModeManager.AddHook(GameModeHooks.HookPointStart, OnPointStart);
            GameModeManager.AddHook(GameModeHooks.HookBattleStart, OnBattleStart);
            GameModeManager.AddHook(GameModeHooks.HookPointEnd, OnPointEnd);
        }

        public void Start()
        {
            // attach the new block effect and passing along reference to owner player's stats
            this.scannerAction = new Action<BlockTrigger.BlockTriggerType>(this.GetDoBlockAction(this.player, this.block).Invoke);
            this.block.BlockAction = (Action<BlockTrigger.BlockTriggerType>)Delegate.Combine(this.block.BlockAction, this.scannerAction);
        }

        public void Update()
        {
            timer += Time.deltaTime;

            if (timer >= procTime)
            {
                stackCount = stats.GetGearData().tacticalScannerStack;
                RecalculateScannerStats();

                if (stackCount > 0)
                {
                    CheckReady();
                }

                timer -= procTime;
                // proc_count++;
            }

        }

        internal void RecalculateScannerStats()
        {
            scannerStatusAmp        = scannerStatusAmpFactor * stackCount;
            scannerStatusDuration   = scannerStatusBaseDuration + (scannerStatusStackDuration * stackCount);
            scannerRange            = scannerBaseRange + (scannerStackRange * stackCount);
            scannerCooldown         = scannerBaseCooldown + (scannerStackCooldown * stackCount);
        }

        internal void CheckReady()
        {
            if (scannerReady || effectWarmUp)
            {
                return;
            }
            else if (Time.time >= timeLastUsed + scannerCooldown)
            {
                scannerReady = true;
            }
        }

        // a work around the delegate limits, cheeky!
        public Action<BlockTrigger.BlockTriggerType> GetDoBlockAction(Player player, Block block)
        {
            return delegate (BlockTrigger.BlockTriggerType trigger)
            {
                bool conditionMet = (trigger == BlockTrigger.BlockTriggerType.Empower && empowerCharged) ||
                                    (trigger == BlockTrigger.BlockTriggerType.Default && scannerReady) ||
                                    (trigger == BlockTrigger.BlockTriggerType.ShieldCharge && scannerReady);

                if (conditionMet)
                {
                    // base.StartCorutine(this.ScannerFX());

                    Vector3 epicenter;

                    if (trigger == BlockTrigger.BlockTriggerType.Empower)
                    {
                        // epicenter is at impact point, gotta figure it out somehow
                        return;
                    }
                    else
                    {
                        epicenter = player.gameObject.transform.position;
                    }

                    // block effect
                    TacticalScannerStatus status;

                    // check enemies in range -- apply debuff mono
                    // check non-enemy in range -- apply buff mono
                    status = this.player.gameObject.GetOrAddComponent<TacticalScannerStatus>();
                    status.ApplyStatus(scannerStatusAmp, scannerStatusDuration, true);

                    if (trigger == BlockTrigger.BlockTriggerType.Empower)
                    {
                        empowerCharged = false;
                    }
                    else
                    {
                        timeLastUsed = Time.time;
                        scannerReady = false;
                        empowerCharged = true;
                    }
                }
            };
        }

        private List<Player> GetEnemyPlayer()
        {
            return (from target in PlayerManager.instance.players
                    where target.teamID != this.player.teamID
                    select target).ToList<Player>();
        }
        private List<Player> GetFriendlyPlayer()
        {
            return (from target in PlayerManager.instance.players
                    where target.teamID == this.player.teamID
                    select target).ToList<Player>();
        }

        private IEnumerator OnPointStart(IGameModeHandler gm)
        {
            effectWarmUp = true;
            yield break;
        }

        private IEnumerator OnBattleStart(IGameModeHandler gm)
        {
            effectWarmUp = false;
            scannerReady = true;

            stackCount = stats.GetGearData().tacticalScannerStack;

            yield break;
        }

        private IEnumerator OnPointEnd(IGameModeHandler gm)
        {
            scannerReady = false;
            empowerCharged = false;

            yield break;
        }

        public void OnDisable()
        {
            // bool isRespawning = player.data.healthHandler.isRespawning;
            // // UnityEngine.Debug.Log($"[HOLLOW] from player [{player.playerID}] - is resurresting [{isRespawning}]");
            // 
            // if (isRespawning)
            // {
            //     // does nothing
            //     // UnityEngine.Debug.Log($"[HOLLOW] from player [{player.playerID}] - is resurresting!?");
            // }
            // else
            // {
            //     effectEnabled = false;
            //     // UnityEngine.Debug.Log($"[HOLLOW] from player [{player.playerID}] - dead ded!?");
            // }
        }

        public void OnDestroy()
        {
            // This effect should persist between rounds, and at 0 stack it should do nothing mechanically

            GameModeManager.RemoveHook(GameModeHooks.HookPointStart, OnPointStart);
            GameModeManager.RemoveHook(GameModeHooks.HookBattleStart, OnBattleStart);
            GameModeManager.RemoveHook(GameModeHooks.HookPointEnd, OnPointEnd);

            this.block.BlockAction = (Action<BlockTrigger.BlockTriggerType>)Delegate.Remove(this.block.BlockAction, this.scannerAction);
        }
    }
}
