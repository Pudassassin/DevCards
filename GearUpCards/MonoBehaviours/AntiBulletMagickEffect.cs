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
using HarmonyLib;

namespace GearUpCards.MonoBehaviours
{
    internal class AntiBulletMagickEffect : MonoBehaviour
    {
        // public float _debugScale = 2.0f;

        private static GameObject spellVFXPrefab = GearUpCards.VFXBundle.LoadAsset<GameObject>("VFX_AntiBulletMagick");

        private const string bulletGameObjectName = "Bullet_Base(Clone)";
        private const float spellCooldownBase = 15.0f;
        private const float spellForceReloadTimeAddBase = 3.5f;
        private const float spellRangeBase = 7.0f;
        private const float spellDurationBase = 1.0f;

        private const float procTime = .025f;
        private const float warmupTime = 2.0f;

        internal Action<BlockTrigger.BlockTriggerType> spellAction;

        // ===== Spell modifiers =====
        // cooldown reduction and cast/burst speed
        internal int magickFragment = 0;
        // AoE and range
        internal int glyptInfluence = 0;
        // Spell power
        internal int glyptPotency = 0;


        internal float spellCooldown = 15.0f;
        internal float spellRange = 7.0f;
        internal float spellForceReloadTimeAdd = 3.5f;
        internal float spellDuration = 1.0f;

        internal bool spellReady = false;
        internal bool empowerCharged = false;

        internal Vector3 prevPosition;
        internal Vector3 castPosition;

        internal float timeLastBlocked = 0.0f;
        internal float timeLastActivated = 0.0f;

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
            this.spellAction = new Action<BlockTrigger.BlockTriggerType>(this.GetDoBlockAction(this.player, this.block).Invoke);
            this.block.BlockAction = (Action<BlockTrigger.BlockTriggerType>)Delegate.Combine(this.block.BlockAction, this.spellAction);
        }

        public void Update()
        {
            timer += TimeHandler.deltaTime;

            if (timer >= procTime)
            {
                prevPosition = this.player.transform.position;
                if (this.stats.GetGearData().uniqueMagick == GearUpConstants.ModType.magickAntiBullet)
                {
                    RecalculateEffectStats();
                    CheckReady();
                }
                else
                {
                    empowerCharged = false;
                    spellReady = false;
                }

                if (Time.time < timeLastActivated + spellDuration)
                {
                    DeleteBullet();
                }

                timer -= procTime;
                // proc_count++;
            }

            this.stats.GetGearData().t_uniqueMagickCooldown = GetCooldown();
        }

        internal void CheckReady()
        {
            if (spellReady)
            {
                return;
            }
            else if (Time.time >= timeLastBlocked + spellCooldown)
            {
                spellReady = true;
            }
        }

        public float GetCooldown()
        {
            float cooldown = timeLastBlocked + spellCooldown - Time.time;
            if (spellReady) return -1.0f;
            else return cooldown;
        }

        // a work around the delegate limits, cheeky!
        public Action<BlockTrigger.BlockTriggerType> GetDoBlockAction(Player player, Block block)
        {
            return delegate (BlockTrigger.BlockTriggerType trigger)
            {
                bool conditionMet = (trigger == BlockTrigger.BlockTriggerType.Empower && empowerCharged) ||
                                    (trigger == BlockTrigger.BlockTriggerType.Default && spellReady) ||
                                    (trigger == BlockTrigger.BlockTriggerType.ShieldCharge && spellReady);

                if (conditionMet)
                {
                    // empower do cheeky teleport, I can just grab player.transform.position

                    // ScanVFX part
                    GameObject VFX = Instantiate(spellVFXPrefab, this.player.transform.position + new Vector3(0.0f, 0.0f, 100.0f), Quaternion.identity);
                    VFX.transform.localScale = Vector3.one * spellRange;
                    VFX.name = "AntiBulletVFX_Copy";
                    VFX.GetComponent<Canvas>().sortingLayerName = "MostFront";
                    VFX.GetComponent<Canvas>().sortingOrder = 10000;

                    VFX.AddComponent<RemoveAfterSeconds>().seconds = 1.25f;

                    // check players in range and apply status
                    float distance;

                    castPosition = this.player.transform.position;
                    DeleteBullet();

                    foreach (Player target in PlayerManager.instance.players)
                    {
                        if (target.playerID == this.player.playerID && trigger == BlockTrigger.BlockTriggerType.Empower)
                        {
                            distance = (this.prevPosition - this.player.transform.position).magnitude;
                        }
                        else
                        {
                            distance = (target.transform.position - this.player.transform.position).magnitude;
                        }

                        if (distance > spellRange)
                            continue;

                        // UnityEngine.Debug.Log($"[AntiBullet] Reading player[{target.playerID}]");

                        Gun targetGun = target.gameObject.GetComponent<WeaponHandler>().gun;
                        GunAmmo targetGunAmmo = target.gameObject.GetComponent<WeaponHandler>().gun.GetComponentInChildren<GunAmmo>();

                        // UnityEngine.Debug.Log($"[AntiBullet] Applying ForceReload to player[{target.playerID}]");
                        ApplyForceReload(targetGun, targetGunAmmo);

                        // UnityEngine.Debug.Log($"[AntiBullet] Forced-Reload player[{target.playerID}]");

                    }

                    if (trigger == BlockTrigger.BlockTriggerType.Empower)
                    {
                        empowerCharged = false;
                        timeLastActivated = Time.time;
                    }
                    else
                    {
                        timeLastBlocked = Time.time;
                        timeLastActivated = Time.time;
                        spellReady = false;
                        empowerCharged = true;
                    }
                }
            };
        }

        internal void RecalculateEffectStats()
        {
            magickFragment = this.stats.GetGearData().magickFragmentStack;
            glyptInfluence = this.stats.GetGearData().glyptInfluence;
            glyptPotency = this.stats.GetGearData().glyptPotency;

            spellCooldown = spellCooldownBase - (magickFragment * 1.5f);
            spellCooldown = Mathf.Clamp(spellCooldown, 7.0f, 30.0f);

            spellRange = spellRangeBase + (0.5f * glyptInfluence);

            spellForceReloadTimeAdd = spellForceReloadTimeAddBase + (0.5f * glyptPotency);
            spellDuration = spellDurationBase + (0.25f * glyptPotency);
        }

        internal void DeleteBullet()
        {
            float distance;
            List<GameObject> bulletToDelete = GameObject.FindGameObjectsWithTag("Bullet").ToList();
            foreach (GameObject bullet in bulletToDelete)
            {
                if (!bullet.name.Equals(bulletGameObjectName))
                    continue;

                distance = (bullet.transform.position - castPosition).magnitude;

                if (distance > spellRange)
                    continue;

                Destroy(bullet);
            }
        }

        internal void ApplyForceReload(Gun gun, GunAmmo gunAmmo)
        {
            Traverse.Create(gunAmmo).Field("currentAmmo").SetValue((int)0);
            Traverse.Create(gunAmmo).Field("freeReloadCounter").SetValue((float)0.0f);
            gunAmmo.InvokeMethod("SetActiveBullets", false);

            float reloadTime = (float)gunAmmo.InvokeMethod("ReloadTime") + spellForceReloadTimeAdd;
            Traverse.Create(gunAmmo).Field("reloadCounter").SetValue((float) reloadTime);
            gun.isReloading = true;
            int maxAmmo = gunAmmo.maxAmmo;
            gun.player.data.stats.InvokeMethod("OnOutOfAmmp", maxAmmo);
        }

        private IEnumerator OnPointStart(IGameModeHandler gm)
        {
            effectWarmUp = true;
            timeLastBlocked = Time.time - spellCooldown + warmupTime;
            yield break;
        }

        private IEnumerator OnBattleStart(IGameModeHandler gm)
        {
            effectWarmUp = false;
            spellReady = true;

            yield break;
        }

        private IEnumerator OnPointEnd(IGameModeHandler gm)
        {
            spellReady = false;
            empowerCharged = false;

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

            this.block.BlockAction = (Action<BlockTrigger.BlockTriggerType>)Delegate.Remove(this.block.BlockAction, this.spellAction);
            // UnityEngine.Debug.Log($"Scanner destroyed  [{this.player.playerID}]");
        }
    }
}
