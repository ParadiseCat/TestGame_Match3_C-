
using System;
using System.Timers;

namespace TestMatchGame
{
    class Destroyer
    {
        public int id;
        public int destPosX;
        public int destPosY;
        public int destMove;

        public bool forDestroy;
        public bool bombType;
        public bool bangType;
        public bool bangShow;

        public  GameBoard eventBoard;
        private Timer     destroyerTimer;

        private int timerTime;
        const int timerStep = 50;

        public Destroyer (int posX, int posY, int move, int idind)
        {
            destPosX = posX;
            destPosY = posY;
            destMove = move;
            id       = idind;

            if (destMove == 0)
            {
                bombType = true;
                DestroyerSetBomb();
            }
            else if (destMove == -1)
            {
                bombType = true;
                bangType = true;
                bangShow = true;
            }
        }

        public void Dispose()
        {
            if (destroyerTimer != null)
            {
                destroyerTimer.Dispose();
            }
        }


        public void DestroyerMoveAction()
        {
            switch (destMove)
            {
                case (int)Settings.Moving.Right:
                    destPosX += 1;
                    break;

                case (int)Settings.Moving.Left:
                    destPosX -= 1;
                    break;

                case (int)Settings.Moving.Down:
                    destPosY += 1;
                    break;

                case (int)Settings.Moving.Up:
                    destPosY -= 1;
                    break;
            }
        }
        public bool DestroyerMoveCheck (int x0, int y0, int x1, int y1)
        {
            if (x0 > destPosX)
            {
                forDestroy = true;
                return false;
            }
            else if (y0 > destPosY)
            {
                forDestroy = true;
                return false;
            }
            else if (x1 < destPosX)
            {
                forDestroy = true;
                return false;
            }
            else if (y1 < destPosY)
            {
                forDestroy = true;
                return false;
            }

            return true;
        }



        public void DestroyerSetObjAction(GameBoard obj)
        {
            eventBoard = obj;
        }
        private void DestroyerSetBomb()
        {
            destroyerTimer = new Timer(timerStep);

            destroyerTimer.Elapsed += DestroyerBangBomb;
            destroyerTimer.Enabled  = true;
        }
        private void DestroyerBangBomb(Object obj, ElapsedEventArgs e)
        {
            timerTime += timerStep;

            if (timerTime >= Settings.BOMB_STEP)
            {
                destroyerTimer.Enabled = false;
                eventBoard.GameBoardDestroyerBombAction(this);
            }
        }
    }
}
