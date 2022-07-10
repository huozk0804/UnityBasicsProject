using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace Basics04
{
    public class FrameRateCounter : MonoBehaviour
    {
        [SerializeField]
        TextMeshProUGUI display;
        int frames;
        float duration, bestDuration = float.MaxValue, worstDuration;
        [SerializeField, Range(0.1f, 2f)]
        float sampleDuration = 1f;

        public enum DispalyMode { FPS, MS }
        [SerializeField] private DispalyMode dispalyMode = DispalyMode.FPS;

        private void Start()
        {

        }

        private void Update()
        {
            float frameDuation = Time.unscaledDeltaTime;
            frames += 1;
            duration += frameDuation;

            if (frameDuation < bestDuration)
                bestDuration = frameDuation;
            if (frameDuation > worstDuration)
            {
                worstDuration = frameDuation;
            }

            if (duration >= sampleDuration)
            {
                if (dispalyMode == DispalyMode.FPS)
                {
                    display.SetText("FPS\n{0:0}\n{1:0}\n{2:0}", 1f / bestDuration, frames / duration, 1f / worstDuration);
                }
                else
                {
                    display.SetText("MS\n{0:1}\n{1:1}\n{2:1}", 1000f * bestDuration, 1000f * duration / frames, 1000f * worstDuration);
                }
                frames = 0;
                duration = 0;
                bestDuration = float.MaxValue;
                worstDuration = 0f;
            }

        }
    }
}
