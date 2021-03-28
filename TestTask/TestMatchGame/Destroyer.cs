
using System;
using System.ComponentModel;
using System.Timers;

namespace TestMatchGame
{
    class Destroyer : IDisposable
    {
        public int id       = 0;
        public int destPosX = 0;
        public int destPosY = 0;
        public int destMove = 0;

        public bool forDestroy = false;
        public bool bombType   = false;
        public bool bangType   = false;
        public bool bangShow   = false;

        public  GameBoard eventBoard;
        private Timer     destroyerTimer;

        private int timerTime = 0;
        private readonly int timerStep = 50;

        private bool disposed = false;
        private IntPtr handle;
        private readonly Component component = new Component();

        [System.Runtime.InteropServices.DllImport("Kernel32")]
        private extern static Boolean CloseHandle(IntPtr handle);



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
        
        ~ Destroyer()
        {
            Dispose(false);
        }



        public void Dispose()
        {
            if (destroyerTimer != null)
            {
                destroyerTimer.Dispose();
            }

            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (this.disposed == false)
            {
                if (disposing)
                {
                    component.Dispose();
                }

                CloseHandle(handle);
                handle = IntPtr.Zero;
                disposed = true;
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
