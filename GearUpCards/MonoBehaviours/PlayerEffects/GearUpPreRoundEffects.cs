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
    internal class GearUpPreRoundEffects : MonoBehaviour
    {
        // internals
        private const float procTickTime = .10f;

        internal float procTimer = 0.0f;
        internal bool effectEnabled = false;
        // internal int proc_count = 0;

        internal Player player;
        internal Gun gun;
        internal GunAmmo gunAmmo;
        internal Block block;
        internal CharacterStatModifiers stats;
        internal HealthHandler healthHandler;

        // backup of character stats
        private int prevGunMaxAmmo = 3;
        private int prevGunNumProjectile = 1;
        private float prevGunBurstTime = 0.0f;
        private float prevGunDamage = 1.0f;

        // additional chatacter stats
        private float hpPercentageRegen = 0.0f;

        public void Awake()
        {
            this.player = this.gameObject.GetComponent<Player>();
            this.gun = this.gameObject.GetComponent<WeaponHandler>().gun;
            this.gunAmmo = this.gun.GetComponentInChildren<GunAmmo>();
            this.block = this.gameObject.GetComponent<Block>();
            this.stats = this.gameObject.GetComponent<CharacterStatModifiers>();
            this.healthHandler = player.data.healthHandler;

            GameModeManager.AddHook(GameModeHooks.HookPointStart, OnPointStart);
            GameModeManager.AddHook(GameModeHooks.HookPointEnd, OnPointEnd);

            GameModeManager.AddHook(GameModeHooks.HookPickEnd, OnPickEnd);
            GameModeManager.AddHook(GameModeHooks.HookPickStart, OnPickStart);

            GameModeManager.AddHook(GameModeHooks.HookGameStart, OnRematch);
        }

        public void Start()
        {

        }

        public void Update()
        {
            if (effectEnabled)
            {
                procTimer += TimeHandler.deltaTime;

                if (procTimer >= procTickTime)
                {
                    healthHandler.Heal(hpPercentageRegen * procTickTime * player.data.maxHealth);
                    RefreshStatsLiveUpdate();

                    procTimer -= procTickTime;
                    // proc_count++;
                }
            }
            
        }

        public void RefreshStatsPreRound()
        {

        }

        public void RefreshStatsLiveUpdate()
        {
            hpPercentageRegen = stats.GetGearData().hpPercentageRegen;
        }

        private void SavePlayerStats()
        {
            prevGunMaxAmmo = gunAmmo.maxAmmo;
            prevGunNumProjectile = gun.numberOfProjectiles;
            prevGunBurstTime = gun.timeBetweenBullets;
            prevGunDamage = gun.damage;
        }

        private void RestorePlayerStats()
        {
            gunAmmo.maxAmmo = prevGunMaxAmmo;
            gun.numberOfProjectiles = prevGunNumProjectile;
            gun.timeBetweenBullets = prevGunBurstTime;
            gun.damage = prevGunDamage;
        }

        private void ApplyBulletsDotRar()
        {
            int newNumProjectile = Math.Clamp(Mathf.RoundToInt((float)(prevGunNumProjectile / 3.0f)), 1, 100);
            if (newNumProjectile == prevGunNumProjectile) return;

            float damageScale = (float)prevGunMaxAmmo / (float)newNumProjectile * 1.7f;

            gunAmmo.maxAmmo = Mathf.Clamp(Mathf.RoundToInt((float)prevGunMaxAmmo / 2.0f), 1, int.MaxValue / 2);
            gun.damage *= damageScale;
            gun.numberOfProjectiles = newNumProjectile;

            if (prevGunBurstTime > 0.0f)
            {
                gun.timeBetweenBullets = 0.075f + (prevGunBurstTime / 3.0f);
            }
        }

        private IEnumerator OnPickEnd(IGameModeHandler gm)
        {
            SavePlayerStats();

            if (this.stats.GetGearData().addOnList.Contains(GearUpConstants.AddOnType.gunBulletsDotRar))
            {
                ApplyBulletsDotRar();
            }

            yield break;
        }

        private IEnumerator OnPickStart(IGameModeHandler gm)
        {
            RestorePlayerStats();

            yield break;
        }

        // Event methods
        private IEnumerator OnPointStart(IGameModeHandler gm)
        {
            effectEnabled = true;
            procTimer = 0.0f;

            yield break;
        }

        private IEnumerator OnPointEnd(IGameModeHandler gm)
        {
            effectEnabled = false;

            yield break;
        }

        private IEnumerator OnRematch(IGameModeHandler gm)
        {
            Destroy(this);
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
            GameModeManager.RemoveHook(GameModeHooks.HookPointEnd, OnPointEnd);

            GameModeManager.RemoveHook(GameModeHooks.HookPickEnd, OnPickEnd);
            GameModeManager.RemoveHook(GameModeHooks.HookPickStart, OnPickStart);

            GameModeManager.RemoveHook(GameModeHooks.HookGameStart, OnRematch);
        }
    }
}

