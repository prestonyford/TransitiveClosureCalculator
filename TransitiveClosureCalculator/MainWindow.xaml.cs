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

        public MainWindow() {
            InitializeComponent();
        }


        private void Calculate_Button_Click(object sender, RoutedEventArgs e) {
            // UpdateResultingMatrix();
        }

        // Helpers

        /*private ArrowLine DrawArrowLine(Point start, Point end, int zIndex = 1) {
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
        }*/
    }
}