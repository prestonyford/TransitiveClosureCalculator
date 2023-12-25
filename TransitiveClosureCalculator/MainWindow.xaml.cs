using System.Security.Cryptography;
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
        private Line? prevFrameEdgeDraw = null;
        private Vertex? StartingVertexEdgeDraw = null;
        private Dictionary<Vertex, List<Line>> VertexConnectingEdges = new Dictionary<Vertex, List<Line>>();
        private Dictionary<Vertex, List<Vertex>> AdjacencyList = new Dictionary<Vertex, List<Vertex>>();
        private Dictionary<Vertex, List<Vertex>> UndirectedAdjacencyList = new Dictionary<Vertex, List<Vertex>>();

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

            VertexConnectingEdges.Add(vertex, new List<Line>());
            AdjacencyList.Add(vertex, new List<Vertex>());
            UndirectedAdjacencyList.Add(vertex, new List<Vertex>());

            vertex.MouseRightButtonDown += (object sender, MouseButtonEventArgs e) => {
                Canvas.Children.Remove(vertex);
                RemovedVertexIDs.Add(vertex.ID);
                foreach (Line edge in VertexConnectingEdges[vertex]) {
                    Canvas.Children.Remove(edge);
                }
                VertexConnectingEdges.Remove(vertex);
                AdjacencyList.Remove(vertex);
                UndirectedAdjacencyList.Remove(vertex);

                e.Handled = true;
            };
            vertex.MouseLeftButtonDown += (object sender, MouseButtonEventArgs e) => {
                Dragging = true;
                DraggedVertex = vertex;
                DragStartPos = e.GetPosition(Canvas);
            };
            vertex.MouseLeftButtonUp += (object sender, MouseButtonEventArgs e) => {
                Dragging = false;
                if (Math.Abs(e.GetPosition(Canvas).X - DragStartPos.X) < 10 && Math.Abs(e.GetPosition(Canvas).Y - DragStartPos.Y) < 10) {
                    if (!DrawingEdge) {
                        Console.WriteLine("Begin drawing edge");
                        DrawingEdge = true;
                        prevFrameEdgeDraw = null;
                        DrawingEdgeStartPos = new Point(Canvas.GetLeft(vertex) + vertexRadius, Canvas.GetTop(vertex) + vertexRadius);
                        StartingVertexEdgeDraw = vertex;
                    }
                    else {
                        Console.WriteLine("End drawing edge");
                        DrawingEdge = false;
                        if (prevFrameEdgeDraw != null) {
                            Canvas.Children.Remove(prevFrameEdgeDraw);
                            Line finishedLine = DrawLine(DrawingEdgeStartPos, new Point(Canvas.GetLeft(vertex) + vertexRadius, Canvas.GetTop(vertex) + vertexRadius));

                            if (StartingVertexEdgeDraw == null) {
                                return;
                            }

                            VertexConnectingEdges[StartingVertexEdgeDraw].Add(finishedLine);
                            VertexConnectingEdges[vertex].Add(finishedLine);
                            AdjacencyList[StartingVertexEdgeDraw].Add(vertex);
                            UndirectedAdjacencyList[StartingVertexEdgeDraw].Add(vertex);
                            UndirectedAdjacencyList[vertex].Add(StartingVertexEdgeDraw);
                        }
                    }
                }
            };

            e.Handled = true;
        }

        private void Canvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {

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

                // Removing existing lines
                foreach (Line edge in VertexConnectingEdges[DraggedVertex]) {
                    Canvas.Children.Remove(edge);
                }
                VertexConnectingEdges[DraggedVertex].Clear();

                // Now rerender lines
                double vertexRadius = DraggedVertex.VertexWidth / 2;
                foreach (Vertex neighbor in UndirectedAdjacencyList[DraggedVertex]) {
                    Point startPos = new Point(Canvas.GetLeft(neighbor) + vertexRadius, Canvas.GetTop(neighbor) + vertexRadius);
                    Line edge = DrawLine(startPos, pos);
                    
                    VertexConnectingEdges[neighbor].Add(edge);
                    VertexConnectingEdges[DraggedVertex].Add(edge);
                }
            }
            else if (DrawingEdge) {
                if (prevFrameEdgeDraw != null) {
                    // Console.WriteLine("Removing prev frame");
                    Canvas.Children.Remove(prevFrameEdgeDraw);
                }

                Line edge = DrawLine(DrawingEdgeStartPos, e.GetPosition(Canvas));
            }
        }

        // Helpers

        private Line DrawLine(Point start, Point end) {
            Line edge = new Line();
            edge.IsHitTestVisible = false;
            edge.Stroke = Brushes.Black;
            edge.StrokeThickness = 2;
            Panel.SetZIndex(edge, 2);
            edge.X1 = start.X;
            edge.Y1 = start.Y;
            edge.X2 = end.X;
            edge.Y2 = end.Y;
            Canvas.Children.Add(edge);
            prevFrameEdgeDraw = edge;
            return edge;
        }
    }
}