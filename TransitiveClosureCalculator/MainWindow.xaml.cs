﻿using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TransitiveClosureCalculator.User_Controls;

namespace TransitiveClosureCalculator {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        private string LatestVertexID = "0";
        private SortedSet<int> RemovedVertexIDs = new SortedSet<int>(); // Int instead of string due to incorrect sorting order of strings
        // private Dictionary<string, string> Edges = new Dictionary<string, string>();
        private bool UserIsDraggingVertex = false;
        private Vertex? DraggedVertex = null;
        private Point DragStartPos = new Point(0, 0);
        private bool UserIsDrawingEdge = false;
        private Point DrawingEdgeStartPos = new Point(0, 0);
        private ArrowLine? prevFrameEdgeDraw = null;
        private Vertex? StartingVertexEdgeDraw = null;
        private Dictionary<Vertex, List<UserControl>> VertexConnectingEdges = new Dictionary<Vertex, List<UserControl>>();
        private Dictionary<UserControl, Tuple<Vertex, Vertex>> EdgesConnectingVertices = new Dictionary<UserControl, Tuple<Vertex, Vertex>>();
        private Dictionary<Vertex, List<Vertex>> AdjacencyList = new Dictionary<Vertex, List<Vertex>>();
        private Dictionary<Vertex, List<Vertex>> ReverseAdjacencyList = new Dictionary<Vertex, List<Vertex>>();

        public MainWindow() {
            InitializeComponent();
        }

        private void Canvas_MouseRightButtonDown(object sender, MouseButtonEventArgs e) {
            var clickPos = e.GetPosition(Canvas);
            var vertex = new Vertex();

            // Determine unique ID of vertex
            if (RemovedVertexIDs.Count > 0) {
                vertex.ID = RemovedVertexIDs.First().ToString();
                RemovedVertexIDs.Remove(RemovedVertexIDs.First());
            }
            else {
                LatestVertexID = (Int32.Parse(LatestVertexID) + 1).ToString();
                vertex.ID = LatestVertexID;
            }

            Canvas.Children.Add(vertex);
            double vertexRadius = vertex.VertexWidth / 2;

            // Correct positioning if out of bounds
            if (clickPos.X < vertexRadius) {
                clickPos.X = vertexRadius;
            }
            else if (clickPos.X > Canvas.ActualWidth - vertexRadius) {
                clickPos.X = Canvas.ActualWidth - vertexRadius;
            }
            if (clickPos.Y < vertexRadius) {
                clickPos.Y = vertexRadius;
            }
            else if (clickPos.Y > Canvas.ActualHeight - vertexRadius) {
                clickPos.Y = Canvas.ActualHeight - vertexRadius;
            }

            double newXPos = clickPos.X - vertex.VertexWidth / 2;
            double newYPos = clickPos.Y - vertex.VertexHeight / 2;
            Canvas.SetLeft(vertex, newXPos);
            Canvas.SetTop(vertex, newYPos);

            VertexConnectingEdges.Add(vertex, new List<UserControl>());
            AdjacencyList.Add(vertex, new List<Vertex>());
            ReverseAdjacencyList.Add(vertex, new List<Vertex>());

            // Right click vertex to remove it
            vertex.MouseRightButtonDown += (object sender, MouseButtonEventArgs e) => {
                if (UserIsDrawingEdge && vertex == StartingVertexEdgeDraw || UserIsDraggingVertex) {
                    e.Handled = true;
                    return;
                }
                Canvas.Children.Remove(vertex);
                RemovedVertexIDs.Add(Int32.Parse(vertex.ID));
                foreach (UserControl edge in VertexConnectingEdges[vertex]) {
                    Canvas.Children.Remove(edge);
                }

                VertexConnectingEdges.Remove(vertex);
                foreach (Vertex neighbor in AdjacencyList[vertex]) {
                    ReverseAdjacencyList[neighbor].Remove(vertex);
                }
                foreach (Vertex neighbor in ReverseAdjacencyList[vertex]) {
                    AdjacencyList[neighbor].Remove(vertex);
                }
                AdjacencyList.Remove(vertex);
                ReverseAdjacencyList.Remove(vertex);

                if (UserIsDrawingEdge && StartingVertexEdgeDraw == vertex) {
                    UserIsDrawingEdge = false;
                    Canvas.Children.Remove(prevFrameEdgeDraw);
                    prevFrameEdgeDraw = null;
                }

                UpdateStartingMatrix();
                e.Handled = true;
            };

            // Save position so that upon mouse up, it can be determined it it was a click or a drag
            vertex.MouseLeftButtonDown += (object sender, MouseButtonEventArgs e) => {
                if (!UserIsDrawingEdge) {
                    UserIsDraggingVertex = true;
                }
                DraggedVertex = vertex;
                DragStartPos = e.GetPosition(Canvas);
            };

            vertex.MouseLeftButtonUp += (object sender, MouseButtonEventArgs e) => {
                // Left click another vertex while drawing edge to finish edge
                if (Math.Abs(e.GetPosition(Canvas).X - DragStartPos.X) < 10 && Math.Abs(e.GetPosition(Canvas).Y - DragStartPos.Y) < 10) {
                    if (!UserIsDrawingEdge) {
                        Console.WriteLine("Begin drawing edge");
                        UserIsDrawingEdge = true;
                        prevFrameEdgeDraw = null;
                        DrawingEdgeStartPos = GetVertexCenterPoint(vertex);
                        StartingVertexEdgeDraw = vertex;
                        Panel.SetZIndex(StartingVertexEdgeDraw, 2);
                    }
                    else {
                        Console.WriteLine("End drawing edge");
                        UserIsDrawingEdge = false;
                        if (prevFrameEdgeDraw != null && StartingVertexEdgeDraw != null) {
                            Canvas.Children.Remove(prevFrameEdgeDraw);
                            if (AdjacencyList[StartingVertexEdgeDraw].Contains(vertex)) {
                                return;
                            }

                            UserControl? edge = null;
                            if (StartingVertexEdgeDraw == vertex) { // Self loop
                                edge = DrawSelfLoop(vertex);
                            }
                            else { // Edge to another vertex
                                ArrowLine finishedArrowLine = DrawArrowLine(DrawingEdgeStartPos, GetVertexCenterPoint(vertex));
                                ShortenArrowLine(finishedArrowLine);
                                edge = finishedArrowLine;
                            }

                            SubscribeEdgeToRightClick(edge);

                            if (StartingVertexEdgeDraw == null) {
                                return;
                            }

                            VertexConnectingEdges[StartingVertexEdgeDraw].Add(edge);
                            VertexConnectingEdges[vertex].Add(edge);
                            EdgesConnectingVertices.Add(edge, new Tuple<Vertex, Vertex>(StartingVertexEdgeDraw, vertex));
                            AdjacencyList[StartingVertexEdgeDraw].Add(vertex);
                            ReverseAdjacencyList[vertex].Add(StartingVertexEdgeDraw);

                            Panel.SetZIndex(StartingVertexEdgeDraw, 0);
                            Panel.SetZIndex(edge, -1);

                            UpdateStartingMatrix();
                        }
                    }
                }
                else if (!UserIsDrawingEdge) { // Mouse is released after moving a vertex
                    // Resubscribe edges right-click handlers after they are redrawn after moving a vertex
                    foreach (UserControl edge in VertexConnectingEdges[vertex]) {
                        SubscribeEdgeToRightClick(edge);
                    }
                }
            };

            UpdateStartingMatrix();
            e.Handled = true;
        }

        private void Canvas_MouseRightButtonUp(object sender, MouseButtonEventArgs e) {

        }

        private void Canvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            if (UserIsDraggingVertex) {
                UserIsDraggingVertex = false;
            }
        }

        private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            
        }

        private void Canvas_MouseMove(object sender, MouseEventArgs e) {
            if (DraggedVertex == null) {
                return;
            }
            if (UserIsDraggingVertex) { // Not super CPU friendly to move/drag a vertex if it has lots of edges
                Point pos = e.GetPosition(Canvas);
                Canvas.SetLeft(DraggedVertex, pos.X - DraggedVertex.VertexWidth / 2);
                Canvas.SetTop(DraggedVertex, pos.Y - DraggedVertex.VertexHeight / 2);

                // Prevent user from dragging out of bounds
                if (Canvas.GetTop(DraggedVertex) < 0) {
                    Canvas.SetTop(DraggedVertex, 0);
                }
                else if (Canvas.GetTop(DraggedVertex) > Canvas.ActualHeight - DraggedVertex.VertexHeight) {
                    Canvas.SetTop(DraggedVertex, Canvas.ActualHeight - DraggedVertex.VertexHeight);
                }
                if (Canvas.GetLeft(DraggedVertex) < 0) {
                    Canvas.SetLeft(DraggedVertex, 0);
                }
                else if (Canvas.GetLeft(DraggedVertex) > Canvas.ActualWidth - DraggedVertex.VertexWidth) {
                    Canvas.SetLeft(DraggedVertex, Canvas.ActualWidth - DraggedVertex.VertexWidth);
                }

                // Removing existing ArrowLines
                foreach (UserControl edge in VertexConnectingEdges[DraggedVertex]) {
                    Canvas.Children.Remove(edge);
                    EdgesConnectingVertices.Remove(edge);
                }
                VertexConnectingEdges[DraggedVertex].Clear();

                // Now redraw ArrowLines
                double vertexRadius = DraggedVertex.VertexWidth / 2;
                foreach (Vertex neighbor in AdjacencyList[DraggedVertex]) {
                    if (DraggedVertex == neighbor) { // Self loop
                        SelfLoop selfLoop = DrawSelfLoop(DraggedVertex);
                        VertexConnectingEdges[neighbor].Add(selfLoop);
                        EdgesConnectingVertices.Add(selfLoop, new Tuple<Vertex, Vertex>(DraggedVertex, DraggedVertex));
                    }
                    else { // Edge to another vertex
                        Point endPos = new Point(Canvas.GetLeft(neighbor) + vertexRadius, Canvas.GetTop(neighbor) + vertexRadius);
                        ArrowLine edge = DrawArrowLine(pos, endPos, -1);
                        VertexConnectingEdges[neighbor].Add(edge);
                        VertexConnectingEdges[DraggedVertex].Add(edge);
                        EdgesConnectingVertices.Add(edge, new Tuple<Vertex, Vertex>(DraggedVertex, neighbor));
                        ShortenArrowLine(edge);
                    }
                }
                foreach (Vertex neighbor in ReverseAdjacencyList[DraggedVertex]) {
                    if (DraggedVertex != neighbor) { // Edge to another vertex (self loops already handled in forward list)
                        Point startPos = new Point(Canvas.GetLeft(neighbor) + vertexRadius, Canvas.GetTop(neighbor) + vertexRadius);
                        ArrowLine edge = DrawArrowLine(startPos, pos, -1);
                        VertexConnectingEdges[neighbor].Add(edge);
                        VertexConnectingEdges[DraggedVertex].Add(edge);
                        EdgesConnectingVertices.Add(edge, new Tuple<Vertex, Vertex>(neighbor, DraggedVertex));
                        ShortenArrowLine(edge);
                    }
                }
            }
            else if (UserIsDrawingEdge) {
                if (prevFrameEdgeDraw != null) {
                    // Console.WriteLine("Removing prev frame");
                    Canvas.Children.Remove(prevFrameEdgeDraw);
                }

                ArrowLine edge = DrawArrowLine(DrawingEdgeStartPos, e.GetPosition(Canvas));
            }
        }

        private void Calculate_Button_Click(object sender, RoutedEventArgs e) {
            UpdateResultingMatrix();
        }

        // Helpers

        private ArrowLine DrawArrowLine(Point start, Point end, int zIndex = 1) {
            ArrowLine edge = new ArrowLine();
            Panel.SetZIndex(edge, zIndex);
            edge.X1 = start.X;
            edge.Y1 = start.Y;
            edge.X2 = end.X;
            edge.Y2 = end.Y;
            Canvas.Children.Add(edge);
            prevFrameEdgeDraw = edge;

            double angleRadians = Math.Atan2(end.Y - start.Y, end.X - start.X);
            double angleDegrees = angleRadians * (180 / Math.PI);
            edge.Angle = angleDegrees;
            return edge;
        }

        private SelfLoop DrawSelfLoop(Vertex vertex) {
            SelfLoop selfLoop = new SelfLoop();
            Panel.SetZIndex(selfLoop, 2);
            Canvas.SetLeft(selfLoop, Canvas.GetLeft(vertex) + vertex.ActualWidth / 2);
            Canvas.SetTop(selfLoop, Canvas.GetTop(vertex));
            Canvas.Children.Add(selfLoop);
            return selfLoop;
        }

        private void ShortenArrowLine(ArrowLine edge) {
            double angleRadians = Math.Atan2(edge.Y2 - edge.Y1, edge.X2 - edge.X1);
            double distance = Math.Sqrt(Math.Pow(edge.X2 - edge.X1, 2) + Math.Pow(edge.Y2 - edge.Y1, 2));
            double newDistance = Math.Max(0, distance - 33);
            edge.X2 = edge.X1 + newDistance * Math.Cos(angleRadians);
            edge.Y2 = edge.Y1 + newDistance * Math.Sin(angleRadians);
        }

        private Point GetVertexCenterPoint(Vertex vertex) {
            double vertexRadius = vertex.VertexHeight / 2;
            return new Point(Canvas.GetLeft(vertex) + vertexRadius, Canvas.GetTop(vertex) + vertexRadius);
        }

        private void SubscribeEdgeToRightClick(UserControl edge) {
            edge.IsHitTestVisible = true;
            edge.MouseRightButtonDown += (object sender, MouseButtonEventArgs e) => {
                Canvas.Children.Remove(edge);
                Vertex from = EdgesConnectingVertices[edge].Item1;
                Vertex to = EdgesConnectingVertices[edge].Item2;
                foreach (Vertex v in AdjacencyList[from].Concat(ReverseAdjacencyList[to])) {
                    VertexConnectingEdges[v].Remove(edge);
                    EdgesConnectingVertices.Remove(edge);
                }
                AdjacencyList[from].Remove(to);
                ReverseAdjacencyList[to].Remove(from);
                UpdateStartingMatrix();
                e.Handled = true;
            };
        }

        private void UpdateStartingMatrix() {
            var stringAdjacencyList = new Dictionary<string, HashSet<string>>();
            foreach (Vertex key in AdjacencyList.Keys) {
                stringAdjacencyList.Add(key.ID, AdjacencyList[key].Select(x => x.ID).ToHashSet());
            }

            Classes.Matrix matrix = new Classes.Matrix(stringAdjacencyList.Keys.ToList(), stringAdjacencyList);

            StartMatrixTextBox.Text = matrix.ToString();
            StartMatrixTextBlock.Text = $"Reflexive: {(matrix.IsReflexive ? "Yes" : "No")}; Symmetric: {(matrix.IsSymmetric ? "Yes" : "No")}; Transitive: {(matrix.IsTransitive ? "Yes" : "No")}";
        }

        private void UpdateResultingMatrix() {
            var stringAdjacencyList = new Dictionary<string, HashSet<string>>();
            foreach (Vertex key in AdjacencyList.Keys) {
                stringAdjacencyList.Add(key.ID, AdjacencyList[key].Select(x => x.ID).ToHashSet());
            }

            Classes.Matrix matrix = new Classes.Matrix(stringAdjacencyList.Keys.ToList(), stringAdjacencyList);

            if (ReflexiveCheckBox.IsChecked == true) {
                matrix.MakeReflexive();
            }
            if (SymmetricCheckBox.IsChecked == true) {
                matrix.MakeSymmetric();
            }
            if (TransitiveCheckBox.IsChecked == true) {
                matrix.MakeTransitive();
            }

            ResultMatrixTextBox.Text = matrix.ToString();
            List<string> addedEdgesStrings = new List<string>(matrix.AddedEdges.Count);
            matrix.AddedEdges.Sort((x, y) => Int32.Parse(x.Item2).CompareTo(Int32.Parse(y.Item2)));
            // Stable sort
            matrix.AddedEdges = matrix.AddedEdges.OrderBy(x => x.Item1).ToList();

            foreach (Tuple<string, string> pair in matrix.AddedEdges) {
                addedEdgesStrings.Add($"({pair.Item1}, {pair.Item2})");
            }
            ResultMatrixAddedEdges.Text = $"Added edges: {string.Join(", ", addedEdgesStrings)}";
            ResultMatrixTextBlock.Text = $"Reflexive: {(matrix.IsReflexive ? "Yes" : "No")}; Symmetric: {(matrix.IsSymmetric ? "Yes" : "No")}; Transitive: {(matrix.IsTransitive ? "Yes" : "No")}";
        }
    }
}