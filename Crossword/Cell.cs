using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crossword
{
    public class Cell
    {
        public char Solution { get; }
        public ConsoleColor Color { get; set; }

        public Cell(char solution, ConsoleColor color)
        {
            Solution = solution;
            Color = color;
        }
    }
}
