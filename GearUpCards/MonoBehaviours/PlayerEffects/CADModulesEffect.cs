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
    internal class CADModulesEffect : MonoBehaviour
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

        // internals
        private const float procTime = .10f;

        internal float timer = 0.0f;
        internal bool effectWarmUp = false;
        // internal int proc_count = 0;

        internal Player player;
        internal Gun gun;
        internal GunAmmo gunAmmo;
        internal Block block;
        internal CharacterStatModifiers stats;

        // snapshots
        public float prevGunProjSpeed = 1.0f;
        public float prevGunProjSim = 1.0f;
        public int prevGunReflect = 0;
        public float prevGunDamageMul = 1.0f;

        public float prevBlockCdAdd = 0.0f;
        public float prevBlockCdMul = 1.0f;


        public void Awake()
        {
            this.player = this.gameObject.GetComponent<Player>();
            this.gun = this.gameObject.GetComponent<WeaponHandler>().gun;
            this.gunAmmo = this.gun.GetComponentInChildren<GunAmmo>();
            this.block = this.gameObject.GetComponent<Block>();
            this.stats = this.gameObject.GetComponent<CharacterStatModifiers>();

            GameModeManager.AddHook(GameModeHooks.HookPointStart, OnPointStart);
            // GameModeManager.AddHook(GameModeHooks.HookPickEnd, OnPickEnd);
            GameModeManager.AddHook(GameModeHooks.HookPointEnd, OnPointEnd);
        }

        public void Start()
        {

        }

        public void Update()
        {
            // timer += TimeHandler.deltaTime;
            // 
            // if (timer >= procTime)
            // {
            //     RecalculateEffectStats();
            // 
            //     timer -= procTime;
            //     // proc_count++;
            // }
            // 
        }

        private void SavePlayerStats()
        {
            prevBlockCdAdd = this.block.cdAdd;
            prevBlockCdMul = this.block.cdMultiplier;

            prevGunDamageMul = this.gun.damage;
            prevGunProjSim = this.gun.projectielSimulatonSpeed;
            prevGunProjSpeed = this.gun.projectileSpeed;
            prevGunReflect = this.gun.reflects;
        }

        private void RestorePlayerStats()
        {
            this.block.cdAdd = prevBlockCdAdd;
            this.block.cdMultiplier = prevBlockCdMul;

            this.gun.damage = prevGunDamageMul;
            this.gun.projectielSimulatonSpeed = prevGunProjSim;
            this.gun.projectileSpeed = prevGunProjSpeed;
            this.gun.reflects = prevGunReflect;
        }

        internal void ApplyGlyphCADModuleEffect()
        {
            int glyphDivination     = this.stats.GetGearData().glyphDivination;
            int glyphGeometric      = this.stats.GetGearData().glyphGeometric;
            // int glyphInfluence      = this.stats.GetGearData().glyphInfluence;
            int magickFragment      = this.stats.GetGearData().magickFragmentStack;
            int glpyhPotency        = this.stats.GetGearData().glyphPotency;

            this.gun.projectileSpeed            *= Mathf.Pow(glyphDivinationProjectileSpeed, glyphDivination);
            this.gun.projectielSimulatonSpeed   *= Mathf.Pow(glyphDivinationProjectileSimSpeed, glyphDivination);

            this.gun.reflects += glyphGeometricGunReflect * glyphGeometric;

            this.block.cdAdd += glyphMagickFragmentBlockCooldownAdd * magickFragment;
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

            this.gun.damage *= Mathf.Pow(glyphPotencyDamage, glpyhPotency);
        }

        // Event methods
        private IEnumerator OnPointStart(IGameModeHandler gm)
        {
            SavePlayerStats();
            if (this.stats.GetGearData().addOnList.Contains(GearUpConstants.AddOnType.cadModuleGlyph))
            {
                ApplyGlyphCADModuleEffect();
            }

            yield break;
        }

        // private IEnumerator OnPickEnd(IGameModeHandler gm)
        // {
        // 
        //     yield break;
        // }

        private IEnumerator OnPointEnd(IGameModeHandler gm)
        {
            RestorePlayerStats();
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
            // GameModeManager.RemoveHook(GameModeHooks.HookBattleStart, OnPickEnd);
            GameModeManager.RemoveHook(GameModeHooks.HookPointEnd, OnPointEnd);

        }
    }
}
