using System;
using System.Collections.Generic;
using System.Collections;

using UnityEngine;

using UnboundLib.GameModes;
using UnboundLib;
using HarmonyLib;

namespace GearUpCards.MonoBehaviours
{
    public class PortalMagickController : MonoBehaviour
    {
        public static float portalTeleportDelay = 0.75f;
        public static float procTickTime = 0.05f;

        private const string bulletGameObjectName = "Bullet_Base(Clone)";
        private const string bulletGameObjectTag = "Bullet";

        // private const string playerGameObjectName = "Player(Clone)";
        // private const string playerGameObjectTag = "Player";

        public int portalID = 0;
        public float portalDuration = 8.0f;
        public float portalSize = 1.0f;
        public float portalDamageAmp = 1.10f;
        public float portalBounceAdd = 3.1f;

        public GameObject portalBlue, portalOrange;
        private CircleMeshMono portalBlueCircle, portalOrangeCircle;

        private GameObject[] projectiles;
        internal PortalMagickTag.PortalColor portalColor;
        internal float distanceToBlueP, distanceToOrangeP;
        internal PortalMagickTag portalTag;

        internal float procTimer = 0.0f;
        internal float durationTimer = 0.0f;

        public void SetUp()
        {
            portalBlueCircle = portalBlue.GetComponentInChildren<CircleMeshMono>();
            portalOrangeCircle = portalOrange.GetComponentInChildren<CircleMeshMono>();

            GameModeManager.AddHook(GameModeHooks.HookPointEnd, OnPointEnd);
        }

        public void Update()
        {
            procTimer += TimeHandler.deltaTime;
            durationTimer += TimeHandler.deltaTime;

            if (procTimer >= procTickTime)
            {
                // resolve effect: projectiles
                projectiles = GameObject.FindGameObjectsWithTag(bulletGameObjectTag);
                foreach (GameObject item in projectiles)
                {
                    distanceToBlueP = (item.transform.position - portalBlue.transform.position).magnitude;
                    distanceToOrangeP = (item.transform.position - portalOrange.transform.position).magnitude;
                    portalTag = item.GetComponent<PortalMagickTag>();
                    
                    if (distanceToBlueP <= portalSize)
                    {
                        // check portal status tag
                        if (portalTag != null)
                        {
                            if (portalTag.lastPortalID == portalID)
                            {
                                continue;
                            }
                        }

                        // teleport to Orange Portal
                        portalColor = PortalMagickTag.PortalColor.blue;
                        item.transform.position += portalOrange.transform.position - portalBlue.transform.position;
                    }
                    else if (distanceToOrangeP <= portalSize)
                    {
                        // check portal status tag
                        if (portalTag != null)
                        {
                            if (portalTag.lastPortalID == portalID)
                            {
                                continue;
                            }
                        }

                        // teleport to Blue Portal
                        portalColor = PortalMagickTag.PortalColor.orange;
                        item.transform.position += portalBlue.transform.position - portalOrange.transform.position;
                    }
                    else
                    {
                        // ignore the rest
                        continue;
                    }

                    // tag the projectile
                    RayCastTrail rayCastTrail = item.GetComponent<RayCastTrail>();
                    Traverse.Create(rayCastTrail).Field("lastPos").SetValue((Vector3)item.transform.position);

                    portalTag = item.GetOrAddComponent<PortalMagickTag>();
                    portalTag.lastPortalColor = portalColor;
                    portalTag.lastPortalID = portalID;
                    portalTag.delayTimer = portalTeleportDelay;

                    // attempt to sync projectile
                    SyncBulletPosition syncMono = item.GetComponentInChildren<SyncBulletPosition>();
                    if (syncMono != null)
                    {
                        syncMono.CallSyncPosition();
                    }
                    else
                    {
                        syncMono = item.AddComponent<SyncBulletPosition>();
                        syncMono.enableIntervalSync = false;
                        syncMono.CallSyncPosition();
                    }

                    // boost regular gun bullets
                    if (item.name == bulletGameObjectName)
                    {
                        ProjectileHit projectileHit = item.GetComponent<ProjectileHit>();
                        projectileHit.dealDamageMultiplierr *= portalDamageAmp;

                        RayHitReflect rayHitReflect = item.GetComponent<RayHitReflect>();
                        if (rayHitReflect == null)
                        {
                            rayHitReflect = item.transform.root.gameObject.AddComponent<RayHitReflect>();
                            rayHitReflect.reflects = Mathf.FloorToInt(portalBounceAdd);
                            rayHitReflect.speedM = 1.0f;
                            rayHitReflect.dmgM = 1.0f;
                            projectileHit.effects.Add(rayHitReflect);
                        }
                        else
                        {
                            rayHitReflect.reflects += Mathf.FloorToInt(portalBounceAdd);
                        }

                    }
                }

                // resolve effect: players
                foreach (Player item in PlayerManager.instance.players)
                {
                    distanceToBlueP = (item.transform.position - portalBlue.transform.position).magnitude;
                    distanceToOrangeP = (item.transform.position - portalOrange.transform.position).magnitude;
                    portalTag = item.gameObject.GetComponent<PortalMagickTag>();

                    if (item.transform.localScale.x > portalSize * 2.0f)
                    {
                        // don't let oversized player teleports
                        continue;
                    }
                    if (distanceToBlueP <= portalSize)
                    {
                        // check portal status tag
                        if (portalTag != null)
                        {
                            if (portalTag.lastPortalID == portalID)
                            {
                                continue;
                            }
                        }

                        // teleport to Orange Portal
                        portalColor = PortalMagickTag.PortalColor.blue;
                        item.transform.position += portalOrange.transform.position - portalBlue.transform.position;
                    }
                    else if (distanceToOrangeP <= portalSize)
                    {
                        // check portal status tag
                        if (portalTag != null)
                        {
                            if (portalTag.lastPortalID == portalID)
                            {
                                continue;
                            }
                        }

                        // teleport to Blue Portal
                        portalColor = PortalMagickTag.PortalColor.orange;
                        item.transform.position += portalBlue.transform.position - portalOrange.transform.position;
                    }
                    else
                    {
                        // ignore the rest
                        continue;
                    }

                    // tag the player
                    PlayerCollision playerCollision = item.gameObject.GetComponent<PlayerCollision>();
                    playerCollision.IgnoreWallForFrames(2);

                    portalTag = item.gameObject.GetOrAddComponent<PortalMagickTag>();
                    portalTag.lastPortalColor = portalColor;
                    portalTag.lastPortalID = portalID;
                    portalTag.delayTimer = portalTeleportDelay;
                }

                procTimer -= procTickTime;
            }

            if (portalBlueCircle != null)
            {
                portalBlueCircle.startAngle = durationTimer / portalDuration * 360.0f;
            }
            if (portalOrangeCircle != null)
            {
                portalOrangeCircle.startAngle = durationTimer / portalDuration * 360.0f;
            }

            if (durationTimer > portalDuration)
            {
                CleanUp();
            }
        }

        public void CleanUp()
        {
            GameModeManager.RemoveHook(GameModeHooks.HookPointEnd, OnPointEnd);

            Destroy(portalBlue);
            Destroy(portalOrange);
            Destroy(gameObject);
        }

        private IEnumerator OnPointEnd(IGameModeHandler gm)
        {
            CleanUp();

            yield break;
        }
    }

    public class PortalMagickTag : MonoBehaviour
    {
        public enum PortalColor
        {
            blue,
            orange
        }

        public float portalTeleportDelay;
        public float delayTimer = 0.0f;
        public int lastPortalID;
        public PortalColor lastPortalColor;

        public void LateUpdate()
        {
            delayTimer -= TimeHandler.deltaTime;
            if (delayTimer <= 0.0f)
            {
                Destroy(this);
            }
        }
    }
}
