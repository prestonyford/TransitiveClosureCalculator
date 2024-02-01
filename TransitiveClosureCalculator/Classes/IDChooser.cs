using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TransitiveClosureCalculator.User_Controls;

namespace TransitiveClosureCalculator.Classes {
    internal class IDChooser {
        private string LatestVertexID = "0";
        private SortedSet<int> RemovedVertexIDs = new SortedSet<int>(); // Int instead of string due to incorrect sorting order of strings

        public IDChooser() {
            
        }

        public string GetID() {
            // Determine unique ID of vertex
            if (RemovedVertexIDs.Count > 0) {
                string id = RemovedVertexIDs.First().ToString();
                RemovedVertexIDs.Remove(RemovedVertexIDs.First());
                return id;
            }
            else {
                string id = LatestVertexID;
                LatestVertexID = (Int32.Parse(LatestVertexID) + 1).ToString();
                return id;
            }
        }

        public void RemoveID(string id) {
            RemovedVertexIDs.Add(Int32.Parse(id));
        }
    }
}
