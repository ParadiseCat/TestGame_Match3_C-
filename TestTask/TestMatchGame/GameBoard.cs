
using System;
using System.Collections.Generic;
using System.Linq;

namespace TestMatchGame
{
    class GameBoard
    {
        public int[,] boardValues;
        public int[,] movingValues;
        public int[,] bonusValues;
        private readonly int[,] removeValues;
        private readonly int[,] oldValues;

        public int currentSelectWidth  = -1;
        public int currentSelectHeight = -1;
        public int oldSelectWidth  = -1;
        public int oldSelectHeight = -1;
        public int newSelectWidth  = -1;
        public int newSelectHeight = -1;
        public int successLoop = 1;
        public int destroyerId = 0;

        private int fallingSteps = 0;

        public bool actionMove;
        public bool actionFall;
        public bool actionUpdate;
        public bool actionDestroyer;
        public bool actionBomb;

        private bool actionReplace;
        private bool actionSuccess;
        private bool canMove = true;

        public           List<Destroyer> destroyersList = new List<Destroyer>();
        private readonly List<Destroyer> destroyersEnd = new List<Destroyer>();
        private readonly List<Destroyer> destroyersNew = new List<Destroyer>();

        private readonly int boardColorCount;
        private readonly int boardWidth;
        private readonly int boardHeight;
        private readonly int REMOVED = -1;
        private readonly int STATIC  =  0;

        private readonly Random rand = new Random();



        public GameBoard (int width, int height, int colorCount)
        {
            boardColorCount = colorCount;
            boardWidth      = width;
            boardHeight     = height;

            boardValues  = new int [width, height];
            movingValues = new int [width, height];
            bonusValues  = new int [width, height];
            removeValues = new int [width, height];
            oldValues    = new int [width, height];

            GameBoardValuesSetDefault (
                boardValues,  REMOVED, 
                movingValues, STATIC,
                bonusValues,  STATIC,
                removeValues, STATIC);

            GameBoardPiecesRandomAdd ();
            GameBoardInit ();
            GameBoardValuesOldState ();
        }



        // PUBLIC
        public void GameBoardSetSelect (int width, int height)
        {
            if (currentSelectWidth == -1)
            {
                currentSelectWidth  = width;
                currentSelectHeight = height;
            }
            else
            {
                oldSelectWidth  = currentSelectWidth;
                oldSelectHeight = currentSelectHeight;

                currentSelectWidth  = width;
                currentSelectHeight = height;

                int shiftSelectWidth  = oldSelectWidth  - currentSelectWidth;
                int shiftSelectHeight = oldSelectHeight - currentSelectHeight;

                int moving = 0;

                if (shiftSelectHeight == 0)
                {
                    if (shiftSelectWidth == 1)
                    {
                        moving = (int)(Settings.Moving.Right);
                    }
                    else if (shiftSelectWidth == -1)
                    {
                        moving = (int)(Settings.Moving.Left);
                    }
                }
                else if (shiftSelectWidth == 0)
                {
                    if (shiftSelectHeight == 1)
                    {
                        moving = (int)(Settings.Moving.Down);
                    }
                    else if (shiftSelectHeight == -1)
                    {
                        moving = (int)(Settings.Moving.Up);
                    }
                }

                if (moving > 0)
                {
                    actionMove  = true;
                    int reverse = moving + 2;

                    if (reverse > 4)
                    {
                        reverse -= 4;
                    }

                    movingValues[currentSelectWidth, currentSelectHeight] = moving;
                    movingValues[oldSelectWidth, oldSelectHeight] = reverse;

                    newSelectWidth  = currentSelectWidth;
                    newSelectHeight = currentSelectHeight;

                    currentSelectWidth  = -1;
                    currentSelectHeight = -1;
                }
            }
        }
        public void GameBoardAction ()
        {
            if (actionSuccess)
            {
                actionMove    = false;
                actionSuccess = false;
                successLoop   = 1;
            }
            else if (actionReplace)
            {
                GameBoardPiecesChooseReplace (newSelectWidth, newSelectHeight, oldSelectWidth, oldSelectHeight);

                actionMove    = false;
                actionReplace = false;
            }
            else if (actionDestroyer)
            {
                GameBoardDestroyerStepMove ();
            }
            else if (actionFall)
            {
                if (fallingSteps > 0)
                {
                    GameBoardValuesOldState ();
                    GameBoardPiecesMoveStepDown ();
                    GameBoardPiecesSetShiftDown ();
                    fallingSteps--;
                }
                else
                {
                    successLoop += 1;
                    GameBoardPiecesMoveStepDown ();

                    if (GameBoardCheck (false))
                    {
                        if (actionDestroyer)
                        {
                            GameBoardValuesSetDefault (movingValues, STATIC);
                            GameBoardDestroyerStepMove ();
                        }
                        else
                        {
                            fallingSteps = GameBoardPiecesFallRemove ();
                            GameBoardPiecesSetShiftDown ();
                        }
                    }
                    else
                    {
                        GameBoardValuesOldState ();

                        actionFall    = false;
                        actionSuccess = true;
                        successLoop  -= 1;

                        canMove = GameBoardPiecesCanMove();

                        if (! canMove)
                        {
                            GameBoardReinit();
                        }
                    }
                }
            }
            else
            {
                GameBoardPiecesChooseReplace(newSelectWidth, newSelectHeight, oldSelectWidth, oldSelectHeight);

                if (GameBoardCheck (false))
                {
                    actionFall = true;

                    if (actionDestroyer)
                    {
                        GameBoardValuesSetDefault (movingValues, STATIC);
                        GameBoardDestroyerStepMove ();
                    }
                    else
                    {
                        fallingSteps = GameBoardPiecesFallRemove ();
                        GameBoardPiecesSetShiftDown ();
                    }
                }
                else
                {
                    actionReplace       = true;
                    currentSelectWidth  = newSelectWidth;
                    currentSelectHeight = newSelectHeight;

                    GameBoardSetSelect (oldSelectWidth, oldSelectHeight);
                }
            }
        }
        public void GameBoardRestart ()
        {
            actionMove    = false;
            actionFall    = false;
            actionUpdate  = false;
            actionReplace = false;
            actionSuccess = false;

            fallingSteps  = 0;
            successLoop   = 0;

            if (Settings.SCORE > Settings.BEST)
            {
                Settings.BEST = Settings.SCORE;
            }

            GameBoardValuesSetDefault (
                boardValues, REMOVED,
                movingValues, STATIC,
                bonusValues, STATIC);

            GameBoardPiecesRandomAdd ();
            GameBoardInit ();
        }

        

        // PRIVATE
        private void GameBoardInit ()
        {
            while (GameBoardCheck (true) == true)
            {
                GameBoardPiecesMoveQuickDown ();
                GameBoardValuesSetDefault (movingValues, STATIC);
                GameBoardPiecesRandomAdd ();
            }

            canMove = GameBoardPiecesCanMove ();

            if (! canMove)
            {
                GameBoardReinit ();
            }

            Settings.SCORE = 0;
        }
        private void GameBoardReinit ()
        {
            while (! canMove)
            {
                GameBoardValuesSetDefault (
                    boardValues, REMOVED,
                    movingValues, STATIC);

                GameBoardPiecesRandomAdd();

                while (GameBoardCheck (true))
                {
                    GameBoardPiecesMoveQuickDown ();
                    GameBoardValuesSetDefault (movingValues, STATIC);
                    GameBoardPiecesRandomAdd ();
                }

                canMove = GameBoardPiecesCanMove ();
            }
        }



        private bool GameBoardCheck (bool quickMode)
        {
            bool needUpdate = false;
            bool checkSelected = false;

            if (quickMode)
            {
                // Vertical checking
                for (int i = 0; i < boardWidth; i++)
                {
                    int select_id = REMOVED;
                    int select_count = 0;

                    for (int j = 0; j < boardHeight; j++)
                    {
                        if (boardValues[i, j] == select_id)
                        {
                            select_count++;

                            if (j == boardWidth - 1)
                            {
                                j = boardWidth;
                                checkSelected = true;
                            }
                        }
                        else
                        {
                            select_id = boardValues[i, j];
                            checkSelected = true;
                        }

                        if (checkSelected == true)
                        {
                            if (select_count >= Settings.SUCCESS_COUNT)
                            {
                                needUpdate = true;

                                for (int rem = 0; rem < select_count; rem++)
                                {
                                    boardValues[i, j - rem - 1] = REMOVED;
                                }
                            }

                            checkSelected = false;
                            select_count = 1;
                        }
                    }
                }

                // Horizontal checking
                for (int j = 0; j < boardHeight; j++)
                {
                    int select_id = REMOVED;
                    int select_count = 0;

                    for (int i = 0; i < boardWidth; i++)
                    {
                        if (boardValues[i, j] == select_id)
                        {
                            select_count++;

                            if (i == boardHeight - 1)
                            {
                                i = boardHeight;
                                checkSelected = true;
                            }
                        }
                        else
                        {
                            select_id = boardValues[i, j];
                            checkSelected = true;
                        }

                        if (checkSelected == true)
                        {
                            if (select_count >= Settings.SUCCESS_COUNT)
                            {
                                needUpdate = true;

                                for (int rem = 0; rem < select_count; rem++)
                                {
                                    boardValues[i - rem - 1, j] = REMOVED;
                                }
                            }

                            checkSelected = false;
                            select_count = 1;
                        }
                    }
                }
            }
            else
            {
                // Vertical checking
                for (int i = 0; i < boardWidth; i++)
                {
                    int select_id    = REMOVED;
                    int select_count = 0;

                    for (int j = 0; j < boardHeight; j++)
                    {
                        if (boardValues[i, j] == select_id)
                        {
                            select_count++;

                            if (j == boardWidth - 1)
                            {
                                j = boardWidth;
                                checkSelected = true;
                            }
                        }
                        else
                        {
                            select_id     = boardValues[i, j];
                            checkSelected = true;
                        }

                        if (checkSelected)
                        {
                            if (select_id > REMOVED && select_count >= Settings.SUCCESS_COUNT)
                            {
                                needUpdate = true;

                                Settings.SCORE += select_count * successLoop;

                                for (int rem = 0; rem < select_count; rem++)
                                {
                                    if (bonusValues[i, j - rem - 1] > STATIC)
                                    {
                                        GameBoardDestroyersSet (
                                            i, j - rem - 1, bonusValues[i, j - rem - 1], destroyersList);
                                        bonusValues[i, j - rem - 1] = STATIC;
                                    }

                                    removeValues[i, j - rem - 1] = REMOVED;
                                }

                                if (select_count >= Settings.BONUS_COUNT)
                                {
                                    bonusValues[i, j - 3]  = (int)Settings.Bonus.BonusBomb;
                                    removeValues[i, j - 3] = STATIC;
                                }
                                else if (select_count > Settings.SUCCESS_COUNT)
                                {
                                    bool set = false;

                                    for (int rem = 0; rem < select_count; rem++)
                                    {
                                        if (oldValues[i, j - rem - 1] != boardValues[i, j - rem - 1])
                                        {
                                            bonusValues[i, j - rem - 1]  = (int)Settings.Bonus.LineVertical;
                                            removeValues[i, j - rem - 1] = STATIC;
                                            set = true;
                                            break;
                                        }
                                    }

                                    if (! set)
                                    {
                                        bonusValues[i, j - 2]  = (int)Settings.Bonus.LineVertical;
                                        removeValues[i, j - 2] = STATIC;
                                    }
                                }
                            }

                            checkSelected = false;
                            select_count  = 1;
                        }
                    }
                }

                // Horizontal checking
                for (int j = 0; j < boardHeight; j++)
                {
                    int select_id    = REMOVED;
                    int select_count = 0;

                    for (int i = 0; i < boardWidth; i++)
                    {
                        if (boardValues[i, j] == select_id)
                        {
                            select_count++;

                            if (i == boardHeight - 1)
                            {
                                i = boardHeight;
                                checkSelected = true;
                            }
                        }
                        else
                        {
                            select_id     = boardValues[i, j];
                            checkSelected = true;
                        }

                        if (checkSelected)
                        {
                            if (select_id > REMOVED && select_count >= Settings.SUCCESS_COUNT)
                            {
                                needUpdate    = true;
                                bool findBomb = false;

                                Settings.SCORE += select_count * successLoop;

                                for (int rem = 0; rem < select_count; rem++)
                                {
                                    if (bonusValues[i - rem - 1, j] > STATIC)
                                    {
                                        GameBoardDestroyersSet (
                                            i - rem - 1, j, bonusValues[i - rem - 1, j], destroyersList);
                                        bonusValues[i - rem - 1, j] = STATIC;
                                    }

                                    if (removeValues[i - rem - 1, j] == REMOVED)
                                    {
                                        findBomb = true;
                                        bonusValues[i - rem - 1, j]  = (int)Settings.Bonus.BonusBomb;
                                        removeValues[i - rem - 1, j] = STATIC;
                                    }
                                    else
                                    {
                                        removeValues[i - rem - 1, j] = REMOVED;
                                    }
                                }

                                if (! findBomb)
                                {
                                    if (select_count >= Settings.BONUS_COUNT)
                                    {
                                        bonusValues[i - 3, j]  = (int)Settings.Bonus.BonusBomb;
                                        removeValues[i - 3, j] = STATIC;
                                    }
                                    else if (select_count > Settings.SUCCESS_COUNT)
                                    {
                                        bool set = false;

                                        for (int rem = 0; rem < select_count; rem++)
                                        {
                                            if (oldValues[i - rem - 1, j] != boardValues[i - rem - 1, j])
                                            {
                                                bonusValues[i - rem - 1, j]  = (int)Settings.Bonus.LineHorizontal;
                                                removeValues[i - rem - 1, j] = STATIC;
                                                set = true;
                                                break;
                                            }
                                        }

                                        if (! set)
                                        {
                                            bonusValues[i - 2, j]  = (int)Settings.Bonus.LineVertical;
                                            removeValues[i - 2, j] = STATIC;
                                        }
                                    }
                                }
                            }

                            checkSelected = false;
                            select_count = 1;
                        }
                    }
                }
            }

            return needUpdate;
        }



        private void GameBoardValuesSetDefault (int[,] arrA, int defA)
        {
            for (int i = 0; i < boardWidth; i++)
            {
                for (int j = 0; j < boardHeight; j++)
                {
                    arrA[i, j] = defA;
                }
            }
        }
        private void GameBoardValuesSetDefault (int[,] arrA, int defA, int[,] arrB, int defB)
        {
            for (int i = 0; i < boardWidth; i++)
            {
                for (int j = 0; j < boardHeight; j++)
                {
                    arrA[i, j] = defA;
                    arrB[i, j] = defB;
                }
            }
        }
        private void GameBoardValuesSetDefault (
            int[,] arrA, int defA, int[,] arrB, int defB, int[,] arrC, int defC)
        {
            for (int i = 0; i < boardWidth; i++)
            {
                for (int j = 0; j < boardHeight; j++)
                {
                    arrA[i, j] = defA;
                    arrB[i, j] = defB;
                    arrC[i, j] = defC;
                }
            }
        }
        private void GameBoardValuesSetDefault (
            int[,] arrA, int defA, int[,] arrB, int defB, int[,] arrC, int defC, int[,] arrD, int defD)
        {
            for (int i = 0; i < boardWidth; i++)
            {
                for (int j = 0; j < boardHeight; j++)
                {
                    arrA[i, j] = defA;
                    arrB[i, j] = defB;
                    arrC[i, j] = defC;
                    arrD[i, j] = defD;
                }
            }
        }



        private void GameBoardValuesOldState ()
        {
            for (int i = 0; i < boardWidth; i++)
            {
                for (int j = 0; j < boardHeight; j++)
                {
                    oldValues[i, j] = boardValues[i, j];
                }
            }
        }



        private void GameBoardPiecesRandomAdd ()
        {
            for (int i = 0; i < boardWidth; i++)
            {
                for (int j = 0; j < boardHeight; j++)
                {
                    if (boardValues[i, j] == REMOVED)
                    {
                        boardValues[i, j] = rand.Next(boardColorCount);
                    }
                }
            }
        }
        private int  GameBoardPiecesFallRemove ()
        {
            int fall = 0;
            int countFall = 0;

            for (int i = 0; i < boardWidth; i++)
            {
                for (int j = 0; j < boardHeight; j++)
                {
                    if (removeValues[i, j] == REMOVED)
                    {
                        boardValues[i, j]  = REMOVED;
                        removeValues[i, j] = STATIC;

                        countFall++;
                    }
                }

                if (countFall > fall)
                {
                    fall = countFall;
                }

                countFall = 0;
            }

            return fall;
        }
        private void GameBoardPiecesSetShiftDown ()
        {
            for (int i = 0; i < boardWidth; i++)
            {
                bool shift = false;

                for (int j = boardHeight - 1; j >= 0; j--)
                {
                    if (shift && boardValues[i, j] > REMOVED)
                    {
                        movingValues [i, j] = (int)Settings.Moving.Down;
                    }
                    else if (boardValues [i, j] == REMOVED)
                    {
                        shift = true;
                    }
                }
            }
        }
        private void GameBoardPiecesMoveStepDown ()
        {
            for (int i = 0; i < boardWidth; i++)
            {
                bool shift = false;

                for (int j = boardHeight - 1; j >= 0; j--)
                {
                    if (shift)
                    {
                        movingValues[i, j] = STATIC;

                        if (j == 0)
                        {
                            boardValues[i, j] = rand.Next(boardColorCount);
                            bonusValues[i, j] = STATIC;
                        }
                        else
                        {
                            boardValues[i, j] = boardValues[i, j - 1];
                            bonusValues[i, j] = bonusValues[i, j - 1];
                        }
                    }
                    else if (boardValues [i, j] == REMOVED)
                    {
                        shift = true;

                        if (j == 0)
                        {
                            boardValues[i, j] = rand.Next(boardColorCount);
                            bonusValues[i, j] = STATIC;
                        }
                        else
                        {
                            boardValues[i, j] = boardValues[i, j - 1];
                            bonusValues[i, j] = bonusValues[i, j - 1];
                        }
                    }
                }
            }
        }
        private void GameBoardPiecesMoveQuickDown ()
        {
            for (int i = 0; i < boardWidth; i++)
            {
                int shift = 0;

                for (int j = boardHeight - 1; j >= 0; j--)
                {
                    if (boardValues [i, j] == REMOVED)
                    {
                        shift++;
                    }
                    else if (shift > 0)
                    {
                        boardValues[i, j + shift] = boardValues[i, j];
                        boardValues[i, j] = REMOVED;
                    }
                }
            }
        }
        private void GameBoardPiecesChooseReplace (int newX, int newY, int oldX, int oldY)
        {
            int replaceValue        = boardValues[newX, newY];
            boardValues[newX, newY] = boardValues[oldX, oldY];
            boardValues[oldX, oldY] = replaceValue;

            int replaceBonus        = bonusValues[newX, newY];
            bonusValues[newX, newY] = bonusValues[oldX, oldY];
            bonusValues[oldX, oldY] = replaceBonus;

            movingValues[newX, newY] = STATIC;
            movingValues[oldX, oldY] = STATIC;
        }
        private bool GameBoardPiecesCanMove ()
        {
            int index = REMOVED;

            for (int i = 0; i < boardWidth; i++)
            {
                for (int j = 0; j < boardHeight; j++)
                {
                    if (boardValues[i, j] == index)
                    {
                        if (i - 1 >= 0)
                        {
                            if ((j + 1 < boardHeight && boardValues[i - 1, j + 1] == index)
                            || (j - 2 >= 0 && boardValues[i - 1, j - 2] == index))
                            {
                                return true;
                            }
                        }
                        
                        if (i + 1 < boardWidth)
                        {
                            if ((j + 1 < boardHeight && boardValues[i + 1, j + 1] == index)
                            || (j - 2 >= 0 && boardValues[i + 1, j - 2] == index))
                            {
                                return true;
                            }
                        }

                        if ((j + 2 < boardHeight && boardValues[i, j + 2] == index)
                        || (j - 3 >= 0 && boardValues[i, j - 3] == index))
                        {
                            return true;
                        }
                    }

                    if (j == boardHeight - 1)
                    {
                        index = REMOVED;
                    }
                    else
                    {
                        index = boardValues[i, j];
                    }
                }
            }

            for (int j = 0; j < boardHeight; j++)
            {
                for (int i = 0; i < boardWidth; i++)
                {
                    if (boardValues[i, j] == index)
                    {
                        if (j - 1 >= 0)
                        {
                            if ((i + 1 < boardHeight && boardValues[j - 1, i + 1] == index)
                            || (i - 2 >= 0 && boardValues[j - 1, i - 2] == index))
                            {
                                return true;
                            }
                        }

                        if (j + 1 < boardWidth)
                        {
                            if ((i + 1 < boardHeight && boardValues[j + 1, i + 1] == index)
                            || (i - 2 >= 0 && boardValues[j + 1, i - 2] == index))
                            {
                                return true;
                            }
                        }

                        if ((i + 2 < boardHeight && boardValues[j, i + 2] == index)
                        || (i - 3 >= 0 && boardValues[j, i - 3] == index))
                        {
                            return true;
                        }
                    }

                    if (i == boardWidth - 1)
                    {
                        index = REMOVED;
                    }
                    else
                    {
                        index = boardValues[i, j];
                    }
                }
            }

            return false;
        }

        

        // DESTROYER PUBLIC
        public void GameBoardDestroyerBombAction (Destroyer obj)
        {
            int x = obj.destPosX;
            int y = obj.destPosY;

            bonusValues[x, y] = STATIC;
            boardValues[x, y] = REMOVED;

            destroyersEnd.Add(obj);
            obj.Dispose();

            for (var i = x - 1; i <= x + 1; i++)
            {
                if (i >= 0 && i < boardWidth)
                {
                    for (var j = y - 1; j <= y + 1; j++)
                    {
                        if (( i != x || j != y )
                        && j >= 0 && j < boardHeight
                        && boardValues[i, j] != REMOVED)
                        {
                            if (bonusValues[i, j] == STATIC)
                            {
                                if (GameBoardDestroyerCheckBombExist(i, j) == false)
                                {
                                    destroyerId += 1;
                                    Destroyer objDest = new Destroyer(i, j, REMOVED, destroyerId);
                                    destroyersNew.Add(objDest);
                                }

                                removeValues[i, j] = REMOVED;
                            }
                            else
                            {
                                GameBoardDestroyersSet(i, j, bonusValues[i, j], destroyersNew);
                                bonusValues[i, j] = STATIC;
                            }
                        }
                    }
                }
            }

            fallingSteps += 3;
            actionBomb = true;
        }



        // DESTROYER PRIVATE
        private void GameBoardDestroyersSet (int x, int y, int bonus, List<Destroyer> list)
        {
            actionDestroyer   = true;
            boardValues[x, y] = REMOVED;

            if (bonus == (int)Settings.Bonus.LineVertical)
            {
                Settings.SCORE += successLoop * boardHeight + 1;

                if (y < boardHeight - 1)
                {
                    destroyerId += 1;
                    Destroyer objDest = new Destroyer(x, y, (int)Settings.Moving.Down, destroyerId);
                    list.Add(objDest);
                }

                if (y > 0)
                {
                    destroyerId += 1;
                    Destroyer objDest = new Destroyer(x, y, (int)Settings.Moving.Up, destroyerId);
                    list.Add(objDest);
                }
            }
            else if (bonus == (int)Settings.Bonus.LineHorizontal)
            {
                Settings.SCORE += successLoop * boardHeight + 1;

                if (x < boardWidth - 1)
                {
                    destroyerId += 1;
                    Destroyer objDest = new Destroyer(x, y, (int)Settings.Moving.Right, destroyerId);
                    list.Add(objDest);
                }

                if (x > 0)
                {
                    destroyerId += 1;
                    Destroyer objDest = new Destroyer(x, y, (int)Settings.Moving.Left, destroyerId);
                    list.Add(objDest);
                }
            }
            else if (bonus == (int)Settings.Bonus.BonusBomb)
            {
                Settings.SCORE += successLoop * (boardHeight + boardWidth) + 2;

                destroyerId += 1;
                Destroyer objDest = new Destroyer(x, y, STATIC, destroyerId);
                objDest.DestroyerSetObjAction(this);
                list.Add(objDest);
            }
        }
        private void GameBoardDestroyerStepMove ()
        {
            foreach (Destroyer obj in destroyersEnd)
            {
                destroyersList.Remove(obj);
                obj.Dispose();
            }

            foreach (Destroyer obj in destroyersList)
            {
                int x = obj.destPosX;
                int y = obj.destPosY;

                if (bonusValues [x, y] > STATIC)
                {
                    GameBoardDestroyerStepMoveSetBonus(x, y, obj.destMove);
                }
                else if (! obj.bombType)
                {
                    movingValues [x, y] = STATIC;
                    bonusValues  [x, y] = STATIC;
                    boardValues  [x, y] = REMOVED;
                    removeValues [x, y] = REMOVED;

                    obj.DestroyerMoveAction();

                    x = obj.destPosX;
                    y = obj.destPosY;

                    if (obj.DestroyerMoveCheck(0, 0, boardWidth - 1, boardHeight - 1))
                    {
                        Settings.SCORE += successLoop;

                        if (bonusValues[x, y] > STATIC)
                        {
                            GameBoardDestroyerStepMoveSetBonus(x, y, obj.destMove);
                        }

                        boardValues[x, y] = REMOVED;
                    }
                    else
                    {
                        destroyersEnd.Add(obj);
                    }
                }
                else if (obj.bangType)
                {
                    if (obj.bangShow)
                    {
                        obj.bangShow = false;
                    }
                    else
                    {
                        movingValues[x, y] = STATIC;
                        bonusValues [x, y] = STATIC;
                        boardValues [x, y] = REMOVED;

                        destroyersEnd.Add(obj);
                    }
                }
            }

            foreach (Destroyer obj in destroyersEnd)
            {
                destroyersList.Remove(obj);
                obj.Dispose();
            }

            foreach (Destroyer obj in destroyersNew)
            {
                destroyersList.Add(obj);
            }

            if (destroyersList.Count == 0)
            {
                actionDestroyer = false;
                fallingSteps    = GameBoardPiecesFallRemove();
                GameBoardPiecesSetShiftDown();
            }

            destroyersNew.Clear();
            destroyersEnd.Clear();
        }
        private void GameBoardDestroyerStepMoveSetBonus (int x, int y, int move)
        {
            switch (bonusValues[x, y])
            {
                case (int)Settings.Bonus.LineHorizontal:

                    if ((move == (int)Settings.Moving.Up)
                    || (move == (int)Settings.Moving.Down))
                    {
                        GameBoardDestroyersSet (x, y, (int)Settings.Bonus.LineHorizontal, destroyersNew);
                    }
                    break;

                case (int)Settings.Bonus.LineVertical:

                    if ((move == (int)Settings.Moving.Left)
                    || (move == (int)Settings.Moving.Right))
                    {
                        GameBoardDestroyersSet(x, y, (int)Settings.Bonus.LineVertical, destroyersNew);
                    }
                    break;

                case (int)Settings.Bonus.BonusBomb:

                    if (boardValues[x, y] != REMOVED && GameBoardDestroyerCheckBombExist(x, y) == false)
                    {
                        GameBoardDestroyersSet(x, y, (int)Settings.Bonus.BonusBomb, destroyersNew);
                    }
                    break;
            }

            bonusValues [x, y] = STATIC;
        }
        private bool GameBoardDestroyerCheckBombExist (int x, int y)
        {
            var allDestroyers = destroyersNew.Concat(destroyersList);
            return allDestroyers.Any(d => d.destPosX == x && d.destPosY == y && d.bombType);
        }
    }
}
