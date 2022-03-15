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
using GearUpCards.Utils;

using HarmonyLib;

namespace GearUpCards.MonoBehaviours
{
    internal class ShieldBatteryEffect : MonoBehaviour
    {
        private static string empowerPrefabName = "A_Empower";
        private static GameObject empowerPrefab = GameObject.Find(empowerPrefabName);
        // public float _debugScanInitialScale = 5.0f;
        // public string _debugScanLayer = "Front";

        private const float empowerBulletSpeedFactor = 1.5f;
        private const float empowerDamageFactor = 1.5f;
        private const float batteryDamageFactor = 1.25f;

        private const float procTime = .10f;

        internal Action<BlockTrigger.BlockTriggerType> blockAction;
        internal Action attackAction;

        private int batteryStackCount = 0;
        private int empowerStackCount = 0;

        private int empowerMaxAmmo = 0;
        private int empowerAmmo = 0;

        private List<Empower> empowerList = new List<Empower>();

        internal float timer = 0.0f;
        internal bool effectWarmUp = false;
        // internal int proc_count = 0;

        internal Player player;
        internal Gun gun;
        internal Block block;
        internal CharacterStatModifiers stats;

        public void Awake()
        {
            this.player = this.gameObject.GetComponent<Player>();
            this.gun = this.gameObject.GetComponent<WeaponHandler>().gun;
            this.block = this.gameObject.GetComponent<Block>();
            this.stats = this.gameObject.GetComponent<CharacterStatModifiers>();

            GameModeManager.AddHook(GameModeHooks.HookPointStart, OnPointStart);
            GameModeManager.AddHook(GameModeHooks.HookBattleStart, OnBattleStart);
            GameModeManager.AddHook(GameModeHooks.HookPointEnd, OnPointEnd);
        }

        public void Start()
        {
            // attach the new actions effect and passing along reference to owner player's stats
            this.blockAction = new Action<BlockTrigger.BlockTriggerType>(this.GetDoBlockAction(this.player, this.block).Invoke);
            this.block.BlockAction = (Action<BlockTrigger.BlockTriggerType>)Delegate.Combine(this.block.BlockAction, this.blockAction);

            this.attackAction = new Action(this.GetDoAttackAction(this.player, this.gun));
            this.gun.AddAttackAction(this.attackAction);
        }

        public void Update()
        {
            timer += TimeHandler.deltaTime;

            if (timer >= procTime)
            {
                RecalculateEffectStats();

                timer -= procTime;
                // proc_count++;
            }

        }

        private void CheckShieldBatteryEmpowerObject()
        {
            // give +1 instance of [Empower] on player while having [Shield Battery]
            string batteryEmpowerName = $"{empowerPrefabName}_GearUp({this.player.playerID})";

            GameObject batteryEmpower = this.player.transform.Find(batteryEmpowerName).gameObject;
            if (batteryEmpower == null)
            {
                batteryEmpower = Instantiate(empowerPrefab, this.player.transform);
                batteryEmpower.name = batteryEmpowerName;
            }
        }

        internal void RecalculateEffectStats()
        {
            batteryStackCount = stats.GetGearData().shieldBatteryStack;
            empowerStackCount = CardUtils.GetPlayerCardsWithName(this.player, "EMPOWER").Count;

            empowerMaxAmmo = batteryStackCount + empowerStackCount;
            empowerList = player.gameObject.GetComponentsInChildren<Empower>().ToList();

            float bulletSpeedMul;
            float damageMul;
            if (batteryStackCount > 0)
            {
                bulletSpeedMul = 1.0f;
                damageMul = batteryDamageFactor;
            }
            else
            {
                bulletSpeedMul = empowerBulletSpeedFactor;
                damageMul = empowerDamageFactor;
            }

            if (empowerStackCount > 0)
            {
                foreach (Empower item in empowerList)
                {
                    Traverse.Create(item).Field("dmgMultiplier").SetValue((float)damageMul);
                    Traverse.Create(item).Field("speedMultiplier").SetValue((float)bulletSpeedMul);
                }
            }
        }

        public float GetChargeCount()
        {
            return empowerAmmo;
        }

        public Action<BlockTrigger.BlockTriggerType> GetDoBlockAction(Player player, Block block)
        {
            return delegate (BlockTrigger.BlockTriggerType trigger)
            {
                bool conditionMet = (trigger == BlockTrigger.BlockTriggerType.Default);

                if (conditionMet && !effectWarmUp && batteryStackCount > 0)
                {
                    // Charge up the empower ammo
                    empowerAmmo = empowerMaxAmmo;
                }
            };
        }

        public Action GetDoAttackAction(Player player, Gun gun)
        {
            return delegate ()
            {
                // check empower ammo
                if (empowerAmmo > 0)
                {
                    // reset Empower state
                    try
                    {
                        foreach (Empower item in empowerList)
                        {
                            Traverse.Create(item).Field("empowered").SetValue((bool)true);
                        }
                    }
                    catch (Exception)
                    {
                        UnityEngine.Debug.LogError($"Shield Battery failed! [{player.playerID}]");
                    }

                    // deduce empower ammo
                    empowerAmmo -= 1;
                }
            };
        }

        private IEnumerator OnPointStart(IGameModeHandler gm)
        {
            effectWarmUp = true;

            empowerAmmo = 0;
            CheckShieldBatteryEmpowerObject();

            yield break;
        }

        private IEnumerator OnBattleStart(IGameModeHandler gm)
        {
            effectWarmUp = false;

            yield break;
        }

        private IEnumerator OnPointEnd(IGameModeHandler gm)
        {

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

            this.block.BlockAction = (Action<BlockTrigger.BlockTriggerType>)Delegate.Remove(this.block.BlockAction, this.blockAction);
            this.gun.InvokeMethod("RemoveAttackAction", (Action)attackAction);
            // UnityEngine.Debug.Log($"Scanner destroyed  [{this.player.playerID}]");
        }
    }
}
