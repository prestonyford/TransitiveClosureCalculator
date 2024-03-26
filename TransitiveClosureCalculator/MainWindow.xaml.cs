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
    public partial class MainWindow : Window, IGraphUpdateHandler {

        private Dictionary<string, HashSet<string>> CurrentStringAdjacencyList = new Dictionary<string, HashSet<string>>();

        public MainWindow() {
            InitializeComponent();
            GraphCanvas canvas = new GraphCanvas(this);
            CanvasContainer.Child = canvas;
        }

        public void HandleCanvasUpdate(Dictionary<string, HashSet<string>> stringAdjacencyList) {
            UpdateStartingMatrix(stringAdjacencyList);
        }

        private void Calculate_Button_Click(object sender, RoutedEventArgs e) {
            UpdateResultingMatrix();
        }

        private void UpdateStartingMatrix(Dictionary<string, HashSet<string>> stringAdjacencyList) {
            CurrentStringAdjacencyList = stringAdjacencyList;
            Classes.Matrix matrix = new Classes.Matrix(stringAdjacencyList.Keys.ToList(), stringAdjacencyList);

            StartMatrixTextBox.Text = matrix.ToString();
            StartMatrixTextBlock.Text = $"Reflexive: {(matrix.IsReflexive ? "Yes" : "No")}; Symmetric: {(matrix.IsSymmetric ? "Yes" : "No")}; Transitive: {(matrix.IsTransitive ? "Yes" : "No")}";
        }

        private void UpdateResultingMatrix() {
            Classes.Matrix matrix = new Classes.Matrix(CurrentStringAdjacencyList.Keys.ToList(), CurrentStringAdjacencyList);

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