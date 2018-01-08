using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AR_Sudoku_Solver
{
    class RecursiveSudoku
    {

        private int[,] sudoku = new int[9, 9];
        public int[] grid;

        public RecursiveSudoku(int[] grid)
        {
            this.grid = grid;
            ConvertTo2D(grid);
        }

        public int[] SolveFully(CancellationToken ct)
        {
            solve(0, 0, ct);
            return ConvertTo1D(grid);
        }

        private void ConvertTo2D(int[] grid)
        {
            int k = 0;
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    sudoku[i,j] = grid[k];
                    k++;
                }
            }
        }

        private int[] ConvertTo1D(int[] grid)
        {
            int k = 0;
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    grid[k] = sudoku[i, j];
                    k++;
                }
            }
            return grid;
        }

        private int solve(int i, int j, CancellationToken ct)
        {
            if (ct.IsCancellationRequested)
                return 0;
            int permanent = 0;
            if (sudoku[i,j] != 0)
            {
                permanent = 1;
            }
            
            if (permanent == 1)
            {
                if (i == 8 && j == 8)
                {
                    return 1;
                }
                else
                {
                    if (j == 8)
                    {
                        if (solve(i + 1, 0, ct) == 1)
                        {
                            return 1;
                        }
                        return 0;
                    }
                    else
                    {
                        if (solve(i, j + 1, ct) == 1)
                        {
                            return 1;
                        }
                        return 0;
                    }
                }
            }

            for (int n = 1; n < 10; n++)
            {
                if ((recursiveboxcheck(i, j, n) == 0 && recursivecollumncheck(i, j, n) == 0 && recursiverowcheck(i, j, n) == 0))
                {
                    if (permanent == 0)
                        sudoku[i,j] = n;
                    if (i == 8 && j == 8)
                    {
                        return 1;
                    }
                    else
                    {
                        if (j == 8)
                        {
                            if (solve(i + 1, 0, ct) == 1)
                            {
                                return 1;
                            }
                        }
                        else
                        {
                            if (solve(i, j + 1, ct) == 1)
                            {
                                return 1;
                            }
                        }
                    }
                }
            }
            if (permanent == 0)
                sudoku[i,j] = 0;
            return 0;
        }

        private int recursiverowcheck(int i, int j, int n)
        {
            for (int l = 0; l < 9; l++)
            {
                if (sudoku[i,l] == n)
                {
                    return 1;
                }
            }
            return 0;
        }

        private int recursivecollumncheck(int i, int j, int n)
        {
            for (int l = 0; l < 9; l++)
            {
                if (sudoku[l,j] == n)
                {
                    return 1;
                }
            }
            return 0;
        }

        private int recursiveboxcheck(int i, int j, int n)
        {
            for (int boxrow = (i / 3) * 3, rmax = boxrow + 3; boxrow < rmax; boxrow++)
            {
                for (int boxcollumn = (j / 3) * 3, cmax = boxcollumn + 3; boxcollumn < cmax; boxcollumn++)
                {
                    if (sudoku[boxrow,boxcollumn] == n)
                    {
                        return 1;
                    }
                }
            }
            return 0;
        }
    }
}
