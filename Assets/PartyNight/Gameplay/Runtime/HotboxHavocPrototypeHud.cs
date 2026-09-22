using UnityEngine;

namespace PartyNight.Gameplay
{
    [DisallowMultipleComponent]
    public sealed class HotboxHavocPrototypeHud : MonoBehaviour
    {
        private HotboxHavocRoundController roundController;
        private GUIStyle titleStyle;
        private GUIStyle bodyStyle;
        private bool initialized;

        public bool IsInitialized => initialized;

        public void Initialize(HotboxHavocRoundController round)
        {
            if (initialized)
            {
                throw new System.InvalidOperationException(
                    "HotboxHavocPrototypeHud is already initialized.");
            }

            roundController = round != null
                ? round
                : throw new System.ArgumentNullException(nameof(round));
            initialized = true;
        }

        private void OnGUI()
        {
            if (!initialized || roundController == null)
            {
                return;
            }

            EnsureStyles();

            var scale = Mathf.Clamp(
                Mathf.Min(Screen.width / 1280f, Screen.height / 720f),
                0.65f,
                1.35f);
            var previousMatrix = GUI.matrix;
            GUI.matrix = Matrix4x4.TRS(
                Vector3.zero,
                Quaternion.identity,
                new Vector3(scale, scale, 1f));

            var oldColor = GUI.backgroundColor;
            GUI.backgroundColor = new Color(0.09f, 0.03f, 0.14f, 0.92f);
            GUI.Box(new Rect(28f, 28f, 370f, 178f), GUIContent.none);
            GUI.backgroundColor = oldColor;

            GUI.Label(new Rect(48f, 42f, 330f, 36f), "HOTBOX HAVOC", titleStyle);
            GUI.Label(new Rect(48f, 84f, 330f, 104f), BuildStatusText(), bodyStyle);

            GUI.matrix = previousMatrix;
        }

        private string BuildStatusText()
        {
            var phaseText = roundController.Phase switch
            {
                HotboxHavocRoundPhase.Countdown =>
                    $"ROUND STARTS IN {Mathf.CeilToInt(roundController.TimeRemaining)}",
                HotboxHavocRoundPhase.Active =>
                    $"SURVIVE  {roundController.TimeRemaining:0.0}s",
                HotboxHavocRoundPhase.Won => "YOU SURVIVED — ROUND WON",
                HotboxHavocRoundPhase.Eliminated => "TOO HAZY — ELIMINATED",
                _ => roundController.Phase.ToString(),
            };

            var exposure = roundController.IsPlayerOutsideClearZone
                ? $"  |  EXPOSURE {roundController.ExposureNormalized * 100f:0}%"
                : string.Empty;

            return
                $"{phaseText}\n" +
                $"HAZE {roundController.HazeNormalized * 100f:0}%  |  " +
                $"CLEAR ZONE {roundController.ClearRadius:0.0}m{exposure}\n" +
                $"LOCAL PROTOTYPE • ROUND {roundController.RoundIndex}";
        }

        private void EnsureStyles()
        {
            if (titleStyle != null)
            {
                return;
            }

            titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 26,
                fontStyle = FontStyle.Bold,
            };
            titleStyle.normal.textColor = new Color(0.42f, 1f, 0.58f);

            bodyStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 17,
                wordWrap = true,
            };
            bodyStyle.normal.textColor = Color.white;
        }
    }
}
