using UnityEngine;

namespace Unidice.SDK.Utilities
    {
        public enum TargetFPS { High, Low }

        public static class FPSManager
        {
            private static TargetFPS _fps;

            public static TargetFPS FPS
            {
                get => _fps;
                set => SetFPS(value);
            }

            private static void SetFPS(TargetFPS value)
            {
#if PLATFORM_WEBGL
                return; // WebGL handles FPS itself
#endif
                _fps = value;

                // Dynamic on mobile (30-60, let device cool down, save battery), 180fps on PCs
                const int lowFPS = 30;
                const int mediumFPS = 60;
                const int highFPS = 180;
                Application.targetFrameRate = Application.isMobilePlatform ? value == TargetFPS.Low ? lowFPS : mediumFPS : highFPS;
            }
        }
    }

