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
        // private static GameObject empowerShotVFX = GearUpCards.VFXBundle.LoadAsset<GameObject>("VFX_EmpowerShot");
        // internal bool addShotVFX = false;

        // extra bonus granted by Glyph CAD Module, may differ from actual card bonus for balancing reason
        private const float glyphDivinationProjectileSpeed = 1.15f;
        private const float glyphDivinationProjectileSimSpeed = 1.10f;

        private const int glyphGeometricGunReflect = 3;

        private const float glyphMagickFragmentBlockCooldownAdd = -0.1f;
        private const float glyphMagickFragmentBlockCooldownMul = 0.75f;

        private const float glyphPotencyDamage = 1.50f;

        private const float glyphTimeGunDragMul = 0.80f;
        private const float glyphTimeGunLifetimeMul = 1.35f;

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

        public float prevGunProjSpeed = 1.0f;
        public float prevGunProjSim = 1.0f;
        public int prevGunReflect = 0;
        public float prevGunDamageMul = 1.0f;

        public float prevBlockCdAdd = 0.0f;
        public float prevBlockCdMul = 1.0f;

        public float prevGunDrag = 0.0f;
        public float prevGunLifetime = 0.0f;

        // additional chatacter stats
        private float hpPercentageRegen = 0.0f;

        public void Awake()
        {
            player = gameObject.GetComponent<Player>();
            gun = gameObject.GetComponent<WeaponHandler>().gun;
            gunAmmo = gun.GetComponentInChildren<GunAmmo>();
            block = gameObject.GetComponent<Block>();
            stats = gameObject.GetComponent<CharacterStatModifiers>();
            healthHandler = player.data.healthHandler;

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

        //public void RefreshStatsPreRound()
        //{
        //
        //}

        public void RefreshStatsLiveUpdate()
        {
            hpPercentageRegen = stats.GetGearData().hpPercentageRegen;
        }

        private void SavePlayerStats()
        {
            prevBlockCdAdd = block.cdAdd;
            prevBlockCdMul = block.cdMultiplier;

            prevGunDamageMul = gun.damage;
            prevGunProjSim = gun.projectielSimulatonSpeed;
            prevGunProjSpeed = gun.projectileSpeed;
            prevGunReflect = gun.reflects;

            prevGunMaxAmmo = gunAmmo.maxAmmo;
            prevGunNumProjectile = gun.numberOfProjectiles;
            prevGunBurstTime = gun.timeBetweenBullets;
            prevGunDamage = gun.damage;

            prevGunDrag = gun.drag;
            prevGunLifetime = gun.destroyBulletAfter;
        }

        private void RestorePlayerStats()
        {
            block.cdAdd = prevBlockCdAdd;
            block.cdMultiplier = prevBlockCdMul;

            gun.damage = prevGunDamageMul;
            gun.projectielSimulatonSpeed = prevGunProjSim;
            gun.projectileSpeed = prevGunProjSpeed;
            gun.reflects = prevGunReflect;

            gunAmmo.maxAmmo = prevGunMaxAmmo;
            gun.numberOfProjectiles = prevGunNumProjectile;
            gun.timeBetweenBullets = prevGunBurstTime;
            gun.damage = prevGunDamage;

            gun.drag = prevGunDrag;
            gun.destroyBulletAfter = prevGunLifetime;
        }

        internal void ApplyGlyphCADModuleEffect()
        {
            int glyphDivination = stats.GetGearData().glyphDivination;
            int glyphGeometric = stats.GetGearData().glyphGeometric;
            // int glyphInfluence      = this.stats.GetGearData().glyphInfluence;
            int magickFragment = stats.GetGearData().magickFragmentStack;
            int glpyhPotency = stats.GetGearData().glyphPotency;
            int glyphTime = stats.GetGearData().glyphTime;

            gun.projectileSpeed *= Mathf.Pow(glyphDivinationProjectileSpeed, glyphDivination);
            gun.projectielSimulatonSpeed *= Mathf.Pow(glyphDivinationProjectileSimSpeed, glyphDivination);

            gun.reflects += glyphGeometricGunReflect * glyphGeometric;

            block.cdAdd += glyphMagickFragmentBlockCooldownAdd * magickFragment;
            for (int i = 0; i < magickFragment; i++)
            {
                if (block.cdMultiplier >= 1.25f)
                {
                    block.cdMultiplier -= (1.0f - glyphMagickFragmentBlockCooldownMul);
                }
                else
                {
                    block.cdMultiplier *= glyphMagickFragmentBlockCooldownMul;
                }
            }

            gun.damage *= Mathf.Pow(glyphPotencyDamage, glpyhPotency);

            for (int i = 0; i < glyphTime; i++)
            {
                if (gun.destroyBulletAfter > 0.0f)
                {
                    gun.destroyBulletAfter *= glyphTimeGunLifetimeMul;
                }
                if (gun.drag > 0.0f)
                {
                    gun.drag *= glyphTimeGunDragMul;
                }
            }
        }

        private void ApplyBulletsDotRar()
        {
            int newNumProjectile = Mathf.RoundToInt(Mathf.Clamp((float)prevGunNumProjectile / 3.0f, 1.0f, 100.0f));
            if (newNumProjectile == prevGunNumProjectile) return;

            float damageScale = (float)prevGunMaxAmmo / (float)newNumProjectile * 1.7f;

            gunAmmo.maxAmmo = Mathf.RoundToInt(Mathf.Clamp((float)prevGunMaxAmmo / 2.0f, 1.0f, (float)int.MaxValue / 2.0f));
            gun.damage *= damageScale;
            gun.numberOfProjectiles = newNumProjectile;

            if (prevGunBurstTime > 0.0f)
            {
                gun.timeBetweenBullets = 0.075f + (prevGunBurstTime / 3.0f);
            }
        }

        // Event methods
        private IEnumerator OnPickEnd(IGameModeHandler gm)
        {
            SavePlayerStats();

            if (this.stats.GetGearData().addOnList.Contains(GearUpConstants.AddOnType.cadModuleGlyph))
            {
                ApplyGlyphCADModuleEffect();
            }

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