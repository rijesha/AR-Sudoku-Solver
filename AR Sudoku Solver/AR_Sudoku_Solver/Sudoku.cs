using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AR_Sudoku_Solver
{
    public class Sudoku
    {
        public int[] detectedNumbers;
        public double[] detectedProbabilites;
        int[] solvedNumbers = new int[81];
        List<int>[,] puzzle = new List<int>[9,9];
        List<int>[] detectedpuzzles = new List<int>[81];
        public bool puzzleUpdated = false;

        public Sudoku(int[] grid, double[] probs)
        {
            detectedNumbers = grid;
            detectedProbabilites = probs;
            int i = 0;
            foreach (var g in grid)
            {
                detectedpuzzles[i] = new List<int>(15);
                detectedpuzzles[i].Add(g);
                i++;
            }
        }

        override public int GetHashCode()
        {
            int hash = 7;
            foreach (var a in detectedNumbers)
            {
                hash = hash * 3 + a;
            }
            return hash;
        }

        public bool checkSolve()
        {
            return solvedNumbers.Sum() == 405;
        }

        public int GetSimilarHash()
        {
            int hash = 0;
            int i = 0;
            foreach (var a in detectedNumbers)
            {
                if (a != 0)
                {
                    hash += i % 3;
                }
                hash += a;
            }
            return hash;
        }

        public Sudoku GetSimilarSudoku(List<Sudoku> sudokus)
        {
            var simHash = GetSimilarHash();
            foreach (var s in sudokus)
            {
                if (simHash == s.GetHashCode() || Math.Abs(s.GetSimilarHash() - simHash) < 20)
                {
                    return s.combineSudokus(this);
                }
            }
            sudokus.Add(this);
            return this;
        }

        public Sudoku combineSudokus(Sudoku s)
        {
            puzzleUpdated = false;
            
            for (int i = 0; i < detectedNumbers.Count(); i++)
            {
                detectedpuzzles[i].Add(s.detectedNumbers[i]);
                int mode1 = mode(detectedpuzzles[i]);
                if (detectedNumbers[i] != mode1)
                {
                    detectedNumbers[i] = mode1;
                    puzzleUpdated = true;
                }
            }
            return this;
        }

        public int mode(List<int> l)
        {
            return l.GroupBy(i => i).OrderByDescending(grp => grp.Count()).Select(grp => grp.Key).First();
        }

        public void preparePuzzle()
        {
            int ind = 0;
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    if (detectedNumbers[ind] == 0)
                    {
                        puzzle[i,j] = new List<int>(new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 });
                    }
                    else
                    {
                        puzzle[i,j] = new List<int>(new int[] { detectedNumbers[ind] });
                    }
                    ind++;
                }
            }
        }

        public void solvePuzzle(CancellationToken ct)
        {
            var r = new RecursiveSudoku(detectedNumbers);
            solvedNumbers = r.SolveFully(ct);
            postRecursivePuzzle(solvedNumbers);

            /*
            int i = 0;
            bool stopSolving = false;
            while (!stopSolving)
            {

                i++;
                stopSolving = iterateSolveOnce();
                
                if (i > 1000)
                {
                    stopSolving = true;
                }
                if (i % 50 == 0)
                {
                    if (checkSolve())
                        break;
                }
                if (ct.IsCancellationRequested)
                {
                    break;
                }
            }
            postparePuzzle();
            */
        }

        public void postRecursivePuzzle(int[] solved)
        {
            int ind = 0;
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    if (detectedNumbers[ind] == 0)
                    {
                        solvedNumbers[ind] = solved[i];
                    }
                    ind++;
                }
            }
            StaticSKInfo.solvedNumbers = solvedNumbers;
        }

        public void postparePuzzle()
        {
            int ind = 0;
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    var l = puzzle[i,j];
                    if (l.Count == 1)
                    {
                        if (detectedNumbers[ind] == 0)
                        {
                            solvedNumbers[ind] = l.First();
                        } 
                    }
                    ind++;
                }
            }
            StaticSKInfo.solvedNumbers = solvedNumbers;
        }

        public bool iterateSolveOnce()
        {
            bool puzzleSolved = true;
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    var l = puzzle[i,j];
                    if (l.Count == 1)
                    {
                        eliminateBox(l.First(), i, j);
                        eliminateCollumn(l.First(), i, j);
                        eliminateRow(l.First(), i, j);
                    }
                    else
                    {
                        checkBox(i, j);
                    }
                    
                    if (l.Count > 1)
                    {
                        puzzleSolved = false;
                    }
                }
            }
            return puzzleSolved;
        }

        public bool checkBox(int row, int col)
        {
            int startrow = (row / 3) * 3;
            int maxrow = startrow + 3;
            int startcol = (col / 3) * 3;
            int maxcol = startcol + 3;
            foreach (var n in puzzle[row, col])
            {
                bool valueIsN = true;
                
                for (int i = startrow; i < maxrow; i++)
                {
                    for (int j = startcol; j < maxcol; j++)
                    {
                        var l = puzzle[row, i];
                        if (l.Contains(n) && i != row && j != col )
                        {
                            valueIsN = false;
                        }
                    }
                }
                for (int i = 0; i < 9; i++)
                {
                    var l = puzzle[row, i];
                    if (l.Contains(n) && i != col)
                    {
                        valueIsN = false;
                    }
                }
                for (int i = 0; i < 9; i++)
                {
                    var l = puzzle[i, col];
                    if (l.Contains(n) && i != row)
                    {
                        valueIsN = false;
                    }
                }
                if (valueIsN)
                {
                    puzzle[row, col] = new List<int>(new int[] { n });
                    return true;
                }
            }
            return false;
        }

        public void eliminateRow(int num, int row, int col)
        {
            for (int i = 0; i <9; i++)
            {
                var l = puzzle[i,col];
                if (l.Contains(num) && l.Count > 1)
                {
                    l.Remove(num);
                }
            }
        }

        public void eliminateCollumn(int num, int row, int col)
        {
            for (int i = 0; i < 9; i++)
            {
                var l = puzzle[row,i];
                if (l.Contains(num) && l.Count > 1)
                {
                    l.Remove(num);
                }
            }
        }

        public void eliminateBox(int num, int row, int col)
        {
            int startrow = (row / 3) * 3;
            int maxrow = startrow + 3;
            int startcol = (col / 3) * 3;
            int maxcol = startcol + 3;

            for (int i = startrow; i < maxrow; i++)
            {
                for (int j = startcol; j < maxcol; j++)
                {
                    var l = puzzle[i,j];
                    if (l.Contains(num) && l.Count > 1)
                    {
                        l.Remove(num);
                    }
                }
            }
        }
    }
    
}
