using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CompareDirs { 
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window {
		//members
		private string path1, path2;
		//constructors
		public MainWindow() {
			InitializeComponent();
			path1 = path2 = null;
		}
		//methods
		public void OpenChooser1Click(object sender, RoutedEventArgs e) {
			path1 = OpenDialog();
		}
		public void OpenChooser2Click(object sender, RoutedEventArgs e) {
			path2 = OpenDialog();
        }
		public string OpenDialog() {
			FolderBrowserDialog dialog = new FolderBrowserDialog();
			dialog.ShowNewFolderButton = false;
			DialogResult res = dialog.ShowDialog();
			return res.Equals(System.Windows.Forms.DialogResult.OK) ? dialog.SelectedPath : null;
		}
		private void AddChildrenToVisualTree(System.Windows.Controls.TreeView visTree, TreeRoot tree) {

		}
		public void CompareButtonClick(object sender, RoutedEventArgs e) {
			try {
				if (path1 != null && path2 != null) {
					TreeRoot root1 = new TreeRoot(path1);
					TreeRoot copyRoot1 = new TreeRoot(root1);
					root1.Print();
					TreeRoot root2 = new TreeRoot(path2);
					TreeRoot copyRoot2 = new TreeRoot(root2);
					root2.Print();
					root1.CompareWith(copyRoot2);
					root2.CompareWith(copyRoot1);
				}
			}
			//catch(Exception x) {
			//Console.WriteLine(x.Message);
			//}
		}
	}
}
