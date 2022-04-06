using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Photon.Pun;
using UnboundLib;

using UnityEngine;
using UnityEngine.SceneManagement;

namespace GearUpCards.Utils
{
    internal class Miscs
    {
        public static bool debugFlag = true;

        public static void Log(object message)
        {
            if (debugFlag)
            {
                UnityEngine.Debug.Log(message);
            }
        }

        public static void LogWarn(object message)
        {
            if (debugFlag)
            {
                UnityEngine.Debug.LogWarning(message);
            }
        }

        public static void LogError(object message)
        {
            if (debugFlag)
            {
                UnityEngine.Debug.LogError(message);
            }
        }
    }
}
