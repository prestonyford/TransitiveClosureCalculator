using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace TransitiveClosureCalculator.User_Controls.Edges {
    public abstract class Edge : UserControl, INotifyPropertyChanged {
        public Edge() {
            IsHitTestVisible = false;
        }

        public abstract event PropertyChangedEventHandler? PropertyChanged;
        protected abstract void OnPropertyChanged(string propertyName);
    }
}
