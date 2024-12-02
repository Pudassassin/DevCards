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
        public static List<GearUpPreRoundEffects> instanceList = new List<GearUpPreRoundEffects>();

        // private static GameObject empowerShotVFX = GearUpCards.VFXBundle.LoadAsset<GameObject>("VFX_EmpowerShot");
        // internal bool addShotVFX = false;

        // extra bonus granted by Glyph CAD Module, may differ from actual card bonus for balancing reason
        private const float glyphDivinationProjectileSpeed = 1.10f;
        private const float glyphDivinationProjectileSimSpeed = 1.10f;

        private const int glyphGeometricGunReflect = 3;

        private const float glyphMagickFragmentBlockCooldownAdd = -0.1f;
        private const float glyphMagickFragment_BlockCdMul = 0.75f;

        private const float glyphPotencyDamage = 1.35f;

        private const float glyphTimeGunDragMul = 0.80f;
        private const float glyphTimeGunLifetimeMul = 1.35f;
        
        private const int glyphReplicationProjectiles = 2;

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
        internal HollowLifeEffect hollowLifeEffect;

        // stats deltas -- for [Glyph CAD Module]
        public bool playerHas_GlyphCAD = false;

        public int statsDelta_GunNumProjectile_ADD = 0;
        public float statsDelta_GunDamage_MUL = 1.0f;

        public float statsDelta_GunProjSpeed_MUL = 1.0f;
        public float statsDelta_GunProjSim_MUL = 1.0f;
        public int statsDelta_GunReflect_ADD = 0;

        public float statsDelta_BlockCdAdd_ADD = 0.0f;
        public float statsDelta_BlockCdMul_ADD = 0.0f;
        public float statsDelta_BlockCdMul_MUL = 1.0f;

        public float statsDelta_GunDrag_MUL = 1.0f;
        public float statsDelta_GunLifetime_MUL = 1.0f;

        // stats deltas 2 -- for [Bullets.rar]
        public bool playerHas_BulletRAR = false;

        public int statsDelta_GunMaxAmmo_ADD_2 = 0;
        public float statsDelta_GunDamage_MUL_2 = 1.0f;
        public int statsDelta_GunNumProjectile_ADD_2 = 0;
        public float statsDelta_GunBurstTime_ADD_2 = 0.0f;

        // stats deltas omega -- live update stats


        // backup of character stats
        private int prevGunMaxAmmo = 3;
        private int prevGunNumProjectile = 1;
        private float prevGunBurstTime = 0.0f;
        private float prevGunDamage = 1.0f;

        public float prevGunProjSpeed = 1.0f;
        public float prevGunProjSim = 1.0f;
        public int prevGunReflect = 0;
        // public float prevGunDamageMul = 1.0f;

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

            // GameModeManager.AddHook(GameModeHooks.HookPickEnd, OnPickEnd);
            GameModeManager.AddHook(GameModeHooks.HookPickStart, OnPickStart);

            GameModeManager.AddHook(GameModeHooks.HookGameStart, OnRematch);

            instanceList.Add(this);
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
                    // %HP regen up to (current) max health
                    hollowLifeEffect = gameObject.GetComponent<HollowLifeEffect>();

                    if (hollowLifeEffect != null)
                    {
                        if (player.data.health < player.data.maxHealth * hollowLifeEffect.GetHealthCapMultiplier())
                        {
                            healthHandler.Heal(hpPercentageRegen * procTickTime * player.data.maxHealth);
                        }

                    }
                    else if (player.data.health < player.data.maxHealth)
                    {
                        healthHandler.Heal(hpPercentageRegen * procTickTime * player.data.maxHealth);
                    }

                    RefreshStatsLiveUpdate();

                    procTimer -= procTickTime;
                    // proc_count++;
                }
            }

        }

        public static void TriggerStatsMods()
        {
            foreach (var item in instanceList)
            {
                item.ApplyStatsMods();
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

            // prevGunDamageMul = gun.damage;
            prevGunProjSim = gun.projectielSimulatonSpeed;
            prevGunProjSpeed = gun.projectileSpeed;
            prevGunReflect = gun.reflects;

            prevGunMaxAmmo = gunAmmo.maxAmmo;
            prevGunNumProjectile = gun.numberOfProjectiles;
            prevGunBurstTime = gun.timeBetweenBullets;
            prevGunDamage = gun.damage;

            prevGunDrag = gun.drag;
            prevGunLifetime = gun.destroyBulletAfter;

            // prevGunProjCount = gun.numberOfProjectiles;
        }

        private void UndoStatsChange()
        {
            if (playerHas_BulletRAR)
            {
                gun.timeBetweenBullets -= statsDelta_GunBurstTime_ADD_2;
                gun.numberOfProjectiles -= statsDelta_GunNumProjectile_ADD_2;
                gun.damage /= statsDelta_GunDamage_MUL_2;
                gunAmmo.maxAmmo -= statsDelta_GunMaxAmmo_ADD_2;

                playerHas_BulletRAR = false;


                statsDelta_GunBurstTime_ADD_2 = 0.0f;
                statsDelta_GunNumProjectile_ADD_2 = 0;
                statsDelta_GunDamage_MUL_2 = 1.0f;
                statsDelta_GunMaxAmmo_ADD_2 = 0;
            }

            if (playerHas_GlyphCAD)
            {
                gun.numberOfProjectiles -= statsDelta_GunNumProjectile_ADD;

                gun.drag /= statsDelta_GunDrag_MUL;
                gun.destroyBulletAfter /= statsDelta_GunLifetime_MUL;

                gun.damage /= statsDelta_GunDamage_MUL;

                block.cdMultiplier /= statsDelta_BlockCdMul_MUL;
                block.cdMultiplier -= statsDelta_BlockCdMul_ADD;
                block.cdAdd -= statsDelta_BlockCdAdd_ADD;

                gun.reflects -= statsDelta_GunReflect_ADD;

                gun.projectielSimulatonSpeed /= statsDelta_GunProjSim_MUL;

                gun.projectileSpeed /= statsDelta_GunProjSpeed_MUL;

                playerHas_GlyphCAD = false;


                statsDelta_GunNumProjectile_ADD = 0;
                statsDelta_GunDamage_MUL = 1.0f;

                statsDelta_GunProjSpeed_MUL = 1.0f;
                statsDelta_GunProjSim_MUL = 1.0f;
                statsDelta_GunReflect_ADD = 0;

                statsDelta_BlockCdAdd_ADD = 0.0f;
                statsDelta_BlockCdMul_ADD = 0.0f;
                statsDelta_BlockCdMul_MUL = 1.0f;

                statsDelta_GunDrag_MUL = 1.0f;
                statsDelta_GunLifetime_MUL = 1.0f;
            }

            Miscs.Log("[GearUp] UndoStatsChange() applied");
        }

        // private void RestorePlayerStats()
        // {
        //     block.cdAdd = prevBlockCdAdd;
        //     block.cdMultiplier = prevBlockCdMul;
        // 
        //     // gun.damage = prevGunDamageMul;
        //     gun.projectielSimulatonSpeed = prevGunProjSim;
        //     gun.projectileSpeed = prevGunProjSpeed;
        //     gun.reflects = prevGunReflect;
        // 
        //     gunAmmo.maxAmmo = prevGunMaxAmmo;
        //     gun.numberOfProjectiles = prevGunNumProjectile;
        //     gun.timeBetweenBullets = prevGunBurstTime;
        //     gun.damage = prevGunDamage;
        // 
        //     gun.drag = prevGunDrag;
        //     gun.destroyBulletAfter = prevGunLifetime;
        // 
        //     // gun.numberOfProjectiles = prevGunProjCount;
        // }

        internal void ApplyGlyphCADModuleEffect()
        {
            if (playerHas_GlyphCAD)
            {
                Miscs.LogWarn("[GearUp] ApplyGlyphCADModuleEffect() is called more than once!");
                return;
            }
            playerHas_GlyphCAD = true;

            int glyphDivination = stats.GetGearData().glyphDivination;
            int glyphGeometric = stats.GetGearData().glyphGeometric;
            // int glyphInfluence      = this.stats.GetGearData().glyphInfluence;
            int magickFragment = stats.GetGearData().glyphMagickFragment;
            int glpyhPotency = stats.GetGearData().glyphPotency;
            int glyphTime = stats.GetGearData().glyphTime;
            int glyphReplication = stats.GetGearData().glyphReplication;

            // modify and save delta's

            // Divination Glyph
            statsDelta_GunProjSpeed_MUL = Mathf.Pow(glyphDivinationProjectileSpeed, glyphDivination);
            gun.projectileSpeed *= statsDelta_GunProjSpeed_MUL;

            statsDelta_GunProjSim_MUL = Mathf.Pow(glyphDivinationProjectileSimSpeed, glyphDivination);
            gun.projectielSimulatonSpeed *= statsDelta_GunProjSim_MUL;

            // Geometric Glyph
            statsDelta_GunReflect_ADD = glyphGeometricGunReflect * glyphGeometric;
            gun.reflects += statsDelta_GunReflect_ADD;

            // Magick Fragment
            statsDelta_BlockCdAdd_ADD = glyphMagickFragmentBlockCooldownAdd * magickFragment;
            block.cdAdd += statsDelta_BlockCdAdd_ADD;

            statsDelta_BlockCdMul_ADD = 0.0f;
            statsDelta_BlockCdMul_MUL = 1.0f;

            for (int i = 0; i < magickFragment; i++)
            {
                if (block.cdMultiplier >= 1.25f)
                {
                    statsDelta_BlockCdMul_ADD -= (1.0f - glyphMagickFragment_BlockCdMul);
                    block.cdMultiplier -= (1.0f - glyphMagickFragment_BlockCdMul);
                }
                else
                {
                    statsDelta_BlockCdMul_MUL *= glyphMagickFragment_BlockCdMul;
                    block.cdMultiplier *= glyphMagickFragment_BlockCdMul;
                }
            }

            // Potency Glyph
            statsDelta_GunDamage_MUL = Mathf.Pow(glyphPotencyDamage, glpyhPotency);
            gun.damage *= statsDelta_GunDamage_MUL;

            // Time Glyph
            statsDelta_GunLifetime_MUL = 1.0f;
            statsDelta_GunDrag_MUL = 1.0f;

            for (int i = 0; i < glyphTime; i++)
            {
                if (gun.destroyBulletAfter > 0.0f)
                {
                    statsDelta_GunLifetime_MUL *= glyphTimeGunLifetimeMul;
                    gun.destroyBulletAfter *= glyphTimeGunLifetimeMul;
                }
                if (gun.drag > 0.0f)
                {
                    statsDelta_GunDrag_MUL *= glyphTimeGunDragMul;
                    gun.drag *= glyphTimeGunDragMul;
                }
            }

            // Replication Glyph
            statsDelta_GunNumProjectile_ADD = glyphReplication * glyphReplicationProjectiles;
            gun.numberOfProjectiles += statsDelta_GunNumProjectile_ADD;

            Miscs.Log("[GearUp] ApplyGlyphCADModuleEffect() applied");
        }

        private void ApplyBulletsDotRar()
        {
            if (playerHas_BulletRAR)
            {
                Miscs.LogWarn("[GearUp] ApplyBulletsDotRar() is called more than once!");
                return;
            }
            playerHas_BulletRAR = true;

            int oldnumProjectile = gun.numberOfProjectiles;
            int newNumProjectile = Mathf.RoundToInt(Mathf.Clamp((float)prevGunNumProjectile / 3.0f, 1.0f, 100.0f));
            if (newNumProjectile == oldnumProjectile) return;

            float damageScale = (float)prevGunMaxAmmo / (float)newNumProjectile * 1.7f;

            // Clamp Gun clip size
            int newMaxAmmo = Mathf.RoundToInt(Mathf.Clamp((float)prevGunMaxAmmo / 2.0f, 1.0f, (float)int.MaxValue / 2.0f));
            statsDelta_GunMaxAmmo_ADD_2 = newMaxAmmo - gunAmmo.maxAmmo;
            gunAmmo.maxAmmo = newMaxAmmo;

            // Scale Gun damage
            statsDelta_GunDamage_MUL_2 = damageScale;
            gun.damage *= damageScale;

            // Set new num of projectile
            statsDelta_GunNumProjectile_ADD_2 = newNumProjectile - oldnumProjectile;
            gun.numberOfProjectiles = newNumProjectile;

            // Set new burst cooldown 
            if (prevGunBurstTime > 0.0f)
            {
                float newGunBurstTime = 0.075f + (prevGunBurstTime / 3.0f);
                statsDelta_GunBurstTime_ADD_2 = newGunBurstTime - gun.timeBetweenBullets;
                gun.timeBetweenBullets = newGunBurstTime;
            }

            Miscs.Log("[GearUp] ApplyBulletsDotRar() applied");
        }

        private void ApplyStatsMods()
        {
            if (this.stats.GetGearData().addOnList.Contains(GearUpConstants.AddOnType.cadModuleGlyph))
            {
                ApplyGlyphCADModuleEffect();
            }

            if (this.stats.GetGearData().addOnList.Contains(GearUpConstants.AddOnType.gunBulletsDotRar))
            {
                ApplyBulletsDotRar();
            }
        }

        // Event methods
        private IEnumerator OnPickEnd(IGameModeHandler gm)
        {
            SavePlayerStats();

            ApplyStatsMods();

            yield break;
        }

        private IEnumerator OnPickStart(IGameModeHandler gm)
        {
            // RestorePlayerStats();
            UndoStatsChange();

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

            // GameModeManager.RemoveHook(GameModeHooks.HookPickEnd, OnPickEnd);
            GameModeManager.RemoveHook(GameModeHooks.HookPickStart, OnPickStart);

            GameModeManager.RemoveHook(GameModeHooks.HookGameStart, OnRematch);
        }
    }
}