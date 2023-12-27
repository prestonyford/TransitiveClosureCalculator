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
    /// Interaction logic for ArrowLine.xaml
    /// </summary>
    public partial class ArrowLine : UserControl {
        public ArrowLine() {
            InitializeComponent();
            DataContext = this;
            IsHitTestVisible = false;
        }

        public double X1 {  get; set; }
        public double Y1 { get; set; }
        public double X2 { get; set; }
        public double Y2 { get; set; }
        public double Angle {  get; set; }
    }
}
