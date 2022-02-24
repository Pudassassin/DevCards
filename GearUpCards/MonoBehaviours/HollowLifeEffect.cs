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
                // Check whether the player's health is above the certain caps, then adjust it accordingly
                // EXC's [Second Wind] implement it differently and seem to either look for health removal or it tries its best to heal up to said health point
                if (!player.data.dead)
                {
                    if (player.data.HealthPercentage > healthCapPercentage)
                    {
                        float healthCut = player.data.maxHealth * (player.data.HealthPercentage - healthCapPercentage);
                        // directly deduce player's health
                        player.data.health = player.data.maxHealth * healthCapPercentage;

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
