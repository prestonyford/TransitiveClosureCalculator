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
        public ArrowLine() : base() {
            InitializeComponent();
            DataContext = this;
        }

        public override event PropertyChangedEventHandler? PropertyChanged;
        protected override void OnPropertyChanged(string propertyName) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
