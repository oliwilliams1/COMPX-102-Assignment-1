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
            foreach (GateWithInfo gInfo in gateWithInfos)
            {
                foreach (Pin p in gInfo.gate.Pins)
                {
                    if (!p.IsInput || p.InputWire == null)
                        continue;

                    Gate fromGate = p.InputWire.FromPin.Owner;
                    foreach (var x in gateWithInfos)
                        if (x.gate == fromGate)
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
            // Create the clone component
            Compound clone = new Compound(left, top);

            // Map original to clone
            Dictionary<Gate, Gate> map = new Dictionary<Gate, Gate>();

            // Iterate through originals
            foreach (GateWithInfo gInfo in gateWithInfos)
            {
                // Clone each original gate and store reference into map for later
                Gate gClone = gInfo.gate.Clone();
                map[gInfo.gate] = gClone;

                // Add the clone gate into the clone compound with relative positioning data
                clone.AddGate(gClone, gInfo.dx, gInfo.dy);
            }

            // Rebuild wires on clone
            foreach (GateWithInfo gInfo in gateWithInfos)
            {
                // Get original and clone gate for readability
                Gate originalGate = gInfo.gate;
                Gate clonedGate = map[originalGate];

                // Iterate through all pins of original and copy structure to clone
                for (int i = 0; i < originalGate.Pins.Count; i++)
                {
                    Pin originalPin = originalGate.Pins[i];
                    if (!originalPin.IsInput || originalPin.InputWire == null)
                        continue;

                    // Get where this pin is from
                    Pin fromPin = originalPin.InputWire.FromPin;
                    Gate fromGate = fromPin.Owner;

                    // If fromGate is NOT within the original component, stop
                    if (!map.ContainsKey(fromGate))
                        continue;

                    // Else, find the clone counterpart
                    Gate clonedFromGate = map[fromGate];
                    int fromPinIndex = fromGate.Pins.IndexOf(fromPin);

                    Pin clonedFromPin = clonedFromGate.Pins[fromPinIndex];
                    Pin clonedToPin = clonedGate.Pins[i];
                    
                    // Wire up the clone counterpart
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
    }
}
