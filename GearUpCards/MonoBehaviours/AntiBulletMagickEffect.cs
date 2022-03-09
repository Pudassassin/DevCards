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
        private static GameObject spellVFXPrefab = GearUpCards.VFXBundle.LoadAsset<GameObject>("VFX_GenericAOE");

        private const string bulletGameObjectName = "Bullet_Base(Clone)";
        private const float spellCooldownBase = 15.0f;
        private const float spellForceReloadTimeAdd = 3.0f;
        private const float spellRange = 7.0f;

        private const float procTime = .10f;

        internal Action<BlockTrigger.BlockTriggerType> spellAction;

        internal int magickFragmentCount;
        internal float spellCooldown = 15.0f;
        internal bool spellReady = false;
        internal bool empowerCharged = false;

        internal Vector3 prevPosition;

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
            this.spellAction = new Action<BlockTrigger.BlockTriggerType>(this.GetDoBlockAction(this.player, this.block).Invoke);
            this.block.BlockAction = (Action<BlockTrigger.BlockTriggerType>)Delegate.Combine(this.block.BlockAction, this.spellAction);
        }

        public void Update()
        {
            timer += Time.deltaTime;

            if (timer >= procTime)
            {
                prevPosition = this.player.transform.position;
                if (this.stats.GetGearData().uniqueMagick == GearUpConstants.ModType.spellAntiBullet)
                {
                    RecalculateEffectStats();
                    CheckReady();
                }
                else
                {
                    empowerCharged = false;
                    spellReady = false;
                }

                timer -= procTime;
                // proc_count++;
            }
        }

        internal void CheckReady()
        {
            if (spellReady || effectWarmUp)
            {
                return;
            }
            else if (Time.time >= timeLastUsed + spellCooldown)
            {
                spellReady = true;
            }
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

                    VFX.GetComponentInChildren<Animator>().speed = 2.0f;
                    VFX.AddComponent<RemoveAfterSeconds>().seconds = 0.50f;
                    VFX.GetComponentInChildren<SpriteRenderer>().color = new Color(1.0f, 0.5f, 0.0f, 1.0f);

                    // check players in range and apply status monos
                    TacticalScannerStatus status;
                    float distance;

                    List<GameObject> bulletToDelete = GameObject.FindGameObjectsWithTag("Bullet").ToList();
                    foreach (GameObject bullet in bulletToDelete)
                    {
                        if (!bullet.name.Equals(bulletGameObjectName))
                            continue;

                        distance = (bullet.transform.position - player.transform.position).magnitude;

                        if (distance > spellRange)
                            continue;

                        Destroy(bullet);
                    }

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

                        UnityEngine.Debug.Log($"[AntiBullet] Reading player[{target.playerID}]");

                        Gun targetGun = target.gameObject.GetComponent<WeaponHandler>().gun;
                        GunAmmo targetGunAmmo = target.gameObject.GetComponent<WeaponHandler>().gun.GetComponentInChildren<GunAmmo>();

                        UnityEngine.Debug.Log($"[AntiBullet] Applying ForceReload to player[{target.playerID}]");
                        ApplyForceReload(targetGun, targetGunAmmo);
                        UnityEngine.Debug.Log($"[AntiBullet] Forced-Reload player[{target.playerID}]");

                    }

                    if (trigger == BlockTrigger.BlockTriggerType.Empower)
                    {
                        empowerCharged = false;
                    }
                    else
                    {
                        timeLastUsed = Time.time;
                        spellReady = false;
                        empowerCharged = true;
                    }
                }
            };
        }

        internal void RecalculateEffectStats()
        {
            magickFragmentCount = this.stats.GetGearData().magickFragmentStack;
            spellCooldown = spellCooldownBase - (magickFragmentCount * 1.5f);

            spellCooldown = Mathf.Clamp(spellCooldown, 7.0f, 30.0f);
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

    // public class AntiBulletMagickStatus : ReversibleEffect
    // {
    //     internal float timeApplied;
    //     internal float duration = 3.0f;
    // 
    //     public override void OnAwake()
    //     {
    //         timeApplied = Time.time;
    // 
    //         GunAmmo gunAmmo = this.gameObject.GetComponent<WeaponHandler>().gun.GetComponentInChildren<GunAmmo>();
    // 
    //         Traverse.Create(gunAmmo).Field("currentAmmo").SetValue((int) 0);
    //         Traverse.Create(gunAmmo).Field("freeReloadCounter").SetValue((float) 0.0f);
    //         Traverse.Create(gunAmmo).InvokeMethod("SetActiveBullets");
    //         
    //         float reloadTime = (float) 
    //         reloadCounter = ReloadTime();
    //         gun.isReloading = true;
    //         gun.player.data.stats.OnOutOfAmmp(maxAmmo);
    //     }
    // 
    //     public void ApplyStatus(float duration)
    //     {
    //         this.duration = duration;
    //     }
    // }
}
