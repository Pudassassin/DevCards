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
        // public float _debugScanInitialScale = 5.0f;
        // public string _debugScanLayer = "Front";
        private static GameObject scanVFXPrefab = GearUpCards.VFXBundle.LoadAsset<GameObject>("VFX_GenericAOE");

        private const float scannerStatusAmpFactor = .20f;

        private const float scannerStatusBaseDuration = 6.0f;
        private const float scannerStatusStackDuration = 1.0f;

        private const float scannerBaseRange = 8.0f;
        private const float scannerStackRange = 2.0f;

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

            if (stackCount <= 0)
            {
                empowerCharged = false;
                scannerReady = false;
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
                    // empower do cheeky teleport, I can just grab player.transform.position

                    // ScanVFX part
                    GameObject scanVFX = Instantiate(scanVFXPrefab, this.player.transform.position + new Vector3(0.0f, 0.0f, 100.0f), Quaternion.identity);
                    scanVFX.transform.localScale = Vector3.one * scannerRange;
                    scanVFX.name = "ScanVFX_Copy";
                    scanVFX.GetComponent<Canvas>().sortingLayerName = "MostFront";
                    scanVFX.GetComponent<Canvas>().sortingOrder = 10000;

                    scanVFX.GetComponent<Animator>().speed = 2.0f;
                    scanVFX.AddComponent<RemoveAfterSeconds>().seconds = 0.50f;
                    scanVFX.GetComponentInChildren<SpriteRenderer>().color = new Color(.50f, 1.0f, 0.5f, 1.0f);

                    // check players in range and apply status monos
                    TacticalScannerStatus status;

                    foreach (Player target in PlayerManager.instance.players)
                    {
                        if ((target.transform.position - this.player.transform.position).magnitude > this.scannerRange)
                            continue;

                        status = target.gameObject.GetOrAddComponent<TacticalScannerStatus>();

                        if (target.teamID != this.player.teamID)
                        {
                            status.ApplyStatus(scannerStatusAmp, scannerStatusDuration, false);
                        }
                        else
                        {
                            status.ApplyStatus(scannerStatusAmp, scannerStatusDuration, true);
                        }
                    }

                    // status = this.player.gameObject.GetOrAddComponent<TacticalScannerStatus>();
                    // status.ApplyStatus(scannerStatusAmp, scannerStatusDuration, true);
                    // status.ApplyStatus(scannerStatusAmp, 999999.9f, true);

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

            GameObject gameObject = GameObject.Find("ScanDataUICopy");
            while (gameObject != null)
            {
                Destroy(gameObject);
                gameObject = GameObject.Find("ScanDataUICopy");
            }

            yield break;
        }

        public void OnDisable()
        {

        }

        public void OnDestroy()
        {
            // This effect should persist between rounds, and at 0 stack it should do nothing mechanically
            // UnityEngine.Debug.Log($"Destroying Scanner  [{this.player.playerID}]");

            GameModeManager.RemoveHook(GameModeHooks.HookPointStart, OnPointStart);
            GameModeManager.RemoveHook(GameModeHooks.HookBattleStart, OnBattleStart);
            GameModeManager.RemoveHook(GameModeHooks.HookPointEnd, OnPointEnd);

            this.block.BlockAction = (Action<BlockTrigger.BlockTriggerType>)Delegate.Remove(this.block.BlockAction, this.scannerAction);
            // UnityEngine.Debug.Log($"Scanner destroyed  [{this.player.playerID}]");
        }
    }
}
