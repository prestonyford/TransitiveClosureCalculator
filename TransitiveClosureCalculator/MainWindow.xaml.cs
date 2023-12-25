using System.Collections;
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
        private SortedSet<string> RemovedVertexIDs = new SortedSet<string>();
        // private Dictionary<string, string> Edges = new Dictionary<string, string>();
        private bool Dragging = false;
        private Vertex? DraggedVertex = null;
        private Point DragStartPos = new Point(0, 0);
        private bool DrawingEdge = false;
        private Point DrawingEdgeStartPos = new Point(0, 0);
        private ArrowLine? prevFrameEdgeDraw = null;
        private Vertex? StartingVertexEdgeDraw = null;
        private Dictionary<Vertex, List<ArrowLine>> VertexConnectingEdges = new Dictionary<Vertex, List<ArrowLine>>();
        private Dictionary<ArrowLine, Tuple<Vertex, Vertex>> EdgesConnectingVertices = new Dictionary<ArrowLine, Tuple<Vertex, Vertex>>();
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
                vertex.ID = RemovedVertexIDs.First();
                RemovedVertexIDs.Remove(vertex.ID);
            }
            else {
                LatestVertexID = (Int32.Parse(LatestVertexID) + 1).ToString();
                vertex.ID = LatestVertexID;
            }

            Canvas.Children.Add(vertex);
            double vertexRadius = vertex.VertexWidth / 2;

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

            VertexConnectingEdges.Add(vertex, new List<ArrowLine>());
            AdjacencyList.Add(vertex, new List<Vertex>());
            ReverseAdjacencyList.Add(vertex, new List<Vertex>());

            vertex.MouseRightButtonDown += (object sender, MouseButtonEventArgs e) => {
                Canvas.Children.Remove(vertex);
                RemovedVertexIDs.Add(vertex.ID);
                foreach (ArrowLine edge in VertexConnectingEdges[vertex]) {
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

                if (DrawingEdge && StartingVertexEdgeDraw == vertex) {
                    DrawingEdge = false;
                    Canvas.Children.Remove(prevFrameEdgeDraw);
                    prevFrameEdgeDraw = null;
                }

                UpdateStartingMatrix();
                e.Handled = true;
            };
            vertex.MouseLeftButtonDown += (object sender, MouseButtonEventArgs e) => {
                if (!DrawingEdge) {
                    Dragging = true;
                }
                
                DraggedVertex = vertex;
                DragStartPos = e.GetPosition(Canvas);
            };
            vertex.MouseLeftButtonUp += (object sender, MouseButtonEventArgs e) => {
                if (Math.Abs(e.GetPosition(Canvas).X - DragStartPos.X) < 10 && Math.Abs(e.GetPosition(Canvas).Y - DragStartPos.Y) < 10) {
                    if (!DrawingEdge) {
                        Console.WriteLine("Begin drawing edge");
                        DrawingEdge = true;
                        prevFrameEdgeDraw = null;
                        DrawingEdgeStartPos = new Point(Canvas.GetLeft(vertex) + vertexRadius, Canvas.GetTop(vertex) + vertexRadius);
                        StartingVertexEdgeDraw = vertex;
                        Panel.SetZIndex(StartingVertexEdgeDraw, 2);
                    }
                    else {
                        Console.WriteLine("End drawing edge");
                        DrawingEdge = false;
                        if (prevFrameEdgeDraw != null && StartingVertexEdgeDraw != null) {
                            Canvas.Children.Remove(prevFrameEdgeDraw);
                            if (AdjacencyList[StartingVertexEdgeDraw].Contains(vertex)) {
                                return;
                            }
                            ArrowLine finishedArrowLine = DrawArrowLine(DrawingEdgeStartPos, new Point(Canvas.GetLeft(vertex) + vertexRadius, Canvas.GetTop(vertex) + vertexRadius));
                            finishedArrowLine.IsHitTestVisible = true;
                            EdgesConnectingVertices.Add(finishedArrowLine, new Tuple<Vertex, Vertex>(StartingVertexEdgeDraw, vertex));

                            // Right click to remove edge
                            finishedArrowLine.MouseRightButtonDown += (object sender, MouseButtonEventArgs e) => {
                                Canvas.Children.Remove(finishedArrowLine);
                                Vertex v1 = EdgesConnectingVertices[finishedArrowLine].Item1;
                                Vertex v2 = EdgesConnectingVertices[finishedArrowLine].Item2;
                                AdjacencyList[v1].Remove(v2);
                                AdjacencyList[v2].Remove(v1);
                                ReverseAdjacencyList[v1].Remove(v2);
                                ReverseAdjacencyList[v2].Remove(v1);
                                UpdateStartingMatrix();
                                e.Handled = true;
                            };

                            if (StartingVertexEdgeDraw == null) {
                                return;
                            }

                            VertexConnectingEdges[StartingVertexEdgeDraw].Add(finishedArrowLine);
                            VertexConnectingEdges[vertex].Add(finishedArrowLine);
                            AdjacencyList[StartingVertexEdgeDraw].Add(vertex);
                            ReverseAdjacencyList[vertex].Add(StartingVertexEdgeDraw);

                            Panel.SetZIndex(StartingVertexEdgeDraw, 0);
                            Panel.SetZIndex(finishedArrowLine, -1);
                            ShortenArrowLine(finishedArrowLine);

                            UpdateStartingMatrix();
                        }
                    }
                }
            };

            UpdateStartingMatrix();
            e.Handled = true;
        }

        private void Canvas_MouseRightButtonUp(object sender, MouseButtonEventArgs e) {
        }

        private void Canvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            if (Dragging) {
                Dragging = false;
            }
        }

        private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            
        }

        private void Canvas_MouseMove(object sender, MouseEventArgs e) {
            if (DraggedVertex == null) {
                return;
            }
            if (Dragging) {
                Point pos = e.GetPosition(Canvas);
                Canvas.SetLeft(DraggedVertex, pos.X - DraggedVertex.VertexWidth / 2);
                Canvas.SetTop(DraggedVertex, pos.Y - DraggedVertex.VertexHeight / 2);

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
                foreach (ArrowLine edge in VertexConnectingEdges[DraggedVertex]) {
                    Canvas.Children.Remove(edge);
                }
                VertexConnectingEdges[DraggedVertex].Clear();

                // Now rerender ArrowLines
                double vertexRadius = DraggedVertex.VertexWidth / 2;
                foreach (Vertex neighbor in AdjacencyList[DraggedVertex]) {
                    Point startPos = new Point(Canvas.GetLeft(neighbor) + vertexRadius, Canvas.GetTop(neighbor) + vertexRadius);
                    ArrowLine edge = DrawArrowLine(pos, startPos, -1);
                    VertexConnectingEdges[neighbor].Add(edge);
                    VertexConnectingEdges[DraggedVertex].Add(edge);
                    ShortenArrowLine(edge);
                }
                foreach (Vertex neighbor in ReverseAdjacencyList[DraggedVertex]) {
                    Point startPos = new Point(Canvas.GetLeft(neighbor) + vertexRadius, Canvas.GetTop(neighbor) + vertexRadius);
                    ArrowLine edge = DrawArrowLine(startPos, pos, -1);
                    VertexConnectingEdges[neighbor].Add(edge);
                    VertexConnectingEdges[DraggedVertex].Add(edge);
                    ShortenArrowLine(edge);
                }
            }
            else if (DrawingEdge) {
                if (prevFrameEdgeDraw != null) {
                    // Console.WriteLine("Removing prev frame");
                    Canvas.Children.Remove(prevFrameEdgeDraw);
                }

                ArrowLine edge = DrawArrowLine(DrawingEdgeStartPos, e.GetPosition(Canvas));
            }
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

        private void ShortenArrowLine(ArrowLine edge) {
            double angleRadians = Math.Atan2(edge.Y2 - edge.Y1, edge.X2 - edge.X1);
            double distance = Math.Sqrt(Math.Pow(edge.X2 - edge.X1, 2) + Math.Pow(edge.Y2 - edge.Y1, 2));
            double newDistance = Math.Max(0, distance - 33);
            edge.X2 = edge.X1 + newDistance * Math.Cos(angleRadians);
            edge.Y2 = edge.Y1 + newDistance * Math.Sin(angleRadians);
        }

        private void UpdateStartingMatrix() {
            var stringAdjacencyList = new Dictionary<string, HashSet<string>>();
            foreach (Vertex key in AdjacencyList.Keys) {
                stringAdjacencyList.Add(key.ID, AdjacencyList[key].Select(x => x.ID).ToHashSet());
            }

            Classes.Matrix matrix = new Classes.Matrix(stringAdjacencyList.Keys.ToList(), stringAdjacencyList);
            StartMatrixTextBox.Text = matrix.ToString();

            StartMatrixTextBlock.Text = $"Reflexive: {(matrix.IsReflexive? "Yes" : "No")}; Symmetric: {(matrix.IsSymmetric ? "Yes" : "No")}; Transitive: {(matrix.IsTransitive ? "Yes" : "No")}";
        }
    }
}