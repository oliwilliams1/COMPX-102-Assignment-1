using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Circuits
{
    /// <summary>
    /// This class implements a compound gate
    /// </summary>
    public class Compound : Gate
    {
        protected List<Gate> gates = new List<Gate>();

        /// <summary>
        /// Creates a compound gate given x, y
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public Compound(int x, int y)
        {
            left = x;
            top = y;
        }

        /// <summary>
        /// Adds a gate to the compound gate
        /// </summary>
        /// <param name="gate"></param>
        public void AddGate(Gate gate)
        {
            gates.Add(gate);
        }

        /// <summary>
        /// Moves the collection to x, y
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public override void MoveTo(int x, int y)
        {
            // Apply dy & dx to children
            int dx = x - left;
            int dy = y - top;

            left = x;
            top = y;

            foreach (Gate g in gates)
                g.MoveTo(g.Left + dx, g.Top + dy);
        }

        /// <summary>
        /// Draws the collection of gates
        /// </summary>
        /// <param name="paper"></param>
        public override void Draw(Graphics paper)
        {
            // Iterate through all child gates and renders them
            foreach (Gate g in gates)
                g.Draw(paper);
        }

        /// <summary>
        /// Overrides the selected property so all gates are selected
        /// </summary>
        public override bool Selected
        {
            get
            {
                if (gates.Count == 0) return false;

                foreach (Gate g in gates)
                    if (g.Selected == false)
                        return false;
                
                return true;
            }
            set
            {
                // Disperse this update to all child gates
                foreach (Gate g in gates)
                    g.Selected = value;
            }
        }

        /// <summary>
        /// This is never called, return false
        /// </summary>
        /// <returns></returns>
        public override bool Evaluate()
        {
            return false;
        }

        /// <summary>
        /// Clones the current gate
        /// </summary>
        /// <returns></returns>
        public override Gate Clone()
        {
            // Create a new compound clone
            Compound clone = new Compound(0, 0);

            // Adds all child gates to the clone
            foreach (Gate g in gates)
                clone.AddGate(g);

            // Return the final constructed clone
            return clone;
        }

        /// <summary>
        /// The compount component is hit if the click hits any child gates
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public override bool IsMouseOn(int x, int y)
        {
            foreach (Gate g in gates)
                if (g.IsMouseOn(x, y))
                    return true;

            return false;
        }
    }
}
