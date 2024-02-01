using System;
using System.Collections.Generic;
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

namespace TransitiveClosureCalculator.User_Controls {
    /// <summary>
    /// Interaction logic for GraphCanvas.xaml
    /// </summary>
    public partial class GraphCanvas : UserControl {
        private IDChooser IDChooser = new IDChooser();

        private bool UserIsDraggingVertex = false;
        private Vertex? DraggedVertex = null;
        private Point DragStartPos = new Point(0, 0);
        private bool UserIsDrawingEdge = false;
        private Point DrawingEdgeStartPos = new Point(0, 0);
        private ArrowLine? DrawnEdge = null;
        private Vertex? StartingVertexEdgeDraw = null;
        private Dictionary<Vertex, List<Edge>> VertexConnectingEdges = new Dictionary<Vertex, List<Edge>>();
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
        private void AddControl(UserControl control, Point pos) {
            Point point = CorrectPoint(pos);

            Canvas.Children.Add(control);
            Canvas.SetLeft(control, point.X - Vertex.Diameter/2);
            Canvas.SetTop(control, point.Y - Vertex.Diameter/2);
        }

        private void RemoveControl(UserControl control) {
            Canvas.Children.Remove(control);
            // TODO: FIRE EVENT TO UPDATE STARTING MATRIX
        }

        private Point SnapEdgePoint(Vertex vertex) {
            return new Point(Canvas.GetLeft(vertex), Canvas.GetTop(vertex));
        }

        private void MoveVertex(Vertex vertex, Point pos) {
            // Move vertex if dragging one
            Point correctedPoint = CorrectPoint(pos);
            Canvas.SetLeft(DraggedVertex, correctedPoint.X - Vertex.Diameter / 2);
            Canvas.SetTop(DraggedVertex, correctedPoint.Y - Vertex.Diameter / 2);
        }

        private void VertexRightClick(object sender, MouseButtonEventArgs e) {
            Vertex vertex = (Vertex)sender;

            if (UserIsDrawingEdge && vertex == StartingVertexEdgeDraw || UserIsDraggingVertex) {
                e.Handled = true;
                return;
            }
            
            RemoveControl(vertex);
            IDChooser.RemoveID(vertex.ID);

            // Remove depending edges
            foreach (Edge edge in VertexConnectingEdges[vertex]) {
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
                BeginDrawingEdge(vertex);
            }
            // Else if it was a drag not a click, and we are now ending the drag:
            else {
                
            }
        }

        private void BeginDrawingEdge(Vertex start) {
            UserIsDrawingEdge = true;
            ArrowLine newEdge = new ArrowLine();
            newEdge.X1 = SnapEdgePoint(start).X;
            newEdge.Y1 = SnapEdgePoint(start).Y;
            newEdge.X2 = SnapEdgePoint(start).X;
            newEdge.Y2 = SnapEdgePoint(start).Y;
            Canvas.Children.Add(newEdge);
            Canvas.SetLeft(newEdge, 100);
            Canvas.SetTop(newEdge, 100);
            DrawnEdge = newEdge;
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
                DrawnEdge.X2 = mousePos.X;
                DrawnEdge.Y2 = mousePos.Y;
            }
        }
    }
}
