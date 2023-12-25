using System;
using System.Collections.Generic;
using System.Linq;
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

namespace TransitiveClosureCalculator.User_Controls {
    /// <summary>
    /// Interaction logic for Vertex.xaml
    /// </summary>
    public partial class Vertex : UserControl {
        public Vertex() {
            InitializeComponent();
            this.DataContext = this;
            this.ID = "?";
            this.VertexHeight = 50;
            this.VertexWidth = 50;
        }

        public string ID { get; set; }
        public double VertexHeight { get; set; }
        public double VertexWidth { get; set; }

        // public event EventHandler VertexRightClicked;

        private void Grid_MouseRightButtonDown(object sender, MouseButtonEventArgs e) {
            // Console.WriteLine("Clicked on vertex " + ID);
            // VertexRightClicked?.Invoke(this, e);
            // Handled in event listener inside MainWindow.xaml.cs
        }
    }
}
