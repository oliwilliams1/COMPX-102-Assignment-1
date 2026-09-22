using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Circuits
{
    /// <summary>
    /// This class implements an AND gate with two inputs
    /// and one output.
    /// </summary>
    public class NotGate : Gate
    {
        /// <summary>
        /// Initialises the Gate.
        /// </summary>
        /// <param name="x">The x position of the gate</param>
        /// <param name="y">The y position of the gate</param>
        public NotGate(int x, int y)
        {
            //Add the input pin to the gate
            pins.Add(new Pin(this, true, 20));
            //Add the output pin to the gate
            pins.Add(new Pin(this, false, 20));

            MoveTo(x, y);
        }

        /// <summary>
        /// Draws the gate in the normal colour or in the selected colour.
        /// </summary>
        /// <param name="paper"></param>
        public override void Draw(Graphics paper)
        {
            //Draw each of the pins
            foreach (Pin p in pins)
                p.Draw(paper);

            // Draw Gate
            if (selected)
                paper.DrawImage(Properties.Resources.NotGateAllRed, Left, Top, WIDTH, HEIGHT);
            else
                paper.DrawImage(Properties.Resources.NotGate, Left, Top, WIDTH, HEIGHT);
        }

        /// <summary>
        /// Moves the gate to the position specified.
        /// </summary>
        /// <param name="x">The x position to move the gate to</param>
        /// <param name="y">The y position to move the gate to</param>
        public override void MoveTo(int x, int y)
        {
            //Debugging message
            Console.WriteLine("pins = " + pins.Count);
            //Set the position of the gate to the values passed in
            left = x;
            top = y;
            // must move the pins too
            pins[0].X = x - GAP;
            pins[0].Y = y + HEIGHT / 2;
            pins[1].X = x + WIDTH + GAP;
            pins[1].Y = y + HEIGHT / 2;
        }
    }
}
