/*
 * Author: Jeppe Andersen
 * Website: nocture.dk
 * 
 * Feel free to use this in any way you want :-)
 * 
 */
using System;
using System.Windows.Forms;

namespace Dijkstra
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new GUI());
        }
    }
}
