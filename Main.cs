using System.Collections.Generic;
using UnityEngine;
using MelonLoader;

namespace GorillaBlackOpsHUD
{
    public class Main : MelonMod
    {
        private List<GorillaPlayer> players = new();
        private GorillaPlayer localPlayer;
        private Camera vrCamera;
        private float refreshRate = 1f, refreshTimer;
        private KeyCode toggleKey = KeyCode.F1;
        private bool hudEnabled = true;

        public override void OnLateInitializeMelon()
        {
            MelonLogger.Msg("Booting Gorilla BlackOps HUD…");
            var rig = GorillaTagger.Instance?.myVRRig;
            if (rig != null)
            {
                localPlayer = rig.GetComponent<GorillaPlayer>();
                vrCamera    = rig.GetComponentInChildren<Camera>();
                MelonLogger.Msg($"VR camera found: {vrCamera.name}");
            }
        }

        public override void OnUpdate()
        {
            if (Input.GetKeyDown(toggleKey))
                hudEnabled = !hudEnabled;

            refreshTimer += Time.deltaTime;
            if (refreshTimer >= refreshRate)
            {
                players      = new List<GorillaPlayer>(Object.FindObjectsOfType<GorillaPlayer>());
                refreshTimer = 0f;
            }
        }

        public override void OnGUI()
        {
            if (!hudEnabled || localPlayer == null || vrCamera == null) return;

            foreach (var p in players)
            {
                if (p == null || p == localPlayer) continue;

                Vector3 worldPos = p.transform.position + Vector3.up * 0.3f;
                Vector3 sp       = vrCamera.WorldToScreenPoint(worldPos);
                if (sp.z <= 0) continue;

                float dist = Vector3.Distance(localPlayer.transform.position, p.transform.position);
                string prox = dist < 5f ? "CLOSE" : dist < 15f ? "NEAR" : "FAR";
                DrawHUD(sp, dist, prox, p);
            }
        }

        private void DrawHUD(Vector3 sp, float dist, string prox, GorillaPlayer p)
        {
            sp.y = Screen.height - sp.y;
            GUIStyle style = new(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize  = Mathf.Clamp(24 - (int)(dist/2), 12, 24),
                fontStyle = FontStyle.Bold
            };

            if (dist < 5f)
                style.normal.textColor = Color.Lerp(Color.red, Color.white, Mathf.PingPong(Time.time * 3, 1));
            else if (dist < 15f)
                style.normal.textColor = new Color(1f,0.5f,0f);
            else
                style.normal.textColor = Color.green;

            string nm = p.photonView?.Owner?.NickName ?? "Unknown";
            GUI.Label(new Rect(sp.x - 75, sp.y - 40, 150, 100),
                      $"<b>{nm}</b>\n<size=14>{dist:F1}m</size>\n[{prox}]", style);
        }
    }
}
