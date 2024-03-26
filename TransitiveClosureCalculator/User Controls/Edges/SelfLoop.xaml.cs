using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace TransitiveClosureCalculator.User_Controls.Edges {
    /// <summary>
    /// Interaction logic for SelfLoop.xaml
    /// </summary>
    public partial class SelfLoop : Edge {

        protected PointCollection polygonPoints;

        public SelfLoop() : base() {
            InitializeComponent();
            DataContext = this;
        }

        public PointCollection PolygonPoints {
            get { return polygonPoints; }
            set {
                if (polygonPoints != value) {
                    polygonPoints = value;
                    Console.WriteLine(polygonPoints);
                    OnPropertyChanged(nameof(PolygonPoints));
                }
            }
        }

        public override void SnapStartToVertexPoint(Point p) {
            Point corrected = new Point(p.X + Vertex.Diameter / 2.0 + 25, p.Y + Vertex.Diameter / 2.0 + 0);
            StartPoint = corrected;
        }
        public override void SnapEndToVertexPoint(Point p) {
            Point corrected = new Point(p.X + Vertex.Diameter / 2.0 - 0, p.Y + Vertex.Diameter / 2.0 - 25);
            EndPoint = corrected;

            PolygonPoints = new PointCollection(new Point[] {
                new Point(-5 + p.Y - 5, -5 - p.X - Vertex.Diameter/2),
                new Point(8 + p.Y - 5, 0 - p.X - Vertex.Diameter/2),
                new Point(-5 + p.Y - 5, 5 - p.X - Vertex.Diameter/2),
            });
        }

        public override event PropertyChangedEventHandler? PropertyChanged;
        protected override void OnPropertyChanged(string propertyName) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
