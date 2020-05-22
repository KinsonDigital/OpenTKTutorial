using GLFW;
using OpenToolkit.Graphics.ES11;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using GLFWWindow = GLFW.Window;
using WPFWindow = System.Windows.Window;

namespace WPFGLSandBox
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : WPFWindow
    {
        private MiniEngine _engine;


        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }


        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var helper = new WindowInteropHelper(this);

            
            _engine = new MiniEngine(Dispatcher);

            //var window = Glfw.CreateWindow(800, 600, "GLFW", Monitor.None, GLFWWindow.None);
            //Glfw.MakeContextCurrent(window);
            //Glfw.SwapInterval(1);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            _engine.Run();
        }
    }
}
