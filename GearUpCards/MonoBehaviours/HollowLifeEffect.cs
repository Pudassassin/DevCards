using System.Collections.Generic;
using UnboundLib;
using UnityEngine;
using System.Linq;
using UnboundLib.Networking;
using Photon.Pun;
using System.Reflection;
using ModdingUtils.MonoBehaviours;

namespace GearUpCards.MonoBehaviours
{
    internal class HollowLifeEffect : MonoBehaviour
    {
        private const float healthCapFactor = .70f;
        private const float procTime = .25f;

        private int stackCount = 0;
        float healthCapPercentage = 1.0f;

        float timer = 0.0f;

        private Player player;

        void Awake()
        {
            this.player = this.gameObject.GetComponent<Player>();
            this.enabled = false;
        }

        void Update()
        {
            timer += Time.deltaTime;
            if (timer > procTime)
            {
                // Check whether the player's health is above the certain caps, then apply health removal damage
                if (!player.data.dead)
                {
                    if (player.data.HealthPercentage > healthCapPercentage)
                    {
                        float healthCut = player.data.maxHealth * (player.data.HealthPercentage - healthCapPercentage);
                        // directly deduce player's health
                        player.data.health = player.data.maxHealth * healthCapPercentage;

                        // health removal/self damage is not count toward taking an actual damage, except for EXC's [Second Wind]
                        // player.data.healthHandler.DoDamage(new Vector2(0.0f, healthCut), Vector2.zero, Color.clear, healthRemoval: true, lethal: false, ignoreBlock: true);

                        // negative heal does nothing
                        // player.data.healthHandler.Heal(-healthCut);
                    }
                }

                timer -= procTime;
            }
        }

        public void OnDestroy()
        {

        }

        public void Destroy()
        {

        }

        public void AddStack()
        {
            this.stackCount += 1;
            this.healthCapPercentage = Mathf.Pow(healthCapFactor, stackCount);

            if (this.stackCount > 0)
            {
                this.enabled = true;
            }
        }

        public void RemoveStack()
        {
            // **Found desync issue on removal
            this.stackCount -= 1;
            this.healthCapPercentage = Mathf.Pow(healthCapFactor, stackCount);

            if (this.stackCount <= 0)
            {
                this.enabled = false;
            }
        }

    }
}
