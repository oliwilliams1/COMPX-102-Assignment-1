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
    /// This class implements an input source
    /// </summary>
    public class InputSource : Gate
    {
        // Variables
        protected bool _value;  // true = high voltage, false = low voltage
        
        /// <summary>
        /// Initialises the Gate.
        /// </summary>
        /// <param name="x">The x position of the gate</param>
        /// <param name="y">The y position of the gate</param>
        public InputSource(int x, int y)
        {
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

            // Select the colour based on value
            Brush brush;
            if (_value)
                brush = Brushes.Green;
            else
                brush = Brushes.Gray;

            // Draw Gate
            paper.FillRectangle(brush, left, top, WIDTH, HEIGHT);

            // Select colour for border based on selection state
            Color color;
            if (selected)
                color = Color.Red;
            else
                color = Color.Black;

            using (Pen pen = new Pen(color, 10))
            {
                // Draw the border
                paper.DrawRectangle(
                    pen,
                    left + 5,
                    top + 5,
                    WIDTH - 10,
                    HEIGHT - 10
                );
            }
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
            pins[0].X = x + WIDTH + GAP;
            pins[0].Y = y + HEIGHT / 2;
        }
    }
}
