using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Linq;

namespace Mankalah
{  
    /*****************************************************************
    * A Mankalah player.  
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
         * h1 = the difference in score
         * h2 = stones in my home
         * h3 = stones in opponent's home
         * h4 = whether or not to go again
         * h5 = whether or not to capture opponent's pieces
         */
        public override int evaluate(Board b)
        {
            int h1;
            int h2;
            int h3;
            int h4 = 0;
            int h5 = 0;
            int target;
            int captureTarget;
            int score;

            if(me == Position.Top)
            {
                h2 = b.stonesAt(13); // stones in my home
                h3 = b.stonesAt(6);  // stones in opponent's home
                h1 = h2 - b.scoreBot(); // my margin of winning/losing
                for (int i = 7; i <13; i++)
                {
                    target = (i + b.stonesAt(i))%12;
                    captureTarget = ((target - 12) + (2 * (12 - target))); // lookup the spot on the opposite of the board for a potential capture
                    if (target == 13 && b.whoseMove() == me) // if the move will end in my home -- a go-again
                    {
                        h4 = 1;
                    }
                    else if(b.stonesAt(target) == 0 && target > 6 && b.whoseMove() == me)  // if it is possible to get a capture
                    {
                        if (b.stonesAt(captureTarget) > 2) // see if capture is worth it
                            h5 = 50;
                    }

                }
            }
            else
            { // if I'm on the bottom, do the same thing but changed for the bottom half of the board
                h2 = b.stonesAt(6);
                h3 = b.stonesAt(13);  
                h1 = h2 - b.scoreTop();
                for (int i = 0; i < 6; i++)
                {
                    target = (i + b.stonesAt(i))%12;
                    captureTarget = ((target + 12) - (2 * target));
                    if (target == 6 && b.whoseMove() == me)
                    {
                        h4 = 1;
                    }
                    else if (b.stonesAt(target) == 0 && target < 6 && b.whoseMove() == me)  // if it is possible to get a capture
                    {
                        if (b.stonesAt(captureTarget) > 2) // see if capture is worth it
                            h5 = 50;
                    }
                }
            }
            score = h1 + h2 + h5 + h4;
            score = (me == Position.Top) ? score : -1 * score;
            if (h2 > 24) // if we've won -- really want this
                score += 500;
            else if (h3 > 24) // if opponent has won -- really don't want this
                score -= 500; 
            return score;
        }

        /*
         * minimax function
         * based on the psuedocode from Prof Plantiga's lecture slides
         */
        private Result minimax(Board b, int depth, int alpha, int beta)
        {
            int bestNum = 0;
            int currentBestMove = 0;

            if (b.gameOver() || depth == 0)  // game is over or depth is 0
            {
                return new Result(0, evaluate(b));
            }

            if (b.whoseMove() == Position.Top) // searching moves for when top player
            {
                bestNum = Int32.MinValue;

                for (int move = 7; move <= 12; move++)
                {
                    if (b.legalMove(move))
                    {
                        Board b1 = new Board(b);
                        b1.makeMove(move, false);

                        Result num = minimax(b1, depth-1, alpha, beta);
                        if (num.getBestScore() > bestNum)
                        {
                            bestNum = num.getBestScore();
                            currentBestMove = move;
                        }
                        if (bestNum > alpha)
                            alpha = bestNum;
                    }
                }
                return new Result(currentBestMove, bestNum);
            }
            else // search moves for when bottom player
            {
                bestNum = int.MaxValue;
                for (int move = 0; move <= 5; move++)
                {
                    if (b.legalMove(move))
                    {
                        Board b1 = new Board(b);
                        b1.makeMove(move, false);

                        Result num = minimax(b1, (depth - 1), alpha, beta);
                        if (num.getBestScore() < bestNum)
                        {
                            bestNum = num.getBestScore();
                            currentBestMove = move;
                        }
                        if (bestNum < beta)
                            beta = bestNum;
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
            sw.Start();  // start stopwatch

            int depth = 1;
            Result nextMove = new Result(0, 0);
            do
            {
                nextMove = minimax(b, depth++, Int32.MinValue, Int32.MaxValue);
            } while (sw.ElapsedMilliseconds < timePerMove);   // search until the time cutoff

            return nextMove.getBestMove();
        }
    }
    /*
     * Result class to store results of minimax and return them easily
     */
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

