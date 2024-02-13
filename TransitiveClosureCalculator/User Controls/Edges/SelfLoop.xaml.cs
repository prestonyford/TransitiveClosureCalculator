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
        
        public SelfLoop() : base() {
            InitializeComponent();
            DataContext = this;
        }

        

        public override void SnapStartToVertexPoint(Point p) {
            Point corrected = new Point(p.X + Vertex.Diameter / 2.0 + 50, p.Y + Vertex.Diameter / 2.0 + 0);
            StartPoint = corrected;
        }
        public override void SnapEndToVertexPoint(Point p) {
            Point corrected = new Point(p.X + Vertex.Diameter / 2.0 - 0, p.Y + Vertex.Diameter / 2.0 - 50);
            EndPoint = corrected;
        }

        public override event PropertyChangedEventHandler? PropertyChanged;
        protected override void OnPropertyChanged(string propertyName) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
