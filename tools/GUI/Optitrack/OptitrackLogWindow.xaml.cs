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
using System.Windows.Shapes;

namespace GUI.Optitrack
{
    public partial class OptitrackLogWindow : Window
    {
        public string Log
        {
            get { return (string)GetValue(LogProperty); }
            set { SetValue(LogProperty, value); }
        }

        public static readonly DependencyProperty LogProperty = DependencyProperty.Register("Log", typeof(string), typeof(OptitrackLogWindow));


        public OptitrackLogWindow()
        {
            InitializeComponent();
        }
    }
}
