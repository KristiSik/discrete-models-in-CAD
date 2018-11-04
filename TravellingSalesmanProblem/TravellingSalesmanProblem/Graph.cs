using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TravellingSalesmanProblem
{
    public class Graph
    {
        public int[][] AdjacencyMatrix { get; set; }
        public int EdgeNumber { get; set; }
        private const int INFINITE = -1;
        private const int REMOVED = -2;
        private int minCost = int.MaxValue;
        
        public class Edge
        {
            public int StartNode { get; set; }
            public int EndNode { get; set; }
            public int Weight { get; set; }
        }

        public void ReadAdjacencyMatrix(string filename)
        {
            FileStream fs = new FileStream(filename, FileMode.Open);
            StreamReader sr = new StreamReader(fs);
            string size = sr.ReadLine();
            int.TryParse(size, out int n);
            AdjacencyMatrix = new int[n][];
            for (int i = 0; i < n; i++)
            {
                string[] values = sr.ReadLine().Split(' ');
                AdjacencyMatrix[i] = new int[n];
                for (int j = 0; j < n; j++)
                {
                    AdjacencyMatrix[i][j] = int.Parse(values[j]);
                }
            }
            sr.Close();
            fs.Close();
        }

        public void TravellingSalesmanProblem(int[][] matrix, int cost, List<Edge> edges)
        {
            bool noValues = true;
            for (int i = 0; i < matrix.Length; i++)
            {
                int minInRow = FindMinInRow(matrix, i);
                if (minInRow != int.MaxValue)
                {
                    matrix = SubstractFromRow(matrix, i, minInRow);
                    cost += minInRow;
                    noValues = false;
                }
            }
            for (int i = 0; i < matrix[0].Length; i++)
            {
                int minInColumn = FindMinInColumn(matrix, i);
                if (minInColumn != int.MaxValue)
                {
                    matrix = SubstractFromColumn(matrix, i, minInColumn);
                    cost += minInColumn;
                    noValues = false;
                }
            }
            if (!noValues)
            {
                Console.WriteLine("After substracting:");
                PrintMatrix(matrix);
                matrix = RemoveMaxCoefficient(matrix, edges);
                PrintMatrix(matrix);
            } else
            {
                cost = edges.Sum(e => e.Weight);
                Console.WriteLine("COST:" + cost);
            }
            Console.WriteLine($"Cost: {cost}");
            if (cost < minCost)
            {
                TravellingSalesmanProblem(matrix, cost, edges);
            }
            if (cost < minCost)
            {
                minCost = cost;
            }
        }

        private int FindMinInRow(int[][] array, int row)
        {
            int min = int.MaxValue;
            for(int i = 0; i < array[row].Length; i++)
            {
                if (array[row][i] != INFINITE && array[row][i] != REMOVED && array[row][i] < min)
                {
                    min = array[row][i];
                }
            }
            return min;
        }

        private int FindMinInColumn(int[][] array, int column)
        {
            int min = int.MaxValue;
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i][column] != INFINITE && array[i][column] != REMOVED && array[i][column] < min)
                {
                    min = array[i][column];
                }
            }
            return min;
        }

        private int[][] SubstractFromRow(int[][] array, int row, int value)
        {
            for(int i = 0; i < array[row].Length; i++)
            {
                if (array[row][i] != INFINITE && array[row][i] != REMOVED)
                {
                    array[row][i] -= value;
                }
            }
            return array;
        }

        private int[][] SubstractFromColumn(int[][] array, int column, int value)
        {
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i][column] != INFINITE && array[i][column] != REMOVED)
                {
                    array[i][column] -= value;
                }
            }
            return array;
        }

        private int[][] RemoveMaxCoefficient(int[][] array, List<Edge> edges)
        {
            int maxCoefI = 0, maxCoefJ = 0, maxCoef = int.MinValue;
            for (int i = 0; i < array.Length; i++)
            {
                for (int j = 0; j < array[0].Length; j++)
                {
                    int coef = 0;
                    if (array[i][j] == 0) {
                        coef = CalculateCoefficient(array, i, j);
                        if (coef > maxCoef)
                        {
                            maxCoefI = i;
                            maxCoefJ = j;
                            maxCoef = coef;
                        }
                    }
                }
            }
            for (int i = 0; i < array.Length; i++)
            {
                array[maxCoefI][i] = REMOVED;
            }
            for (int i = 0; i < array[0].Length; i++)
            {
                array[i][maxCoefJ] = REMOVED;
            }
            edges.Add(new Edge() {
                StartNode = maxCoefI,
                EndNode = maxCoefJ,
                Weight = AdjacencyMatrix[maxCoefI][maxCoefJ]
            });
            Console.WriteLine($"Edge ({maxCoefI}, {maxCoefJ})");
            return array;
        }

        private int CalculateCoefficient(int[][] array, int x, int y)
        {
            int minInRow = int.MaxValue, minInColumn = int.MaxValue;
            for (int i = 0; i < array.Length; i++)
            {
                if (i != x && array[i][y] != INFINITE && array[i][y] != REMOVED && array[i][y] != 0 && array[i][y] < minInColumn)
                {
                    minInColumn = array[i][y];
                }
            }
            for (int j = 0; j < array[0].Length; j++)
            {
                if (j != y && array[x][j] != INFINITE && array[x][j] != REMOVED && array[x][j] != 0 && array[x][j] < minInRow)
                {
                    minInRow = array[x][j];
                }
            }
            if (minInColumn == int.MaxValue)
            {
                minInColumn = 0;
            }
            if (minInRow == int.MaxValue)
            {
                minInRow = 0;
            }
            return minInColumn + minInRow;
        }

        private void PrintMatrix(int[][] matrix)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("\t");
            for (int i = 0; i < matrix.Length; i++)
            {
                Console.Write($"{i}\t");
            }
            Console.WriteLine();
            for (int i = 0; i < matrix.Length; i++)
            {
                for (int j = 0; j < matrix.Length; j++)
                {
                    if (j == 0)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.Write($"{i}\t");
                        Console.ForegroundColor = ConsoleColor.Gray;
                    }
                    if (matrix[i][j] == INFINITE)
                    {
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.Write("INF\t");
                        Console.ForegroundColor = ConsoleColor.Gray;
                    }
                    else
                    if (matrix[i][j] == REMOVED)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.Write("INF\t");
                        Console.ForegroundColor = ConsoleColor.Gray;
                    }
                    else
                    {
                        Console.Write(matrix[i][j] + "\t");
                    }
                }
                Console.WriteLine();
            }
                Console.WriteLine();
        }
    }
}