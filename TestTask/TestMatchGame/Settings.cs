using System;
using System.Drawing;

namespace TestMatchGame
{
    public class Settings
    {
        // APPLICATION
        public const int APP_WIDTH  = 800;
        public const int APP_HEIGHT = 480;

        // GAME BOARD
        public const int BOARD_X = 200; // 400 of absolute
        public const int BOARD_Y = 200;

        public const int ELEMENT_SIZE  = 40;
        public const int ELEMENT_COUNT =  8;

        public const int START_X = BOARD_X - (ELEMENT_COUNT * ELEMENT_SIZE / 2);
        public const int START_Y = BOARD_Y - (ELEMENT_COUNT * ELEMENT_SIZE / 2);

        // GAME ROOM
        public static int ROOM_CFG = 1;

        // GAME SPEED
        public const  int ACTION_STEP  = 3;
        public const  int ACTION_DEST  = 1;
        public static int ACTION_COUNT = 0;

        public const  int GAME_STEP  =  40;
        public const  int BOMB_STEP  = 250;
        public static int STEP_COUNT =   0;

        public const  int GAME_FPS    = 1000 / GAME_STEP;
        public const  int SEC_SESSION = 60;
        public static int SEC_COUNT   = SEC_SESSION;

        // GAME PLAY
        public const  int COLOR_COUNT   = 5;
        public const  int SUCCESS_COUNT = 3;
        public const  int BONUS_COUNT   = 5;

        // GAME SCORE
        public static int SCORE = 0;
        public static int BEST  = 0;



        // IMAGES
        public static Image[] ImageCollection = new Image[COLOR_COUNT]
        {
            Properties.Resources.SpriteJewel_Blue,
            Properties.Resources.SpriteJewel_Green,
            Properties.Resources.SpriteJewel_Magenta,
            Properties.Resources.SpriteJewel_Orange,
            Properties.Resources.SpriteJewel_Red
        };

        public static Image[] LineHorizontalCollection = new Image[COLOR_COUNT]
        {
            Properties.Resources.SpriteLine_Blue,
            Properties.Resources.SpriteLine_Green,
            Properties.Resources.SpriteLine_Magenta,
            Properties.Resources.SpriteLine_Orange,
            Properties.Resources.SpriteLine_Red
        };

        public static Image[] LineVerticalCollection = new Image[COLOR_COUNT]
        {
            Properties.Resources.SpriteLine_Blue,
            Properties.Resources.SpriteLine_Green,
            Properties.Resources.SpriteLine_Magenta,
            Properties.Resources.SpriteLine_Orange,
            Properties.Resources.SpriteLine_Red
        };

        public static Image[] BonusBombCollection = new Image[COLOR_COUNT]
        {
            Properties.Resources.SpriteBomb_Blue,
            Properties.Resources.SpriteBomb_Green,
            Properties.Resources.SpriteBomb_Magenta,
            Properties.Resources.SpriteBomb_Orange,
            Properties.Resources.SpriteBomb_Red
        };

        public static Brush[] BrushCollection = new Brush[COLOR_COUNT]
        {
            Brushes.Blue,
            Brushes.MediumTurquoise,
            Brushes.DarkMagenta,
            Brushes.DarkOrange,
            Brushes.Crimson
        };



        // ENUMS
        public enum Moving : int
        {
            Up    = 1,
            Right = 2,
            Down  = 3,
            Left  = 4
        };

        public enum Bonus : int
        {
            LineVertical   = 1,
            LineHorizontal = 2,
            BonusBomb      = 3
        };



        // METHODS
        public static void InitVerticalLineImages()
        {
            for (int i = 0; i < COLOR_COUNT; i++)
            {
                Image transformed = LineVerticalCollection[i];
                transformed.RotateFlip(RotateFlipType.Rotate270FlipNone);

                LineVerticalCollection[i] = transformed;
            }
        }



        public static void Log(object a)
        {
            Console.WriteLine("{0}", a);
        }
        public static void Log(object a, object b)
        {
            Console.WriteLine("{0} {1}", a, b);
        }
        public static void Log(object a, object b, object c)
        {
            Console.WriteLine("{0} {1} {2}", a, b, c);
        }
        public static void Log(object a, object b, object c, object d)
        {
            Console.WriteLine("{0} {1} {2} {3}", a, b, c, d);
        }
        public static void Log(object a, object b, object c, object d, object e)
        {
            Console.WriteLine("{0} {1} {2} {3} {4}", a, b, c, d, e);
        }
        public static void Log(object a, object b, object c, object d, object e, object f)
        {
            Console.WriteLine("{0} {1} {2} {3} {4} {5}", a, b, c, d, e, f);
        }
    }
}
