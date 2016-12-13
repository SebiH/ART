using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace GUI.Optitrack
{
    public partial class OptitrackLogControl : UserControl
    {
        public ObservableCollection<string> Log
        {
            get { return (ObservableCollection<string>)GetValue(LogProperty); }
            set { SetValue(LogProperty, value); }
        }

        public static readonly DependencyProperty LogProperty = DependencyProperty.Register("Log", typeof(ObservableCollection<string>), typeof(OptitrackLogControl));


        public OptitrackLogControl()
        {
            Log = new ObservableCollection<string>();
            InitializeComponent();
        }
    }
}
