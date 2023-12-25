using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TransitiveClosureCalculator.User_Controls;

namespace TransitiveClosureCalculator.Classes {
    internal class Matrix {
        private List<string> Vertices;
        private Dictionary<string, HashSet<string>> AdjacencyList;
        public List<Tuple<string, string>> AddedEdges = new List<Tuple<string, string>>();

        public Matrix(List<string> vertices, Dictionary<string, HashSet<string>> adjacencyList) {
            vertices.Sort((x, y) => Int32.Parse(x).CompareTo(Int32.Parse(y)));
            Vertices = vertices;
            AdjacencyList = adjacencyList;
        }

        public override string ToString() {
            string res = " ";
            foreach (string vertex in Vertices) {
                res += $" {((Int32.Parse(Vertices.Last()) >= 10 && Int32.Parse(vertex) < 10) ? " " : "")}{vertex}";
            }
            foreach (string vertex in Vertices) {
                res += $"\n{vertex} {((Int32.Parse(Vertices.Last()) >= 10 && Int32.Parse(vertex) < 10) ? " " : "")}";
                foreach (string vertex2 in Vertices) {
                    res += $"{(AdjacencyList[vertex].Contains(vertex2) ? "1" : "0")} {((Int32.Parse(Vertices.Last()) >= 10) ? " " : "")}";
                }
            }
            return res;
        }

        public void MakeReflexive() {
            foreach (string vertex in Vertices) {
                if (!AdjacencyList[vertex].Contains(vertex)) {
                    AdjacencyList[vertex].Add(vertex);
                    AddedEdges.Add(new Tuple<string, string>(vertex, vertex));
                }
            }
        }
        public void MakeSymmetric() {
            foreach (string vertex in Vertices) {
                foreach (string neighbor in AdjacencyList[vertex]) {
                    if (!AdjacencyList[neighbor].Contains(vertex)) {
                        AdjacencyList[neighbor].Add(vertex);
                        AddedEdges.Add(new Tuple<string, string>(neighbor, vertex));
                    }
                }
            }
        }
        public void MakeTransitive() {
            // Warshall's Algorithm
            foreach (string k in Vertices) {
                foreach (string i in Vertices) {
                    foreach (string j in Vertices) {
                        if (AdjacencyList[i].Contains(k) && AdjacencyList[k].Contains(j)) {
                            if (!AdjacencyList[i].Contains(j)) {
                                AdjacencyList[i].Add(j);
                                AddedEdges.Add(new Tuple<string, string>(i, j));
                            }
                        }
                    }
                }
            }
        }

        public bool IsReflexive {
            // All nodes have a path to themselves (diagonal 1s)
            get {
                foreach (string vertex in Vertices) {
                    if (!AdjacencyList[vertex].Contains(vertex)) {
                        return false;
                    }
                }
                return true;
            }
        }
        public bool IsSymmetric {
            // Edges must go in both directions
            get {
                foreach (string vertex in Vertices) {
                    foreach (string neighbor in AdjacencyList[vertex]) {
                        if (!AdjacencyList[neighbor].Contains(vertex)) {
                            return false;
                        }
                    }
                }
                return true;
            }
        }
        public bool IsTransitive {
            // For all two step paths there is a one step path
            get {
                // Warshall's Algorithm
                foreach (string k in Vertices) {
                    foreach (string i in Vertices) {
                        foreach (string j in Vertices) {
                            if (AdjacencyList[i].Contains(k) && AdjacencyList[k].Contains(j)) {
                                if (!AdjacencyList[i].Contains(j)) {
                                    Console.WriteLine($"Does not satisfy ({i}, {j})");
                                    return false;
                                }
                            }
                        }
                    }
                }
                return true;
            }
        }
    }
}
