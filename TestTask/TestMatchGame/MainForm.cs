using System;
using System.Drawing;
using System.Windows.Forms;
   
namespace TestMatchGame
{
    public partial class MainForm : Form
    {
        // INIT OBJECTS
        private readonly PictureBox boardBox = new PictureBox();
        private readonly PictureBox infoBox  = new PictureBox();

        private readonly GameButton startBtn = new GameButton();
        private readonly GameButton endBtn   = new GameButton();

        private readonly GameBoard boardValue = new GameBoard(
            Settings.ELEMENT_COUNT, 
            Settings.ELEMENT_COUNT,
            Settings.COLOR_COUNT);

        private readonly Timer startTimer = new Timer();
        private readonly Timer gameTimer  = new Timer();

        private readonly Font drawFont = new Font("Arial", 24);



        // MAIN FUNCTION
        public MainForm()
        {
            InitializeComponent();
            GetRoomConfiguration();
        }



        // GAME CONFIGURATION
        private void InitializeComponent ()
        {
            SuspendLayout ();
            ClientSize      = new Size(Settings.APP_WIDTH, Settings.APP_HEIGHT);
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            StartPosition   = FormStartPosition.CenterScreen;
            BackColor       = Color.White;
            Name  = "Main Form";
            Text  = "Test Game";
            Load += new EventHandler(PictureBoxSet);
            Load += new EventHandler(InfoBoxSet);
            Hide ();
            ResumeLayout (false);
        }
        private void GetRoomConfiguration ()
        {
            switch (Settings.ROOM_CFG)
            {
                case 1:
                    BackgroundImageSet (Properties.Resources.ImageMoscow);
                    GameButtonSet (startBtn, "PLAY", 400, 200, 150, 75, Color.Yellow);
                    GameButtonSet (endBtn,   "OK",   400, 230, 100, 50, Color.ForestGreen);
                    GameButtonMode (endBtn, false);
                    AddObject (endBtn);
                    AddObject (startBtn);
                    SetTimer (startTimer, Settings.GAME_STEP, "buttonType",   true);
                    SetTimer (gameTimer,  Settings.GAME_STEP, "gameLoopType", false);
                    break;

                case 2:
                    BackgroundImageSet (null);
                    RemoveObject (startBtn);
                    GameButtonMode (endBtn, false);
                    AddObject (boardBox);
                    AddObject (infoBox);
                    ResetTimer (startTimer);
                    RestartGameAction (boardBox, infoBox);
                    break;

                case 3:
                    ResetTimer (gameTimer);
                    GameButtonMode (endBtn, true);
                    break;
            }
        }



        // GAME METHODS
        private void AddObject (Control obj)
        {
            if (Controls.Contains(obj) == false)
            {
                Controls.Add(obj);
            }
        }
        private void RemoveObject (GameButton obj)
        {
            obj.Dispose();
        }
        private void BackgroundImageSet (Bitmap image)
        {
            BackgroundImage = image;
        }
        private void GameButtonMode (GameButton obj, bool show)
        {
            if (show == true)
            {
                obj.Show();
            }
            else
            {
                obj.Hide();
            }
        }
        private void RestartGameAction (PictureBox game, PictureBox info)
        {
            if (Settings.SEC_COUNT < Settings.SEC_SESSION)
            {
                Settings.SEC_COUNT = Settings.SEC_SESSION;
                boardValue.GameBoardRestart();
                game.Invalidate();
                info.Invalidate();
            }
        }



        // GAME BUTTON ACTION
        private void GameButtonSet(GameButton obj, string name, int centerX, int centerY, 
            int width, int height, Color color)
        {
            obj.Text      = name;
            obj.Font      = new Font(FontFamily.GenericSansSerif, 12.0f, FontStyle.Bold);
            obj.BackColor = color;
            obj.Width     = width;
            obj.Height    = height;

            obj.ButtonSetCenterPosition(centerX, centerY);

            if (obj == startBtn)
            {
                obj.MouseDown  += new MouseEventHandler (GameButtonMouseDown);
                obj.MouseEnter += new EventHandler (GameButtonMouseEnter);
                obj.MouseLeave += new EventHandler (GameButtonMouseLeave);
            }
            else if (obj == endBtn)
            {
                obj.MouseDown  += new MouseEventHandler (GameButtonGameOver);
            }
        }
        private void GameButtonMouseDown (object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                Settings.ROOM_CFG++;

                if (Settings.ROOM_CFG > 2)
                {
                    Settings.ROOM_CFG = 1;
                }

                GetRoomConfiguration();
            }
        }
        private void GameButtonMouseEnter (object sender, EventArgs e)
        {
            if(sender == startBtn)
            {
                startBtn.BackColor = Color.Magenta;
                startTimer.Stop();
            }
        }
        private void GameButtonMouseLeave (object sender, EventArgs e)
        {
            if(sender == startBtn)
            {
                startBtn.BackColor = Color.Yellow;
                startTimer.Start();
            }
        }
        private void GameButtonGameOver (object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                Settings.ROOM_CFG = 2;
                GetRoomConfiguration();
            }
        }


        
        // PICTURE BOX ACTION
        private void PictureBoxSet (object sender, EventArgs e)
        {
            boardBox.Size       = new Size  (400, 400);
            boardBox.Location   = new Point (200,   0);
            boardBox.Paint     += new PaintEventHandler (PictureBoxDrawBorder);
            boardBox.Paint     += new PaintEventHandler (PictureBoxDrawElements);
            boardBox.Paint     += new PaintEventHandler (PictureBoxDrawGameEnd);
            boardBox.MouseDown += new MouseEventHandler (PictureBoxMouseDown);
        }

        private void PictureBoxDrawElements (object sender, PaintEventArgs e)
        {
            for (int i = 0; i < Settings.ELEMENT_COUNT; i++)
            {
                for (int j = 0; j < Settings.ELEMENT_COUNT; j++)
                {
                    int index = boardValue.boardValues[i, j];

                    if (index >= 0)
                    {
                        // if   STANDARD
                        // else BONUS
                        if (boardValue.bonusValues[i, j] == 0)
                        {
                            // if       SELECT STATIC
                            // else if  UNSELECT MOVING
                            // else     UNSELECT STATIC

                            Image jewel = Settings.ImageCollection[index];

                            if (i == boardValue.currentSelectWidth
                            &&  j == boardValue.currentSelectHeight)
                            {
                                Rectangle rectElement = new Rectangle (
                                    Settings.START_X + i * Settings.ELEMENT_SIZE,
                                    Settings.START_Y + j * Settings.ELEMENT_SIZE,
                                    Settings.ELEMENT_SIZE,
                                    Settings.ELEMENT_SIZE);

                                Rectangle rectSelect = new Rectangle (
                                    Settings.START_X + i * Settings.ELEMENT_SIZE + 4,
                                    Settings.START_Y + j * Settings.ELEMENT_SIZE + 4,
                                    Settings.ELEMENT_SIZE - 8,
                                    Settings.ELEMENT_SIZE - 8);

                                Brush color = Settings.BrushCollection[index];
                                e.Graphics.FillRectangle(color, rectElement);
                                e.Graphics.DrawImage(jewel, rectSelect);
                            }
                            else if (boardValue.movingValues[i, j] > 0)
                            {
                                int   typeMove    = boardValue.movingValues[i, j];
                                float shiftWidth  = i;
                                float shiftHeight = j;

                                switch (typeMove)
                                {
                                    case (int)Settings.Moving.Up:
                                        shiftHeight = j - (float)Settings.ACTION_COUNT / Settings.ACTION_STEP;
                                        break;

                                    case (int)Settings.Moving.Right:
                                        shiftWidth = i + (float)Settings.ACTION_COUNT / Settings.ACTION_STEP;
                                        break;

                                    case (int)Settings.Moving.Down:
                                        shiftHeight = j + (float)Settings.ACTION_COUNT / Settings.ACTION_STEP;
                                        break;

                                    case (int)Settings.Moving.Left:
                                        shiftWidth = i - (float)Settings.ACTION_COUNT / Settings.ACTION_STEP;
                                        break;
                                }

                                Point pt = new Point (
                                    Settings.START_X + (int)(shiftWidth * Settings.ELEMENT_SIZE),
                                    Settings.START_Y + (int)(shiftHeight * Settings.ELEMENT_SIZE));

                                e.Graphics.DrawImage (jewel, pt);
                            }
                            else
                            {
                                Point pt = new Point (
                                    Settings.START_X + i * Settings.ELEMENT_SIZE,
                                    Settings.START_Y + j * Settings.ELEMENT_SIZE);

                                e.Graphics.DrawImage(jewel, pt);
                            }
                        }
                        else
                        {
                            // if       SELECT STATIC
                            // else if  UNSELECT MOVING
                            // else     UNSELECT STATIC

                            Image bonus = Settings.ImageCollection[index];

                            switch (boardValue.bonusValues[i, j])
                            {
                                case (int)Settings.Bonus.LineVertical:
                                    bonus = Settings.LineVerticalCollection[index];
                                    break;

                                case (int)Settings.Bonus.LineHorizontal:
                                    bonus = Settings.LineHorizontalCollection[index];
                                    break;

                                case (int)Settings.Bonus.BonusBomb:
                                    bonus = Settings.BonusBombCollection[index];
                                    break;
                            }

                            if (i == boardValue.currentSelectWidth
                            &&  j == boardValue.currentSelectHeight)
                            {
                                Rectangle rectElement = new Rectangle (
                                    Settings.START_X + i * Settings.ELEMENT_SIZE,
                                    Settings.START_Y + j * Settings.ELEMENT_SIZE,
                                    Settings.ELEMENT_SIZE,
                                    Settings.ELEMENT_SIZE);

                                Rectangle rectSelect = new Rectangle (
                                    Settings.START_X + i * Settings.ELEMENT_SIZE + 4,
                                    Settings.START_Y + j * Settings.ELEMENT_SIZE + 4,
                                    Settings.ELEMENT_SIZE - 8,
                                    Settings.ELEMENT_SIZE - 8);

                                Brush color = Settings.BrushCollection[index];

                                e.Graphics.FillRectangle (color, rectElement);
                                e.Graphics.DrawImage     (bonus, rectSelect);
                            }
                            else if (boardValue.movingValues[i, j] > 0)
                            {
                                int   typeMove    = boardValue.movingValues[i, j];
                                float shiftWidth  = i;
                                float shiftHeight = j;

                                switch (typeMove)
                                {
                                    case (int)Settings.Moving.Up:
                                        shiftHeight = j - (float)Settings.ACTION_COUNT / Settings.ACTION_STEP;
                                        break;

                                    case (int)Settings.Moving.Right:
                                        shiftWidth = i + (float)Settings.ACTION_COUNT / Settings.ACTION_STEP;
                                        break;

                                    case (int)Settings.Moving.Down:
                                        shiftHeight = j + (float)Settings.ACTION_COUNT / Settings.ACTION_STEP;
                                        break;

                                    case (int)Settings.Moving.Left:
                                        shiftWidth = i - (float)Settings.ACTION_COUNT / Settings.ACTION_STEP;
                                        break;
                                }

                                Rectangle rectElement = new Rectangle (
                                    Settings.START_X + (int)(shiftWidth * Settings.ELEMENT_SIZE),
                                    Settings.START_Y + (int)(shiftHeight * Settings.ELEMENT_SIZE),
                                    Settings.ELEMENT_SIZE,
                                    Settings.ELEMENT_SIZE);

                                e.Graphics.DrawImage(bonus, rectElement);
                            }
                            else
                            {
                                Rectangle rectElement = new Rectangle (
                                    Settings.START_X + i * Settings.ELEMENT_SIZE,
                                    Settings.START_Y + j * Settings.ELEMENT_SIZE,
                                    Settings.ELEMENT_SIZE,
                                    Settings.ELEMENT_SIZE);

                                e.Graphics.DrawImage(bonus, rectElement);
                            }
                        }
                    }
                }
            }

            if (boardValue.actionDestroyer == true)
            {
                Image bonus = Properties.Resources.SpriteDestroyerSmoke;

                foreach (Destroyer obj in boardValue.destroyersList)
                {
                    int i    = obj.destPosX;
                    int j    = obj.destPosY;

                    float shiftWidth  = i;
                    float shiftHeight = j;

                    switch (obj.destMove)
                    {
                        case (int)Settings.Moving.Up:
                            shiftHeight = j - (float)Settings.ACTION_COUNT / Settings.ACTION_STEP;
                            break;

                        case (int)Settings.Moving.Right:
                            shiftWidth = i + (float)Settings.ACTION_COUNT / Settings.ACTION_STEP;
                            break;

                        case (int)Settings.Moving.Down:
                            shiftHeight = j + (float)Settings.ACTION_COUNT / Settings.ACTION_STEP;
                            break;

                        case (int)Settings.Moving.Left:
                            shiftWidth = i - (float)Settings.ACTION_COUNT / Settings.ACTION_STEP;
                            break;
                    }

                    Point pt = new Point (
                        Settings.START_X + (int)(shiftWidth * Settings.ELEMENT_SIZE),
                        Settings.START_Y + (int)(shiftHeight * Settings.ELEMENT_SIZE));

                    e.Graphics.DrawImage(bonus, pt);
                }
            }
            else if (boardValue.successLoop > 1)
            {
                string    strCombo  = "X " + boardValue.successLoop.ToString();
                PointF    ptfCombo  = new PointF    (170f, 185f);
                Rectangle rectCombo = new Rectangle (150,  175, 100, 50);

                e.Graphics.FillRectangle (Brushes.Black, rectCombo);
                e.Graphics.DrawString (strCombo, drawFont, Brushes.Yellow, ptfCombo);
            }
        }
        private void PictureBoxDrawBorder (object sender, PaintEventArgs e)
        {
            Pen drawGeneral = new Pen(Color.DeepSkyBlue)
            {
                Width = 8.0f
            };

            int margin = 10;
            int border = Settings.ELEMENT_COUNT * Settings.ELEMENT_SIZE;
            int startX = Settings.BOARD_X - border / 2;
            int startY = Settings.BOARD_Y - border / 2;

            Rectangle rectGeneral = new Rectangle (
                startX - margin, 
                startY - margin, 
                border + margin * 2, 
                border + margin * 2);

            e.Graphics.DrawRectangle (drawGeneral, rectGeneral);
        }
        private void PictureBoxDrawGameEnd (object sender, PaintEventArgs e)
        {
            if (Settings.ROOM_CFG == 3)
            {
                Pen penGameOver = new Pen(Brushes.Black)
                {
                    Width = 5f
                };

                Rectangle rectGameOver = new Rectangle (100,  100, 200, 200);
                PointF    ptfGameOver  = new PointF    (110f, 140f);
                string    strGameOver  = "Game Over";

                e.Graphics.FillRectangle (Brushes.Yellow, rectGameOver);
                e.Graphics.DrawRectangle (penGameOver, rectGameOver);
                e.Graphics.DrawString (strGameOver, drawFont, Brushes.Black, ptfGameOver);
            }
        }
        private void PictureBoxMouseDown (object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && Settings.SEC_COUNT > 0
            && boardValue.actionMove == false)
            {
                int indexWidth  = (e.X - Settings.START_X) / Settings.ELEMENT_SIZE;
                int indexHeight = (e.Y - Settings.START_Y) / Settings.ELEMENT_SIZE;

                if (gameTimer.Enabled == false)
                {
                    gameTimer.Start();
                }
                
                if (indexWidth >= 0 && indexWidth < Settings.ELEMENT_COUNT
                && indexHeight >= 0 && indexHeight < Settings.ELEMENT_COUNT)
                {
                    boardValue.GameBoardSetSelect (indexWidth, indexHeight);
                    PictureBoxRedraw (boardBox, indexWidth, indexHeight);

                    if (boardValue.oldSelectWidth > -1)
                    {
                        PictureBoxRedraw (boardBox, boardValue.oldSelectWidth, boardValue.oldSelectHeight);
                    }
                }
            }
        }
        
        private void PictureBoxRedraw (PictureBox obj)
        {
            Rectangle rectDraw = new Rectangle (
                Settings.START_X,
                Settings.START_Y,
                Settings.ELEMENT_SIZE * Settings.ELEMENT_COUNT,
                Settings.ELEMENT_SIZE * Settings.ELEMENT_COUNT
                );

            obj.Invalidate (rectDraw);
        }
        private void PictureBoxRedraw (PictureBox obj, int indexWidth, int indexHeight)
        {
            Rectangle rectRedraw = new Rectangle (
                Settings.START_X + indexWidth * Settings.ELEMENT_SIZE,
                Settings.START_Y + indexHeight * Settings.ELEMENT_SIZE,
                Settings.ELEMENT_SIZE,
                Settings.ELEMENT_SIZE);

            obj.Invalidate (rectRedraw);
        }
        private void PictureBoxRedraw (PictureBox obj, int newWidth, int newHeight, int oldWidth, int oldHeight)
        {
            int indexWidth   = newWidth;
            int indexHeight  = newHeight;
            int lenghtWidth  = Settings.ELEMENT_SIZE;
            int lenghtHeight = lenghtWidth;

            if (newWidth == oldWidth)
            {
                lenghtHeight *= 2;

                if (newHeight > oldHeight)
                {
                    indexHeight = oldHeight;
                }
            }
            else if (newHeight == oldHeight)
            {
                lenghtWidth *= 2;

                if (newWidth > oldWidth)
                {
                    indexWidth = oldWidth;
                }
            }

            Rectangle rectRedraw = new Rectangle (
                Settings.START_X + indexWidth * Settings.ELEMENT_SIZE,
                Settings.START_Y + indexHeight * Settings.ELEMENT_SIZE,
                lenghtWidth,
                lenghtHeight);

            obj.Invalidate (rectRedraw);
        }



        private void InfoBoxSet (object sender, EventArgs e)
        {
            infoBox.Size   = new Size (200, 400);
            infoBox.Paint += new PaintEventHandler(InfoBoxDrawScoreTime);
        }
        private void InfoBoxDrawScoreTime (object sender, PaintEventArgs e)
        {
            string strTimeTitle  = "Time:";
            PointF ptfTimeTitle  = new PointF(50f, 50f);

            string strTimeValue  = Settings.SEC_COUNT.ToString();
            PointF ptfTimeValue  = new PointF(70f, 90f);

            string strScoreTitle = "Score:";
            PointF ptfScoreTitle = new PointF(50f, 150f);

            string strScoreValue = Settings.SCORE.ToString();
            PointF ptfScoreValue = new PointF(70f, 190f);

            string strBestTitle  = "The Best:";
            PointF ptfBestTitle  = new PointF(50f, 250f);

            string strBestValue  = Settings.BEST.ToString();
            PointF ptfBestValue  = new PointF(70f, 290f);

            e.Graphics.DrawString(strTimeTitle,  drawFont, Brushes.Black, ptfTimeTitle);
            e.Graphics.DrawString(strTimeValue,  drawFont, Brushes.Black, ptfTimeValue);
            e.Graphics.DrawString(strScoreTitle, drawFont, Brushes.Black, ptfScoreTitle);
            e.Graphics.DrawString(strScoreValue, drawFont, Brushes.Black, ptfScoreValue);
            e.Graphics.DrawString(strBestTitle,  drawFont, Brushes.Black, ptfBestTitle);
            e.Graphics.DrawString(strBestValue,  drawFont, Brushes.Black, ptfBestValue);
        }



        // TIMER ACTION
        private void SetTimer (Timer obj, int interval, string type, bool start)
        {
            if (type == "buttonType")
            {
                obj.Tick += new EventHandler(TimerEventButton);
            }
            else if (type == "gameLoopType")
            {
                obj.Tick += new EventHandler(TimerEventGame);
            }

            obj.Interval = interval;

            if (start == true)
            {
                obj.Start ();
            }
        }
        private void ResetTimer (Timer obj)
        {
            obj.Stop ();
        }
        private void TimerEventButton (object sender, EventArgs e)
        {
            startBtn.ScaleAction ();
        }
        private void TimerEventGame (object sender, EventArgs e)
        { 
            if (boardValue.actionMove)
            {
                Settings.ACTION_COUNT++;

                if (boardValue.actionBomb)
                {
                    boardValue.actionBomb = false;
                    PictureBoxRedraw (boardBox);
                }
                else if (boardValue.actionDestroyer)
                {
                    if (Settings.ACTION_COUNT > Settings.ACTION_DEST)
                    {
                        Settings.ACTION_COUNT = 0;
                        boardValue.GameBoardAction ();
                        PictureBoxRedraw (boardBox);
                    }
                }
                else if (Settings.ACTION_COUNT > Settings.ACTION_STEP)
                {
                    Settings.ACTION_COUNT = 0;
                    boardValue.GameBoardAction ();
                    PictureBoxRedraw (boardBox);
                }
                else if(boardValue.actionFall == true)
                {
                    PictureBoxRedraw (boardBox);
                    infoBox.Invalidate ();
                }
                else
                {
                    PictureBoxRedraw (boardBox,
                        boardValue.newSelectWidth, boardValue.newSelectHeight,
                        boardValue.oldSelectWidth, boardValue.oldSelectHeight);
                }
            }

            Settings.STEP_COUNT++;

            if (Settings.STEP_COUNT >= Settings.GAME_FPS)
            {
                Settings.STEP_COUNT = 0;
                Settings.SEC_COUNT--;

                infoBox.Invalidate ();

                if (Settings.SEC_COUNT == 0)
                {
                    Settings.ROOM_CFG++;

                    boardBox.Invalidate ();
                    GetRoomConfiguration ();
                }
            }
        }
    }
}
