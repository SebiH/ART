using System.Collections.ObjectModel;
using System.Windows;

namespace GUI.Optitrack
{
    public partial class OptitrackLogWindow : Window
    {
        public ObservableCollection<string> Log
        {
            get { return (ObservableCollection<string>)GetValue(LogProperty); }
            set { SetValue(LogProperty, value); }
        }

        public static readonly DependencyProperty LogProperty = DependencyProperty.Register("Log", typeof(ObservableCollection<string>), typeof(OptitrackLogWindow));


        public OptitrackLogWindow()
        {
            Log = new ObservableCollection<string>();
            InitializeComponent();
        }
    }
}
