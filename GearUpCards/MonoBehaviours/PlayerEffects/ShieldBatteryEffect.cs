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
using UnboundLib.Utils;

namespace GearUpCards.MonoBehaviours
{
    internal class ShieldBatteryEffect : MonoBehaviour
    {
        // private static GameObject empowerShotVFX = GearUpCards.VFXBundle.LoadAsset<GameObject>("VFX_EmpowerShot");
        // internal bool addShotVFX = false;

        private static string empowerPrefabName = "A_Empower";
        private static GameObject empowerPrefab = null;
        // public float _debugScanInitialScale = 5.0f;
        // public string _debugScanLayer = "Front";

        private const float empowerBulletSpeedFactor = 1.5f;
        private const float empowerDamageFactor = 1.5f;
        private const float batteryDamageFactor = 1.25f;

        private const float procTime = .10f;

        internal Action<BlockTrigger.BlockTriggerType> blockAction;
        internal Action attackAction;
        internal Action<GameObject> shootAction;

        private int batteryStackCount = 0;
        private int empowerStackCount = 0;

        private int empowerMaxAmmo = 0;
        private int empowerAmmo = 0;

        private List<Empower> empowerList = new List<Empower>();

        private bool burstMode = false;

        internal float timer = 0.0f;
        internal bool effectWarmUp = false;
        // internal int proc_count = 0;

        internal Player player;
        internal Gun gun;
        internal GunAmmo gunAmmo;
        internal Block block;
        internal CharacterStatModifiers stats;

        public void Awake()
        {
            if (empowerPrefab == null)
            {
                ReadEmpowerPrefab();
            }

            this.player = this.gameObject.GetComponent<Player>();
            this.gun = this.gameObject.GetComponent<WeaponHandler>().gun;
            this.gunAmmo = this.gun.GetComponentInChildren<GunAmmo>();
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

            // this.shootAction = new Action<GameObject>(this.GetDoShootAction(this.player, this.gun));
            // this.gun.ShootPojectileAction = (Action<GameObject>)Delegate.Combine(this.gun.ShootPojectileAction, this.shootAction);
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

        private static void ReadEmpowerPrefab()
        {
            CardInfo cardInfo = CardManager.cards.Values.First(card => card.cardInfo.cardName == "EMPOWER").cardInfo;
            CharacterStatModifiers stats = cardInfo.gameObject.GetComponent<CharacterStatModifiers>();
            empowerPrefab = stats.AddObjectToPlayer;
        }

        private void AddShieldBatteryEmpower()
        {
            // give a baseline instance of [Empower] on player while having [Shield Battery]
            string batteryEmpowerName = $"{empowerPrefabName}_GearUp({this.player.playerID})";

            // UnityEngine.Debug.Log(empowerPrefab);

            GameObject batteryEmpower;

            Transform transform = this.player.transform.Find(batteryEmpowerName);
            // UnityEngine.Debug.Log(transform);

            if (transform == null)
            {
                batteryEmpower = Instantiate(empowerPrefab, this.player.transform);
                batteryEmpower.name = batteryEmpowerName;

                // UnityEngine.Debug.Log(batteryEmpower);
            }
        }

        private void RemoveShieldBatteryEmpower()
        {
            // removing baseline instance of [Empower]
            string batteryEmpowerName = $"{empowerPrefabName}_GearUp({this.player.playerID})";

            Transform transform = this.player.transform.Find(batteryEmpowerName);

            if (transform != null)
            {
                Destroy(transform.gameObject);
            }
        }

        internal void RecalculateEffectStats()
        {
            batteryStackCount = stats.GetGearData().shieldBatteryStack;
            empowerStackCount = CardUtils.GetPlayerCardsWithName(this.player, "EMPOWER").Count;

            empowerMaxAmmo = (batteryStackCount * 2) + empowerStackCount;

            // baseline for owning at least one [Empower] card
            if (empowerStackCount > 0)
            {
                empowerMaxAmmo += 1;
            }

            empowerList = player.gameObject.GetComponentsInChildren<Empower>().ToList();

            // int bulletBatch;
            // if (this.gun.bursts > 1)
            // {
            //     bulletBatch = this.gun.bursts * this.gun.numberOfProjectiles;
            // }
            // else
            // {
            //     bulletBatch = this.gun.numberOfProjectiles;
            // }
            // 
            // float vollayPerClip = (float)this.gunAmmo.maxAmmo / (float)(bulletBatch);
            // vollayPerClip = Mathf.CeilToInt(vollayPerClip);
            // if (empowerMaxAmmo > vollayPerClip)
            // {
            //     burstMode = true;
            // }
            // else
            // {
            //     burstMode = false;
            // }

            float bulletSpeedMul;
            float damageMul;
            if (batteryStackCount > 0)
            {
                bulletSpeedMul = 1.0f;
                damageMul = 1.0f;
            }
            else
            {
                bulletSpeedMul = empowerBulletSpeedFactor;
                damageMul = empowerDamageFactor;
            }

            foreach (Empower item in empowerList)
            {
                Traverse.Create(item).Field("dmgMultiplier").SetValue((float)damageMul);
                Traverse.Create(item).Field("speedMultiplier").SetValue((float)bulletSpeedMul);
            }
        }

        public int GetChargeCount()
        {
            return empowerAmmo;
        }

        public Action<BlockTrigger.BlockTriggerType> GetDoBlockAction(Player player, Block block)
        {
            return delegate (BlockTrigger.BlockTriggerType trigger)
            {
                bool conditionMet = (trigger == BlockTrigger.BlockTriggerType.Default);

                if (conditionMet && batteryStackCount > 0)
                {
                    // Charge up the empower ammo
                    empowerAmmo = empowerMaxAmmo;
                }
            };
        }

        // Action Delegates
        // public Action<GameObject> ShootActionAddVFX()
        // {
        //     return delegate (GameObject bulletFired)
        //     {
        //         if (!addShotVFX) return;
        // 
        //         try
        //         {
        //             this.ExecuteAfterFrames(1, () =>
        //             {
        //                 GameObject VFX = Instantiate(empowerShotVFX, bulletFired.transform);
        //                 VFX.transform.up = bulletFired.transform.forward;
        //                 VFX.transform.localScale /= 2;
        //             });
        //         }
        //         catch (Exception)
        //         {
        //             UnityEngine.Debug.LogWarning($"[ShieldBatteryEffect] ShootActionAddVFX failed! [{player.playerID}]");
        //         }
        // 
        //         addShotVFX = false;
        //     };
        // }

        public Action GetDoAttackAction(Player player, Gun gun)
        {
            return delegate ()
            {
                // check empower ammo
                // if (empowerAmmo > 0 && !burstMode)
                if (empowerAmmo > 0)
                {
                    // reset Empower state
                    try
                    {
                        foreach (Empower item in empowerList)
                        {
                            Traverse.Create(item).Field("empowered").SetValue((bool)true);
                            Traverse.Create(item).Field("isOn").SetValue((bool)true);
                        }
                    }
                    catch (Exception)
                    {
                        UnityEngine.Debug.LogError($"[Shield Battery] AttackAction failed! [{player.playerID}]");
                    }

                    // deduce empower ammo
                    // addShotVFX = true;
                    empowerAmmo -= 1;
                }
            };
        }

        public Action<GameObject> GetDoShootAction(Player player, Gun gun)
        {
            return delegate (GameObject bulletFired)
            {
                // check empower ammo
                if (empowerAmmo > 1 && burstMode)
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
                        UnityEngine.Debug.LogError($"[Shield Battery] ShootAction failed! [{player.playerID}]");
                    }

                    // deduce empower ammo
                    // addShotVFX = true;
                    empowerAmmo -= 1;
                }
                else
                {
                    empowerAmmo = 0;
                }
            };
        }

        // Event methods
        private IEnumerator OnPointStart(IGameModeHandler gm)
        {
            effectWarmUp = true;

            empowerAmmo = 0;

            RecalculateEffectStats();

            if (batteryStackCount <= 0)
            {
                RemoveShieldBatteryEmpower();
            }
            else
            { 
                AddShieldBatteryEmpower();
            }

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
