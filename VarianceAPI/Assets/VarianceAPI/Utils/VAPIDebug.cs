using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace VarianceAPI
{
    internal class VAPIDebug : MonoBehaviour
    {
        private void Awake()
        {
            #region networking
            //you can connect to yourself with a second instance of the game by hosting a private game with one and opening the console on the other and typing connect localhost:7777
            On.RoR2.Networking.GameNetworkManager.OnClientConnect += (self, user, t) => { };
            #endregion networking
        }
    }
}
