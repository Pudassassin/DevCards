using System.Collections.Generic;
using System.Collections;
using System;
using System.Reflection;

using UnboundLib;
using UnityEngine;
using System.Linq;
using UnboundLib.GameModes;
using Photon.Pun;
using ModdingUtils.MonoBehaviours;
using HarmonyLib;

using GearUpCards.Extensions;
using GearUpCards.Utils;

namespace GearUpCards.MonoBehaviours
{
    internal class UniqueGunSpreadMono : MonoBehaviour
    {
        private const float procTime = 0.1f;

        internal Player player;
        internal Gun gun;
        internal CharacterStatModifiers stats;

        internal float prevSpread;
        internal float prevSpreadMul;
        internal float prevEvenSpread;
        internal float prevGravity;

        internal float timer = 0.0f;

        internal bool effectApplied = false;
        internal bool wasDeactivated = false;

        internal Action<BlockTrigger.BlockTriggerType> blockAction;
        internal Action attackAction;
        internal Action<GameObject> shootAction;

        private GameObject oldGunObject, newGunObject, dummyGunObject;
        private Gun playerOldGun, newSpreadGun;
        public Gun dummySpreadGun = null;
        internal bool isGunReplaced = false;
        internal GearUpConstants.ModType lastGunSpreadMod = GearUpConstants.ModType.none;

        public void Awake()
        {
            this.player = this.gameObject.GetComponent<Player>();
            this.gun = this.gameObject.GetComponent<WeaponHandler>().gun;
            this.stats = this.gameObject.GetComponent<CharacterStatModifiers>();

            oldGunObject = new GameObject("UniqueGunSpreadHolder_oldGun");
            oldGunObject.transform.parent = player.transform;
            oldGunObject.transform.localPosition = Vector3.zero;

            newGunObject = new GameObject("UniqueGunSpreadHolder_newGun");
            newGunObject.transform.parent = player.transform;
            newGunObject.transform.localPosition = Vector3.zero;

            dummyGunObject = new GameObject("UniqueGunSpreadHolder_dummyGun");
            dummyGunObject.transform.parent = player.transform;
            dummyGunObject.transform.localPosition = Vector3.zero;

            SetupDummyGun();

            GameModeManager.AddHook(GameModeHooks.HookPointStart, OnPointStart);
            GameModeManager.AddHook(GameModeHooks.HookRoundStart, OnRoundStart);
            GameModeManager.AddHook(GameModeHooks.HookPointEnd, OnPointEnd);
        }

        public void Start()
        {
            this.playerOldGun = new Gun();

            this.attackAction = new Action(this.GetDoAttackAction(this.player, this.gun));
            // this.gun.AddAttackAction(this.attackAction);

            this.shootAction = new Action<GameObject>(this.GetDoShootAction(this.player, this.gun));
            this.gun.ShootPojectileAction = (Action<GameObject>)Delegate.Combine(this.gun.ShootPojectileAction, this.shootAction);
        }

        public void Update()
        {
            // Respawn case
            if (wasDeactivated)
            {
                ApplyModifier();
            
                wasDeactivated = false;
                effectApplied = true;
            }

        }

        private IEnumerator OnRoundStart(IGameModeHandler gm)
        {
            prevSpread = gun.spread;
            prevEvenSpread = gun.evenSpread;
            prevSpreadMul = gun.multiplySpread;
            prevGravity = gun.gravity;

            SetupDummyGun();

            // if (stats.GetGearData().gunSpreadMod == GearUpConstants.ModType.gunSpreadFlak)
            // {
            //     Miscs.CopyGunStats(gun, playerOldGun);
            // }

            yield break;
        }

        private IEnumerator OnPointStart(IGameModeHandler gm)
        {
            ApplyModifier();

            wasDeactivated = false;
            effectApplied = true;

            yield break;
        }

        private IEnumerator OnPointEnd(IGameModeHandler gm)
        {
            RemoveModifier();

            effectApplied = false;
            wasDeactivated = false;

            yield break;
        }

        public void OnDisable()
        {
            bool isRespawning = player.data.healthHandler.isRespawning;
            // UnityEngine.Debug.Log($"[HOLLOW] from player [{player.playerID}] - is resurresting [{isRespawning}]");

            if (isRespawning)
            {
                // does nothing
                // UnityEngine.Debug.Log($"[HOLLOW] from player [{player.playerID}] - is resurresting!?");
            }
            else
            {
                RemoveModifier();

                wasDeactivated = true;
                effectApplied = false;
                // UnityEngine.Debug.Log($"[HOLLOW] from player [{player.playerID}] - dead ded!?");
            }
        }

        public void OnDestroy()
        {
            GameModeManager.RemoveHook(GameModeHooks.HookPointStart, OnPointStart);
            GameModeManager.RemoveHook(GameModeHooks.HookPointEnd, OnPointEnd);
            GameModeManager.RemoveHook(GameModeHooks.HookRoundStart, OnRoundStart);

            this.gun.ShootPojectileAction = (Action<GameObject>)Delegate.Remove(this.gun.ShootPojectileAction, this.shootAction);
        }

        public Action GetDoAttackAction(Player player, Gun gun)
        {
            // trigger on firing one burst of bullets...?
            return delegate ()
            {
                try
                {

                }
                catch (Exception exception)
                {
                    UnityEngine.Debug.LogError($"[UniqueGunSpreadMono] AttackAction failed! [{player.playerID}]");
                    UnityEngine.Debug.LogWarning(exception);
                }
            };
        }

        public Action<GameObject> GetDoShootAction(Player player, Gun gun)
        {
            // triggers on firing EACH bullet
            return delegate (GameObject bulletFired)
            {
                try
                {
                    switch (stats.GetGearData().gunSpreadMod)
                    {
                        case GearUpConstants.ModType.gunSpreadFlak:
                            bulletFired.GetOrAddComponent<FlakShellModifier>().Setup();
                            break;
                        default:
                            break;
                    }
                }
                catch (Exception exception)
                {
                    UnityEngine.Debug.LogError($"[UniqueGunSpreadMono] ShootAction failed! [{player.playerID}]");
                    UnityEngine.Debug.LogWarning(exception);
                }
            };
        }

        private void SetupDummyGun()
        {
            // full backup of player's original gun
            playerOldGun = oldGunObject.GetOrAddComponent<Gun>();
            Miscs.CopyGunStats(gun, playerOldGun);

            // prepare player's replacement gun
            newSpreadGun = newGunObject.GetOrAddComponent<Gun>();
            dummySpreadGun = dummyGunObject.GetOrAddComponent<Gun>();
        }

        private void ResetEffect()
        {
            if (ModdingUtils.Extensions.CharacterStatModifiersExtension.GetAdditionalData(player.data.stats).blacklistedCategories.Contains(CardUtils.GearCategory.typeUniqueGunSpread))
            {
                ModdingUtils.Extensions.CharacterStatModifiersExtension.GetAdditionalData(player.data.stats).blacklistedCategories.Remove(CardUtils.GearCategory.typeUniqueGunSpread);
            }

            if (isGunReplaced)
            {
                Miscs.CopyGunStats(playerOldGun, this.gun);
                isGunReplaced = false;
            }


        }

        private void RemoveModifier()
        {
            Miscs.Log("[GearUP] UniqueGunSpread: called remove");

            if (effectApplied)
            {
                if (isGunReplaced)
                {
                    Miscs.CopyGunStats(playerOldGun, gun);
                    isGunReplaced = false;
                }

                gun.spread = prevSpread;
                gun.evenSpread = prevEvenSpread;
                gun.multiplySpread = prevSpreadMul;
                gun.gravity = prevGravity;

                Miscs.Log("[GearUP] UniqueGunSpread: removed");
            }
        }

        private void ApplyModifier()
        {
            Miscs.Log("[GearUP] UniqueGunSpread: called apply");

            if (!effectApplied)
            {
                // Miscs.CopyGunStats(gun, playerOldGun);


                switch (stats.GetGearData().gunSpreadMod)
                {
                    case GearUpConstants.ModType.gunSpreadArc:
                        ApplySpreadArc();
                        break;
                    case GearUpConstants.ModType.gunSpreadLine:
                        break;
                    case GearUpConstants.ModType.gunSpreadParallel:
                        ApplySpreadParallel();
                        break;
                    case GearUpConstants.ModType.gunSpreadFlak:
                        ApplySpreadFlak();
                        break;
                    case GearUpConstants.ModType.none:
                        ResetEffect();
                        break;
                    default:
                        break;
                }

                Miscs.Log("[GearUP] UniqueGunSpread: applied");
            }
        }

        private void ApplySpreadArc()
        {
            if (prevSpread <= 0.0f)
            {
                gun.spread = 0.0f;
                gun.evenSpread = prevSpread;
                gun.multiplySpread = prevSpreadMul;
            }
            else
            {
                gun.spread = 0.01f;

                if (prevEvenSpread < 1.0f)
                {
                    gun.evenSpread = 1.0f;
                }
                else
                {
                    gun.evenSpread = prevEvenSpread;
                }

                gun.multiplySpread = prevSpreadMul * prevSpread * 135; // uas 150
            }
        }

        private void ApplySpreadParallel()
        {
            if (prevSpread <= 0.0f)
            {
                gun.spread = 0.0f;
                gun.evenSpread = prevSpread;
                gun.multiplySpread = prevSpreadMul;
            }
            else
            {
                gun.spread = 0.01f;

                if (prevEvenSpread < 1.0f)
                {
                    gun.evenSpread = 1.0f;
                }
                else
                {
                    gun.evenSpread = prevEvenSpread;
                }

                // gun.multiplySpread = prevSpreadMul * prevSpread * (50 + gun.numberOfProjectiles * 5);
                gun.multiplySpread = prevSpreadMul * prevSpread * (50 + gun.numberOfProjectiles * 3);
            }
        }

        private void ApplySpreadFlak()
        {
            // flak cannon stats
            Miscs.CopyGunStats(playerOldGun, newSpreadGun);

            newSpreadGun.bursts = Mathf.RoundToInt((float)playerOldGun.bursts / 4.0f);
            newSpreadGun.timeBetweenBullets = 0.25f;

            newSpreadGun.numberOfProjectiles = 1 + Mathf.RoundToInt((float)playerOldGun.numberOfProjectiles / 10.0f);

            newSpreadGun.damage = playerOldGun.damage * 2.5f;
            newSpreadGun.damageAfterDistanceMultiplier = 1.0f;
            newSpreadGun.dmgMOnBounce = 1.0f;
            newSpreadGun.percentageDamage = 0.0f;

            newSpreadGun.projectileSpeed = Mathf.Clamp(playerOldGun.projectileSpeed * 0.25f, 1.5f, 10.0f);
            newSpreadGun.projectielSimulatonSpeed = Mathf.Clamp(playerOldGun.projectielSimulatonSpeed, 0.25f, 10.0f);
            newSpreadGun.drag = 0.0f;
            newSpreadGun.dragMinSpeed = 1.0f;

            newSpreadGun.reflects = 99;

            newSpreadGun.attackSpeed = 0.20f + playerOldGun.attackSpeed;
            newSpreadGun.attackSpeedMultiplier = 0.5f + (playerOldGun.attackSpeedMultiplier - 0.5f) * 1.25f;

            newSpreadGun.knockback = playerOldGun.knockback * 1.5f;
            newSpreadGun.recoil = playerOldGun.recoil * 0.05f;

            // fragmentation stats (to be fired 4 times)
            Miscs.CopyGunStats(playerOldGun, dummySpreadGun);

            dummySpreadGun.bursts = Mathf.RoundToInt((float)playerOldGun.bursts / 4.0f);
            dummySpreadGun.timeBetweenBullets = 0.15f;

            dummySpreadGun.numberOfProjectiles = 8 + Mathf.RoundToInt((float)playerOldGun.numberOfProjectiles / 4.0f);

            dummySpreadGun.damage = playerOldGun.damage * 0.5f;

            dummySpreadGun.projectileSpeed = Mathf.Clamp(playerOldGun.projectileSpeed, 0.5f, 25.0f);
            dummySpreadGun.projectielSimulatonSpeed = Mathf.Clamp(playerOldGun.projectielSimulatonSpeed, 0.20f, 10.0f);

            dummySpreadGun.evenSpread = 0.0f;
            dummySpreadGun.spread = 1.0f;
            dummySpreadGun.multiplySpread = 1.0f;

            Action doNothing = () => { };
            Traverse.Create(dummySpreadGun).Field("attackAction").SetValue((Action)doNothing);
            dummySpreadGun.ShootPojectileAction = new Action<GameObject>((GameObject _) => { });

            Miscs.CopyGunStats(newSpreadGun, gun);
            isGunReplaced = true;
        }
    }

    class BulletNoClipModifier : MonoBehaviour
    {
        private const float tickTime = 0.1f;

        private float delayTime = 1.5f;
        private bool effectEnabled = false;
        private bool persistentOverride = false;

        internal bool previousState = true;
        internal float timer = 0.0f;

        public void Awake()
        {
            if (transform.parent != null)
            {
                previousState = transform.parent.Find("Collider").gameObject.activeSelf;
                transform.parent.Find("Collider").gameObject.SetActive(false);
                effectEnabled = true;
            }
        }

        public void Update()
        {
            if (effectEnabled)
            {
                timer += TimeHandler.deltaTime;

                if (!persistentOverride)
                {
                    if (timer >= delayTime)
                    {
                        RemoveModifier();
                    }
                }
                else
                {
                    // Persistent override the no bullet-bullet collision
                    if (timer >= tickTime)
                    {
                        if (transform.parent.Find("Collider").gameObject.activeSelf == false)
                        {
                            transform.parent.Find("Collider").gameObject.SetActive(false);
                        }

                        timer -= tickTime;
                    }
                }
            }
        }

        public void SetDuration(float duration)
        {
            delayTime = duration;
        }

        public void SetPersistentOverride(bool input)
        {
            persistentOverride = input;
        }

        public void RemoveModifier()
        {
            transform.parent.Find("Collider").gameObject.SetActive(previousState);
            Destroy(this);
        }

    }

    class ParallelBulletModifier : RayHitEffect
    {
        private float delayTime = 0.1f;
        private float initSpeed = 55.0f;

        internal float timer = 0.0f;

        private float prevGravity, prevSpeed, prevSimSpeed, prevDrag;
        private Vector3 targetDirection;

        private MoveTransform bulletMove;
        private ProjectileHit bulletHit;
        private Gun shooterGun;

        private bool effectEnabled = false;


        public void Start()
        {
            if (transform.parent != null)
            {
                try
                {
                    // Miscs.Log("[GearUp] ParallelBulletModifier: Update 0");
                    bulletMove = gameObject.GetComponentInParent<MoveTransform>();

                    // Miscs.Log("[GearUp] ParallelBulletModifier: Update 1");
                    bulletHit = gameObject.GetComponentInParent<ProjectileHit>();

                    // Miscs.Log("[GearUp] ParallelBulletModifier: Update 2");
                    shooterGun = bulletHit.ownWeapon.GetComponent<Gun>();

                    prevGravity = bulletMove.gravity;
                    bulletMove.gravity = 0.0f;

                    prevSpeed = bulletMove.velocity.magnitude;
                    bulletMove.velocity = bulletMove.velocity.normalized * initSpeed;

                    prevDrag = bulletMove.drag;
                    bulletMove.drag = 0.0f;

                    prevSimSpeed = Traverse.Create(bulletMove).Field("simulationSpeed").GetValue<float>();
                    Traverse.Create(bulletMove).Field("simulationSpeed").SetValue((float)1.0f);

                    targetDirection = shooterGun.shootPosition.forward;

                    // Miscs.Log("[GearUp] ParallelBulletModifier: Start");
                }
                catch (Exception exception)
                {
                    Miscs.LogWarn("[GearUP] ParallelBulletModifier: caught an exception (I swear this only happen once when it is first added to gun.objectToSpawn)");
                    Miscs.LogWarn(exception);
                    effectEnabled = false;
                }

                effectEnabled = true;
            }
        }

        public override HasToReturn DoHitEffect(HitInfo hit)
        {
            // in case bullet hit anything before going parallel, immediately restore the stats and destroy this
            timer += delayTime;

            return HasToReturn.canContinue;
        }

        public void Update()
        {
            if (effectEnabled)
            {
                timer += TimeHandler.deltaTime;

                if (timer >= delayTime)
                {
                    // plan B: delay alignment
                    bulletMove.gravity = prevGravity;
                    bulletMove.velocity = targetDirection * prevSpeed;
                    bulletMove.drag = prevDrag;
                    Traverse.Create(bulletMove).Field("simulationSpeed").SetValue((float) prevSimSpeed);

                    // Miscs.Log("[GearUp] ParallelBulletModifier: Updated and cleared");
                    Destroy(this);
                }
            }
        }
    }

    class FlakShellModifier : RayHitEffect
    {
        public static float delayTime = 1.5f;
        private float timer = 0.0f;
         
        private Gun shrapnelDummyGun = null;
        private Player shooterPlayer;

        public bool effectEnable = false;

        public void Setup()
        {
            shooterPlayer = gameObject.GetComponent<ProjectileHit>().ownPlayer;
            shrapnelDummyGun = gameObject.AddComponent<Gun>();
            Miscs.CopyGunStats(shooterPlayer.gameObject.GetComponent<UniqueGunSpreadMono>().dummySpreadGun, shrapnelDummyGun);

            effectEnable = true;
        }

        public void Update()
        {
            if (effectEnable)
            {
                timer += TimeHandler.deltaTime;
                if (timer >= delayTime)
                {
                    // explode after timer
                    FlakExplode();
                }
            }
        }

        public override HasToReturn DoHitEffect(HitInfo hit)
        {

            if (hit.transform == null)
            {
                return HasToReturn.canContinue;
            }
            if (hit.transform.gameObject.tag.Contains("Bullet"))
            {
                return HasToReturn.canContinue;
            }

            if (hit.transform.GetComponent<Player>())
            {
                // explode immediately
                FlakExplode();
            }

            return HasToReturn.canContinue;
        }

        public void FlakExplode()
        {
            if (shrapnelDummyGun != null)
            {
                Traverse.Create(shrapnelDummyGun).Field("forceShootDir").SetValue((Vector3) new Vector3(1.0f, 0.0f, 0.0f));
                shrapnelDummyGun.Attack(0.0f, true, useAmmo: false);

                Traverse.Create(shrapnelDummyGun).Field("forceShootDir").SetValue((Vector3)new Vector3(-1.0f, 0.0f, 0.0f));
                shrapnelDummyGun.Attack(0.0f, true, useAmmo: false);

                Traverse.Create(shrapnelDummyGun).Field("forceShootDir").SetValue((Vector3)new Vector3(0.0f, 1.0f, 0.0f));
                shrapnelDummyGun.Attack(0.0f, true, useAmmo: false);

                Traverse.Create(shrapnelDummyGun).Field("forceShootDir").SetValue((Vector3)new Vector3(0.0f, -1.0f, 0.0f));
                shrapnelDummyGun.Attack(0.0f, true, useAmmo: false);
            }
            else
            {
                Miscs.LogWarn("[GearUp] FlakShellModifier: Dummy Gun is NULL!");
            }

            this.ExecuteAfterFrames(1, () =>
            {
                // Miscs.Log("Boom!");
                Destroy(gameObject);
            });
        }
    }
}
