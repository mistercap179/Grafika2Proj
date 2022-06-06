using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1.Logic
{
    internal class MatrixPlacement
    {
        public static List<int> pronadjiMjesto(double x, double y, int[,] matrica)
        {
            int X = (Int32)x - 1;
            int Y = (Int32)y - 1;
            List<int> povratna = new List<int>();

            if (X >= 235)
            {
                X--;
            }
            else if (Y >= 155)
            {
                Y--;
            }

            if (X < 0)
            {
                X = 0;
            }
            if (Y < 0)
            {
                Y = 0;
            }

            matrica[X, Y]++;
            povratna.Add(X);
            povratna.Add(Y);

            return povratna;
        }
    }
}
