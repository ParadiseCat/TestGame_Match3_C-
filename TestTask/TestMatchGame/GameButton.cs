using System.Windows.Forms;

namespace TestMatchGame
{
    public class GameButton : Button
    {
        public int baseWidth  = 0;
        public int baseHeight = 0;
        public int centerX    = 0;
        public int centerY    = 0;

        private bool  grow       = false;
        private float scaleValue = 1f;

        private readonly float scaleStep    = 0.01f;
        private readonly float scaleMinimum = 0.9f;
        private readonly float scaleMaximum = 1.0f;

        public void ScaleAction ()
        {
            if (grow == true)
            {
                scaleValue += scaleStep;

                if (scaleValue >= scaleMaximum)
                {
                    grow = false;
                }
            }
            else
            {
                scaleValue -= scaleStep;

                if (scaleValue <= scaleMinimum)
                {
                    grow = true;
                }
            }

            Height = (int)(baseHeight * scaleValue);
            Width  = (int)(baseWidth * scaleValue);

            Left = centerX - (Width / 2);
            Top  = centerY - (Height / 2);
        }

        public void ButtonSetCenterPosition (int centerx, int centery)
        {
            centerX = centerx;
            centerY = centery;

            baseWidth = Width;
            baseHeight = Height;

            Left = centerX - (Width / 2);
            Top  = centerY - (Height / 2);
        }
    }
}
