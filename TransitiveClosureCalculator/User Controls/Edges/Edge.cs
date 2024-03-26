using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using Point = System.Windows.Point;

namespace TransitiveClosureCalculator.User_Controls.Edges {
    public abstract class Edge : UserControl, INotifyPropertyChanged {
        protected Point startPoint;
        protected Point endPoint;
        protected double angle;

        public Edge() {
            IsHitTestVisible = false;
        }

        public virtual void SnapStartToVertexPoint(Point p) {
            Point corrected = new Point(p.X + Vertex.Diameter / 2.0, p.Y + Vertex.Diameter / 2.0);
            StartPoint = corrected;
        }
        public virtual void SnapEndToVertexPoint(Point p) {
            Point corrected = new Point(p.X + Vertex.Diameter / 2.0, p.Y + Vertex.Diameter / 2.0);
            EndPoint = corrected;
            double angleRadians = Math.Atan2(EndPoint.Y - StartPoint.Y, EndPoint.X - StartPoint.X);
            double distance = Math.Sqrt(Math.Pow(EndPoint.X - StartPoint.X, 2) + Math.Pow(EndPoint.Y - StartPoint.Y, 2));
            double newDistance = Math.Max(0, distance - 33);
            EndPoint = new Point(StartPoint.X + newDistance * Math.Cos(angleRadians), StartPoint.Y + newDistance * Math.Sin(angleRadians));

            double angleDegrees = angleRadians * (180 / Math.PI);
            Angle = angleDegrees;
        }

        public virtual void SnapEndToExactPoint(Point p) {
            Point corrected = new Point(p.X, p.Y);
            EndPoint = corrected;
            double angleRadians = Math.Atan2(EndPoint.Y - StartPoint.Y, EndPoint.X - StartPoint.X);
            double distance = Math.Sqrt(Math.Pow(EndPoint.X - StartPoint.X, 2) + Math.Pow(EndPoint.Y - StartPoint.Y, 2));
            EndPoint = new Point(StartPoint.X + distance * Math.Cos(angleRadians), StartPoint.Y + distance * Math.Sin(angleRadians));

            double angleDegrees = angleRadians * (180 / Math.PI);
            Angle = angleDegrees;
        }

        public Point StartPoint {
            get { return startPoint; }
            set {
                if (startPoint != value) {
                    startPoint = value;
                    OnPropertyChanged(nameof(StartPoint));
                }
            }
        }
        public Point EndPoint {
            get { return endPoint; }
            set {
                if (endPoint != value) {
                    endPoint = value;
                    OnPropertyChanged(nameof(EndPoint));
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
