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
        internal bool effectEnabled;
        internal bool effectApplied;
        internal bool wasDeactivated = false;

        public void Awake()
        {
            this.player = this.gameObject.GetComponent<Player>();
            this.gun = this.gameObject.GetComponent<WeaponHandler>().gun;
            this.stats = this.gameObject.GetComponent<CharacterStatModifiers>();

            GameModeManager.AddHook(GameModeHooks.HookPointStart, OnPointStart);
            GameModeManager.AddHook(GameModeHooks.HookPointEnd, OnPointEnd);
        }

        public void Start()
        {

        }

        // ideally stat would be reset back to default at round end, but the round winner wouldn't since they don't get to pick a card

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
        }

        private void RemoveModifier()
        {
            Miscs.Log("[GearUP] UniqueGunSpread: called remove");

            if (effectApplied)
            {
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
                prevSpread = gun.spread;
                prevEvenSpread = gun.evenSpread;
                prevSpreadMul = gun.multiplySpread;
                prevGravity = gun.gravity;

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

                gun.multiplySpread = prevSpreadMul * prevSpread * 50;
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

                gun.multiplySpread = prevSpreadMul * prevSpread * (50 + gun.numberOfProjectiles * 5);
            }
        }
    }

    class BulletNoClipModifier : MonoBehaviour
    {
        private const float delayTime = 1.5f;
        internal float timer = 0.0f;
        private bool effectEnabled = false;

        public void Awake()
        {
            if (transform.parent != null)
            {
                transform.parent.Find("Collider").gameObject.SetActive(false);
                effectEnabled = true;
            }
        }

        public void Update()
        {
            if (effectEnabled)
            {
                timer += TimeHandler.deltaTime;

                if (timer >= delayTime)
                {
                    transform.parent.Find("Collider").gameObject.SetActive(true);
                    Destroy(this);
                }
            }
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
            // in case bullet hit anything before going parallel
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
}
