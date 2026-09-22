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
    /// This class implements an output lamp
    /// </summary>
    public class OutputLamp : Gate
    {
        // Variables
        protected bool _value;  // true = high voltage, false = low voltage

        /// <summary>
        /// Initialises the Gate.
        /// </summary>
        /// <param name="x">The x position of the gate</param>
        /// <param name="y">The y position of the gate</param>
        public OutputLamp(int x, int y)
        {
            //Add the input pin to the gate
            pins.Add(new Pin(this, true, 20));

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
            paper.FillEllipse(brush, left, top, WIDTH, HEIGHT);

            // Select colour for border based on selection state
            Color color;
            if (selected)
                color = Color.Red;
            else
                color = Color.Black;

            using (Pen pen = new Pen(color, BORDER))
            {
                // Draw the border
                paper.DrawEllipse(
                    pen,
                    left + BORDER / 2,
                    top + BORDER / 2,
                    WIDTH - BORDER,
                    HEIGHT - BORDER
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
            pins[0].X = x - GAP;
            pins[0].Y = y + HEIGHT / 2;
        }

        /// <summary>
        /// Evaluates input and updates internal state
        /// </summary>
        /// <returns></returns>
        public override bool Evaluate()
        {
            if (pins[0].InputWire == null) return false;

            // Get input gate
            Gate input = pins[0].InputWire.FromPin.Owner;
            
            // Update internal state the the result of the input gate and return this state
            _value = input.Evaluate();
            return _value;
        }

        /// <summary>
        /// Returns a clone of this gate
        /// </summary>
        /// <returns></returns>
        public override Gate Clone()
        {
            Gate newGate = new OutputLamp(0, 0);
            return newGate;
        }
    }
}
