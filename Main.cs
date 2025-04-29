using System.Collections.Generic;
using UnityEngine;
using MelonLoader;

namespace GorillaBlackOpsHUD
{
    public class Main : MelonMod
    {
        private List<GorillaPlayer> players = new List<GorillaPlayer>();
        private GorillaPlayer localPlayer;
        private Camera vrCamera;
        private float refreshRate = 1f;
        private float refreshTimer = 0f;
        private KeyCode toggleKey = KeyCode.F1;
        private bool hudEnabled = true;

        public override void OnLateInitializeMelon()
        {
            MelonLogger.Msg("Booting Gorilla BlackOps HUD…");
            var rigObj = GorillaTagger.Instance?.myVRRig;
            if (rigObj != null)
            {
                localPlayer = rigObj.GetComponent<GorillaPlayer>();
                vrCamera = rigObj.GetComponentInChildren<Camera>();
                MelonLogger.Msg($"Found VR camera: {vrCamera.name}");
            }
        }

        public override void OnUpdate()
        {
            if (Input.GetKeyDown(toggleKey))
                hudEnabled = !hudEnabled;

            refreshTimer += Time.deltaTime;
            if (refreshTimer >= refreshRate)
            {
                players = new List<GorillaPlayer>(Object.FindObjectsOfType<GorillaPlayer>());
                refreshTimer = 0f;
            }
        }

        public override void OnGUI()
        {
            if (!hudEnabled || localPlayer == null || vrCamera == null) 
                return;

            foreach (var player in players)
            {
                if (player == null || player == localPlayer) 
                    continue;

                Vector3 worldPos = player.transform.position + Vector3.up * 0.3f;
                Vector3 screenPos = vrCamera.WorldToScreenPoint(worldPos);
                if (screenPos.z <= 0) 
                    continue;

                float distance = Vector3.Distance(localPlayer.transform.position, player.transform.position);
                string proximity = GetProximityText(distance);
                DrawHUD(screenPos, distance, proximity, player);
            }
        }

        private string GetProximityText(float d)
        {
            if (d < 5f) return "CLOSE";
            if (d < 15f) return "NEAR";
            return "FAR";
        }

        private void DrawHUD(Vector3 sp, float dist, string prox, GorillaPlayer ply)
        {
            sp.y = Screen.height - sp.y;

            GUIStyle style = new GUIStyle(GUI.skin.label)
            {
                alignment   = TextAnchor.MiddleCenter,
                fontSize    = Mathf.Clamp(24 - (int)(dist / 2f), 12, 24),
                fontStyle   = FontStyle.Bold
            };

            if (dist < 5f)
                style.normal.textColor = Color.Lerp(Color.red, Color.white, Mathf.PingPong(Time.time * 3, 1));
            else if (dist < 15f)
                style.normal.textColor = new Color(1f, 0.5f, 0f);
            else
                style.normal.textColor = Color.green;

            string name = ply.photonView?.Owner?.NickName ?? "Unknown";
            GUI.Label(new Rect(sp.x - 75, sp.y - 40, 150, 100),
                $"<b>{name}</b>\n<size=14>{dist:F1}m</size>\n[{prox}]", style);
        }
    }
}
