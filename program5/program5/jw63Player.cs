using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Linq;

namespace Mankalah
{  
    /*****************************************************************
    * A Mankalah player.  This is the base class for players.
    * You'll derive a player from this base. Be sure your player
    * works correctly both as TOP and as BOTTOM.
    *****************************************************************/
    public class jw63Player : Player
    {
        private Position me;
        private int timePerMove;	// time allowed per move in msec

        /*
         * constructor.
         */
        public jw63Player(Position pos, int maxTimePerMove) : base(pos, "JonathanWinkle", maxTimePerMove)
        {
            me = pos;
            timePerMove = maxTimePerMove;
        }

        /*
         * get image for tournament
         */
        public override String getImage() { return "jw63.png"; }

        /*
         * Override with your own personalized gloat.
         */
        public override String gloat() { return "Give me a real challenge."; }

        /*
         * Evaluate: return a number saying how much we like this board. 
         * TOP is MAX, so positive scores should be better for TOP.
         * This default just counts the score so far. Override to improve!
         */
        public override int evaluate(Board b)
        {
            int score =  b.stonesAt(13) - b.stonesAt(6); //use original example to evaluate the score

            int stones = 0;
            int goAgains = 0;
            int capturesVal = 0;
            int topSituation = 0;
            int bottomSituation = 0;


                for (int i = 7; i <=12; i++)
                {
                    stones += b.stonesAt(i);
                    if (b.stonesAt(i) - (13 - i) == 0)
                        goAgains++;

                    int captureTarget = i + b.stonesAt(i);
                    if (captureTarget < 13)
                    {
                        int targetStones = b.stonesAt(captureTarget);
                        if (b.whoseMove() == Position.Top)
                        {
                            if (targetStones == 0 && b.stonesAt(13 - captureTarget - 1) != 0)
                                capturesVal += b.stonesAt(13 - targetStones - 1);
                        }
                    }
                    topSituation = score + stones + goAgains + capturesVal;
                }

            stones = goAgains = capturesVal = 0;

                for (int i = 0; i <= 5; i++)
                {
                    stones += b.stonesAt(i);
                    if (b.stonesAt(i) - (6 - i) == 0)
                        goAgains++;

                    int captureTarget = i + b.stonesAt(i);
                    if (captureTarget < 6)
                    {
                        int targetStones = b.stonesAt(captureTarget);
                        if (b.whoseMove() == Position.Bottom)
                        {
                            if (targetStones == 0 && b.stonesAt(13 - captureTarget - 1) != 0)
                                capturesVal += b.stonesAt(13 - targetStones - 1);
                        }
                    }
                bottomSituation = score + stones + goAgains + capturesVal;
                }

            if ( b.whoseMove() == Position.Top)
            {
                if (me == Position.Top)
                {
                    return (topSituation - bottomSituation);
                }
                else
                {
                    return (bottomSituation - topSituation);
                }
            }
            else
            {
                if (me == Position.Bottom)
                {
                    return (bottomSituation - topSituation);
                }
                else
                {
                    return (topSituation - bottomSituation);
                }
            }
        }

        /*
         * minimax function
         * based on the psuedocode from Prof Plantiga's lecture slides
         */
        private Result minimax(Board b, int depth)
        {
            int bestNum = evaluate(b);
            int currentBestMove = 0;

            if (b.gameOver() || depth == 0)  // game is over or depth is 0
            {
                return new Result(0, evaluate(b));
            }

            if (b.whoseMove() == me)
            {
                for (int move = 7; move <= 12; move++)
                {
                    if (b.legalMove(move))
                    {
                        Board b1 = new Board(b);
                        b1.makeMove(move, false);

                        Result num = new Result(move, evaluate(b1));
                        if (num.getBestScore() > bestNum)
                        {
                            bestNum = num.getBestScore();
                            currentBestMove = move;
                        }
                    }
                }
                return new Result(currentBestMove, bestNum);
            }
            else
            {
                bestNum = int.MaxValue;
                for (int move = 0; move <= 5; move++)
                {
                    if (b.legalMove(move))
                    {
                        Board b1 = new Board(b);
                        b1.makeMove(move, false);

                        Result num = minimax(b1, (depth - 1));
                        if (num.getBestScore() < bestNum)
                        {
                            bestNum = num.getBestScore();
                            currentBestMove = move;
                        }
                    }
                }
                return new Result(currentBestMove, bestNum);

            }

        }

        /*
         * choosemove function
         */
        public override int chooseMove(Board b)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            int depth = 1;
            Result nextMove = new Result(0, 0);
            do
            {
                nextMove = minimax(b, depth++);
            } while (sw.ElapsedMilliseconds < timePerMove);

            return nextMove.getBestMove();
        }
    }

    class Result
    {
        private int bestMove;
        private int bestScore;

        public Result(int move, int score)
        {
            bestMove = move;
            bestScore = score;
        }

        public int getBestMove() { return bestMove; }
        public int getBestScore() { return bestScore; }
        public void assignBestMove(int move) { bestMove = move; }
        public void assignBestScore(int score) { bestScore = score; }
    }
}

