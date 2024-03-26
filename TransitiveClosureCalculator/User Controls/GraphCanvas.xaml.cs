using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TransitiveClosureCalculator.Classes;
using TransitiveClosureCalculator.User_Controls.Edges;
using Point = System.Windows.Point;

namespace TransitiveClosureCalculator.User_Controls {
    /// <summary>
    /// Interaction logic for GraphCanvas.xaml
    /// </summary>
    public partial class GraphCanvas : UserControl {
        private IDChooser IDChooser = new IDChooser();

        // Creation of controls
        private bool UserIsDraggingVertex = false;
        private Vertex? DraggedVertex = null;
        private Point DragStartPos = new Point(0, 0);
        private bool UserIsDrawingEdge = false;
        private Edge? DrawnEdge = null;
        private Vertex? StartingVertexEdgeDraw = null;

        // Management of controls in dictionaries

        // Key: Vertex; Value: Edges connected to it
        private Dictionary<Vertex, List<Edge>> VertexConnectingEdges = new Dictionary<Vertex, List<Edge>>();
        // Key: Edge; Value: Ordered tuple, first element is from Vertex, second is to Vertex
        private Dictionary<Edge, Tuple<Vertex, Vertex>> EdgesConnectingVertices = new Dictionary<Edge, Tuple<Vertex, Vertex>>();
        private Dictionary<Vertex, List<Vertex>> AdjacencyList = new Dictionary<Vertex, List<Vertex>>();
        private Dictionary<Vertex, List<Vertex>> ReverseAdjacencyList = new Dictionary<Vertex, List<Vertex>>();

        public GraphCanvas() {
            InitializeComponent();
        }

        private Point CorrectPoint(Point pos) {
            // Correct positioning if out of bounds
            double vertexRadius = Vertex.Diameter / 2;
            Point res = new Point(pos.X, pos.Y);
            if (pos.X < vertexRadius) {
                res.X = vertexRadius;
            }
            else if (pos.X > Canvas.ActualWidth - vertexRadius) {
                res.X = Canvas.ActualWidth - vertexRadius;
            }
            if (pos.Y < vertexRadius) {
                res.Y = vertexRadius;
            }
            else if (pos.Y > Canvas.ActualHeight - vertexRadius) {
                res.Y = Canvas.ActualHeight - vertexRadius;
            }
            return res;
        }
        private Point GetVertexCoords(Vertex vertex) {
            return new Point(Canvas.GetLeft(vertex), Canvas.GetTop(vertex));
        }

        private void AddControl(UserControl control, Point pos) {
            Point point = CorrectPoint(pos);

            Canvas.Children.Add(control);
            Canvas.SetLeft(control, point.X - Vertex.Diameter/2);
            Canvas.SetTop(control, point.Y - Vertex.Diameter/2);
        }

        private void RemoveControls(IEnumerable<UserControl> controls) {
            foreach (UserControl control in controls) {
                Canvas.Children.Remove(control);
            }
            // TODO: FIRE EVENT TO UPDATE STARTING MATRIX
        }

        private void MoveVertex(Vertex vertex, Point pos) {
            // Move vertex if dragging one
            Point correctedPoint = CorrectPoint(pos);
            Canvas.SetLeft(DraggedVertex, correctedPoint.X - Vertex.Diameter / 2);
            Canvas.SetTop(DraggedVertex, correctedPoint.Y - Vertex.Diameter / 2);

            // Move its connecting edges
            foreach (Edge edge in VertexConnectingEdges[vertex]) {
                Tuple<Vertex, Vertex> pair = EdgesConnectingVertices[edge];
                Vertex start = pair.Item1;
                Vertex end = pair.Item2;

                if (ReferenceEquals(vertex, start)) {
                    /*edge.X1*/
                }
            }
        }

        private void VertexRightClick(object sender, MouseButtonEventArgs e) {
            Vertex vertex = (Vertex)sender;

            if (UserIsDrawingEdge && vertex == StartingVertexEdgeDraw || UserIsDraggingVertex) {
                e.Handled = true;
                return;
            }
            IDChooser.RemoveID(vertex.ID);

            // Remove the vertex and all its connecting edges
            RemoveControls(VertexConnectingEdges[vertex].Union(new UserControl[]{vertex}));

            // Remove depending edges
            VertexConnectingEdges.Remove(vertex);
            foreach (Vertex neighbor in AdjacencyList[vertex]) {
                ReverseAdjacencyList[neighbor].Remove(vertex);
            }
            foreach (Vertex neighbor in ReverseAdjacencyList[vertex]) {
                AdjacencyList[neighbor].Remove(vertex);
            }
            AdjacencyList.Remove(vertex);
            ReverseAdjacencyList.Remove(vertex);

            e.Handled = true;
        }

        private void VertexLeftClickDown(object sender, MouseButtonEventArgs e) {
            Vertex vertex = (Vertex)sender;
            // Save position so that upon mouse up, it can be determined it it was a click or a drag
            if (!UserIsDrawingEdge) {
                UserIsDraggingVertex = true;
            }
            DraggedVertex = vertex;
            DragStartPos = e.GetPosition(Canvas);
            e.Handled = true;
        }

        private void VertexLeftClickUp(object sender, MouseButtonEventArgs e) {
            Vertex vertex = (Vertex)sender;
            UserIsDraggingVertex = false;
            DraggedVertex = null;
            // Left click another vertex while drawing edge to finish edge
            // If it was a click not a drag:
            if (Math.Abs(e.GetPosition(Canvas).X - DragStartPos.X) < 10 && Math.Abs(e.GetPosition(Canvas).Y - DragStartPos.Y) < 10) {
                if (!UserIsDrawingEdge) {
                    BeginDrawingEdge(vertex);
                }
                else {
                    EndDrawingEdge(vertex);
                }
            }
            // Else if it was a drag not a click, and we are now ending the drag:
            else {
                
            }
        }

        private void BeginDrawingEdge(Vertex start) {
            UserIsDrawingEdge = true;
            StartingVertexEdgeDraw = start;
            ArrowLine newEdge = new ArrowLine();
            newEdge.SnapStartToVertexPoint(GetVertexCoords(start));
            newEdge.SnapEndToVertexPoint(GetVertexCoords(start));
            Canvas.Children.Add(newEdge);
            DrawnEdge = newEdge;
        }

        private void EndDrawingEdge(Vertex end) {
            if (DrawnEdge == null) return;
            UserIsDrawingEdge = false;

            double left = Canvas.GetLeft(end);
            double top = Canvas.GetTop(end);

            // Self loop edge
            // Remove the existing edge to replace with self loop
            if (StartingVertexEdgeDraw == end) {
                // Remove the arrow line
                Canvas.Children.Remove(DrawnEdge);
                DrawnEdge = DrawSelfLoop(end);
            }
            else {
                // Normal line edge
                // Snap the arrow to the Vertex and change its length to match
                DrawnEdge.SnapEndToVertexPoint(GetVertexCoords(end));
            }

            // Add to dictionaries
            // Should never be null at this point but Visual Studio keeps yelling at me
            if (StartingVertexEdgeDraw != null && DrawnEdge != null) {
                VertexConnectingEdges[StartingVertexEdgeDraw].Add(DrawnEdge);
                VertexConnectingEdges[end].Add(DrawnEdge);
                EdgesConnectingVertices.Add(DrawnEdge, new Tuple<Vertex, Vertex>(StartingVertexEdgeDraw, end));
                AdjacencyList[StartingVertexEdgeDraw].Add(end);
                ReverseAdjacencyList[end].Add(StartingVertexEdgeDraw);
            }

            // Finally,
            DrawnEdge = null;
            StartingVertexEdgeDraw = null;
        }

        private SelfLoop DrawSelfLoop(Vertex vertex) {
            SelfLoop selfLoop = new SelfLoop();
            Panel.SetZIndex(selfLoop, 2);
            Point vertexCoord = GetVertexCoords(vertex);
            selfLoop.SnapStartToVertexPoint(vertexCoord);
            selfLoop.SnapEndToVertexPoint(vertexCoord);
            Canvas.Children.Add(selfLoop);
            return selfLoop;
        }

        private void Canvas_MouseRightButtonDown(object sender, MouseButtonEventArgs e) {
            // Create a vertex
            var clickPos = e.GetPosition(Canvas);
            var vertex = new Vertex();
            vertex.ID = IDChooser.GetID();
            AddControl(vertex, clickPos);
            VertexConnectingEdges.Add(vertex, new List<Edge>());
            AdjacencyList.Add(vertex, new List<Vertex>());
            ReverseAdjacencyList.Add(vertex, new List<Vertex>());

            // Right click vertex to remove it
            vertex.MouseRightButtonDown += VertexRightClick;
            // Left click vertex
            vertex.MouseLeftButtonDown += VertexLeftClickDown;
            vertex.MouseLeftButtonUp += VertexLeftClickUp;

            /*
            UpdateStartingMatrix();*/
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
            Point mousePos = e.GetPosition(Canvas);
            // Vertex drag
            if (UserIsDraggingVertex && DraggedVertex != null) {
                MoveVertex(DraggedVertex, mousePos);
            }
            // Edge draw
            else if (UserIsDrawingEdge && DrawnEdge != null) {
                DrawnEdge.EndPoint = mousePos;
            }
        }
    }
}
