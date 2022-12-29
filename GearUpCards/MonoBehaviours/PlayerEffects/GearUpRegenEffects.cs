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
    internal class GearUpRegenEffects : MonoBehaviour
    {
        // internals
        private const float procTime = .10f;

        internal float timer = 0.0f;
        internal bool effectEnabled = false;
        // internal int proc_count = 0;

        internal Player player;
        internal Gun gun;
        internal GunAmmo gunAmmo;
        internal Block block;
        internal CharacterStatModifiers stats;
        internal HealthHandler healthHandler;

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
        }

        public void Start()
        {

        }

        public void Update()
        {
            timer += TimeHandler.deltaTime;
            
            if (timer >= procTime)
            {
                healthHandler.Heal(hpPercentageRegen * procTime * player.data.maxHealth);
                RefreshStats();

                timer -= procTime;
                // proc_count++;
            }
            
        }

        public void RefreshStats()
        {
            hpPercentageRegen = stats.GetGearData().hpPercentageRegen;
        }

        // Event methods
        private IEnumerator OnPointStart(IGameModeHandler gm)
        {
            effectEnabled = true;

            yield break;
        }

        // private IEnumerator OnPickEnd(IGameModeHandler gm)
        // {
        // 
        //     yield break;
        // }

        private IEnumerator OnPointEnd(IGameModeHandler gm)
        {
            effectEnabled = false;

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

        }
    }
}

