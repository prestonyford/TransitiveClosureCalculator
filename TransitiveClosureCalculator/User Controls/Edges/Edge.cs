using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using Point = System.Windows.Point;

namespace TransitiveClosureCalculator.User_Controls.Edges {
    public abstract class Edge : UserControl, INotifyPropertyChanged {
        protected double x1;
        protected double y1;
        protected double x2;
        protected double y2;
        protected double angle;

        public Edge() {
            IsHitTestVisible = false;
        }

        public void SetStartPoint(Point p) {
            X1 = p.X;
            Y1 = p.Y;
        }
        public void SetEndPoint(Point p) {
            X2 = p.X;
            Y2 = p.Y;
        }
        public void SnapEndToVertexPoint(Point p) {
            Point corrected = new Point(p.X + Vertex.Diameter / 2.0, p.Y + Vertex.Diameter / 2.0);
            SetEndPoint(corrected);
        }

        public double X1 {
            get { return x1; }
            set {
                if (x1 != value) {
                    x1 = value;
                    OnPropertyChanged(nameof(X1));
                }
            }
        }
        public double Y1 {
            get { return y1; }
            set {
                if (y1 != value) {
                    y1 = value;
                    OnPropertyChanged(nameof(Y1));
                }
            }
        }
        public double X2 {
            get { return x2; }
            set {
                if (x2 != value) {
                    x2 = value;
                    OnPropertyChanged(nameof(X2));
                }
            }
        }
        public double Y2 {
            get { return y2; }
            set {
                if (y2 != value) {
                    y2 = value;
                    OnPropertyChanged(nameof(Y2));
                }
            }
        }
        public double Angle {
            get { return angle; }
            set {
                if (angle != value) {
                    angle = value;
                    OnPropertyChanged(nameof(Angle));
                }
            }
        }

        public abstract event PropertyChangedEventHandler? PropertyChanged;
        protected abstract void OnPropertyChanged(string propertyName);
    }
}
