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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TransitiveClosureCalculator.User_Controls.Edges {
    /// <summary>
    /// Interaction logic for ArrowLine.xaml
    /// </summary>
    public partial class ArrowLine : Edge {
        private double x1;
        private double y1;
        private double x2;
        private double y2;
        private double angle;
        public ArrowLine() : base() {
            InitializeComponent();
            DataContext = this;
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

        public override event PropertyChangedEventHandler? PropertyChanged;
        protected override void OnPropertyChanged(string propertyName) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
