using System;

namespace MGRBosses.Content.Systems.Cinematic
{
    public class Sequence
    {
        public int timeLeft;
        public Action action;
        public bool blocksInput;

        public Sequence(int timeLeft, Action action, bool blocksInput)
        {
            this.timeLeft = timeLeft;
            this.action = action;
            this.blocksInput = blocksInput;
        }

        public void Update()
        {
            action?.Invoke();
        }
    }
}
