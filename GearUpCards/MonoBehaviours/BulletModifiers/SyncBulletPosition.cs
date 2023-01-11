using Photon.Pun;
using UnityEngine;

namespace GearUpCards.MonoBehaviours
{
    internal class SyncBulletPosition : MonoBehaviour
    {
        public static float defaultSyncInterval = 0.05f;
        public static string RPCKey = GearUpCards.ModId + ":PosSync";

        public float interval;
        public float lastSent;
        public PhotonView view;

        private void Awake()
        {
            this.interval = defaultSyncInterval;
            this.view = GetComponentInParent<PhotonView>();
            GetComponentInParent<ChildRPC>().childRPCsVector2.Add(RPCKey, SyncPosition);
        }

        private void Update()
        {
            if (view != null && (view.IsMine) && Time.time > (this.lastSent + this.interval))
            {
                GetComponentInParent<ChildRPC>().CallFunction(RPCKey, (Vector2)this.transform.root.position);
                this.lastSent = Time.time;
            }
        }

        public void SyncPosition(Vector2 pos)
        {
            this.transform.root.position = pos;
        }

        private void OnDestroy()
        {
            GetComponentInParent<ChildRPC>()?.childRPCsVector2.Remove(RPCKey);
        }
    }
}
