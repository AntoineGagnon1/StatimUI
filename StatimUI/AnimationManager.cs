using System;
using System.Collections.Generic;
using System.Text;

namespace StatimUI
{
    public static class AnimationManager
    {
        private static List<AnimationDesc> animationDescs = new();

        public static void Start(AnimationDesc animationDesc)
        {
            animationDescs.Add(animationDesc);
        }

        public static void Update(float deltaTime)
        {
            for (int i = 0; i < animationDescs.Count; i++)
            {
                var desc = animationDescs[i];

                if (desc.Paused)
                    continue;

                desc.CurrentTime += deltaTime;

                if (desc.CurrentTime >= desc.Duration)
                {
                    desc.IterationCount--;
                    if (desc.IterationCount > 0)
                        desc.CurrentTime = 0f;
                }

                if (desc.CurrentTime >= 0f)
                {
                    desc.Animation.Update(desc.Component, desc.Easing.Evaluate(desc.CurrentTime / desc.Duration));
                }
            }

            animationDescs.RemoveAll(desc => desc.CurrentTime >= desc.Duration); // TODO: optimize with unsorted remove and saving animations to remove in the main loop
        }
    }
}
