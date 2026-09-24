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
        /// <summary>
        /// Used to store the relative offsets with the gate to preserve spacings
        /// </summary>
        protected struct GateWithInfo
        {
            /// <summary>
            /// Construct this isntance with gate, rel x, and rel y
            /// </summary>
            /// <param name="g"></param>
            /// <param name="dx"></param>
            /// <param name="dy"></param>
            public GateWithInfo(Gate g, int dx, int dy)
            {
                this.gate = g;
                this.dx = dx;
                this.dy = dy;
            }
            // Member variables
            public Gate gate;
            public int dx;
            public int dy;
        }
        
        // List of gates in this context
        protected List<GateWithInfo> gateWithInfos = new List<GateWithInfo>();

        /// <summary>
        /// Exposes the gates to the root context
        /// </summary>
        public List<Gate> Gates
        {
            get
            {
                // Expose the gates
                List<Gate> gates = new List<Gate>();
                foreach (GateWithInfo gInfo in gateWithInfos)
                    gates.Add(gInfo.gate);

                return gates;
            }
        }

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
            // Get rel x and rel y
            int dx = gate.Left - left;
            int dy = gate.Top - top;

            // Package gate with rel positioning
            GateWithInfo gInfo = new GateWithInfo(gate, dx, dy);
            gateWithInfos.Add(gInfo);
        }

        /// <summary>
        /// Adds a gate with specified relative coordinates
        /// </summary>
        /// <param name="gate"></param>
        /// <param name="dx"></param>
        /// <param name="dy"></param>
        public void AddGate(Gate gate, int dx, int dy)
        {
            // Sets the coords
            gate.MoveTo(left + dx, top + dy);

            // Package gate with rel positioning
            GateWithInfo gInfo = new GateWithInfo(gate, dx, dy);
            gateWithInfos.Add(gInfo);
        }

        /// <summary>
        /// Moves the collection to x, y
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public override void MoveTo(int x, int y)
        {
            // Update compound pos
            left = x;
            top = y;

            // Moves to rel coords
            foreach (GateWithInfo gInfo in gateWithInfos)
                gInfo.gate.MoveTo(left + gInfo.dx, top + gInfo.dy);
        }

        /// <summary>
        /// Draws the collection of gates
        /// </summary>
        /// <param name="paper"></param>
        public override void Draw(Graphics paper)
        {
            // Iterate through all child gates and renders them
            foreach (GateWithInfo gInfo in gateWithInfos)
                gInfo.gate.Draw(paper);

            // Draw wires
            List<Gate> leaves = GetLeafGates();
            foreach (Gate leaf in leaves)
            {
                foreach (Pin p in leaf.Pins)
                {
                    if (p.IsInput && p.InputWire != null
                        && leaves.Contains(p.InputWire.FromPin.Owner))
                        p.InputWire.Draw(paper);
                }
            }
        }

        /// <summary>
        /// Expose the compounds pins
        /// </summary>
        public override List<Pin> Pins
        {
            get
            {
                // Expose the pins
                List<Pin> allPins = new List<Pin>();
                foreach (GateWithInfo gInfo in gateWithInfos)
                    allPins.AddRange(gInfo.gate.Pins);

                return allPins;
            }
        }

        /// <summary>
        /// Overrides the selected property so all gates are selected
        /// </summary>
        public override bool Selected
        {
            get
            {
                if (gateWithInfos.Count == 0) return false;

                // if any are not selected, the component is deemed unselected
                foreach (GateWithInfo gInfo in gateWithInfos)
                    if (gInfo.gate.Selected == false)
                        return false;
                
                // otherwise, we are selected
                return true;
            }
            set
            {
                // Disperse this update to all child gates
                foreach (GateWithInfo gInfo in gateWithInfos)
                    gInfo.gate.Selected = value;
            }
        }

        /// <summary>
        /// Same thing as in form1
        /// </summary>
        /// <returns></returns>
        public override bool Evaluate()
        {
            // Eval the children of this
            foreach (GateWithInfo gInfo in gateWithInfos)
                if (gInfo.gate is OutputLamp || gInfo.gate is Compound)
                    gInfo.gate.Evaluate();

            return false; // collection of multiple states
        }

        /// <summary>
        /// Clones the current gate
        /// </summary>
        /// <returns></returns>
        public override Gate Clone()
        {
            // Create a clone
            Compound clone = new Compound(left, top);

            // Clone children into clone
            foreach (GateWithInfo gInfo in gateWithInfos)
                clone.AddGate(gInfo.gate.Clone(), gInfo.dx, gInfo.dy);

            List<Gate> originalGates = GetLeafGates();
            List<Gate> cloneGates = clone.GetLeafGates();

            // Map original to clone
            Dictionary<Gate, Gate> map = new Dictionary<Gate, Gate>();
            for (int i = 0; i < originalGates.Count; i++)
                map[originalGates[i]] = cloneGates[i];

            // Rebuild every wire connection
            foreach (Gate originalGate in originalGates)
            {
                // Get associative clone
                Gate clonedGate = map[originalGate];

                // Iterate through the pins of the original
                for (int i = 0; i < originalGate.Pins.Count; i++)
                {
                    // Skip if not an input pin or has no wire to rebuild
                    Pin originalPin = originalGate.Pins[i];
                    if (!originalPin.IsInput || originalPin.InputWire == null)
                        continue;

                    // Travel the wire to get the owning pin and gate
                    Pin fromPin = originalPin.InputWire.FromPin;
                    Gate fromGate = fromPin.Owner;

                    // Wire comes from outside, so leave it unconnected
                    if (!map.ContainsKey(fromGate))
                        continue;

                    // Get the associate pin on clone
                    Pin clonedToPin = clonedGate.Pins[i];

                    // If the wire exists, skip
                    if (clonedToPin.InputWire != null)
                        continue;

                    // Create the wire on the clone
                    Pin clonedFromPin = map[fromGate].Pins[fromGate.Pins.IndexOf(fromPin)];
                    clonedToPin.InputWire = new Wire(clonedFromPin, clonedToPin);
                }
            }

            // Return the finished clone
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
            foreach (GateWithInfo gInfo in gateWithInfos)
                if (gInfo.gate.IsMouseOn(x, y))
                    return true;

            return false;
        }

        /// <summary>
        /// Pass the OnMouseClick to children
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public override void OnMouseClick(int x, int y)
        {
            foreach (GateWithInfo gInfo in gateWithInfos)
                if (gInfo.gate.IsMouseOn(x, y))
                    gInfo.gate.OnMouseClick(x, y);
        }

        public List<Gate> GetLeafGates()
        {
            // Flat list of leaf nodes
            List<Gate> leaves = new List<Gate>();
            
            // Iterate through gates
            foreach (GateWithInfo gInfo in gateWithInfos)
            {
                // If we are a compound, call recursively on child until reach leads, else add self
                Gate nested = gInfo.gate;
                if (nested is Compound)
                {
                    Compound nestedCompound = nested as Compound;
                    leaves.AddRange(nestedCompound.GetLeafGates());
                }
                else
                {
                    leaves.Add(gInfo.gate);
                }
            }

            // Return flattened list
            return leaves;
        }

        /// <summary>
        /// Sets this potion to the top-left corner of its children and updates childrens rel positions
        /// </summary>
        public void FixRelativePos()
        {
            if (gateWithInfos.Count == 0)
                return;

            // Find the top-left corner of the bounding box of all children
            int minLeft = int.MaxValue;
            int minTop = int.MaxValue;
            foreach (GateWithInfo gInfo in gateWithInfos)
            {
                minLeft = Math.Min(minLeft, gInfo.gate.Left);
                minTop = Math.Min(minTop, gInfo.gate.Top);
            }

            // Position at this point
            left = minLeft;
            top = minTop;

            // Update the data
            for (int i = 0; i < gateWithInfos.Count; i++)
            {
                Gate g = gateWithInfos[i].gate;
                // Update the data
                gateWithInfos[i] = new GateWithInfo(g, g.Left - left, g.Top - top);
            }

        }
    }
}
