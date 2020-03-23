using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crossword
{
    public class Grid
    {
        public int Width { get; }
        public int Height { get; }

        public Cell[,] Data { get; }

        public Grid(int width, int height)
        {
            Width = width;
            Height = height;

            Data = new Cell[Width, Height];
        }

        public Cell this[int x, int y]
        {
            get
            {
                return Data[x, y];
            }
            set
            {
                Data[x, y] = value;
            }
        }
    }
}
