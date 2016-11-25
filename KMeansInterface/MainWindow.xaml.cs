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

using KMeans;

namespace KMeansInterface
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
		private List<Centroid> _centroids = new List<Centroid>();
		private List<KMeans.Point> _points = new List<KMeans.Point>();

		private int _featuresNumber = 0;
        public MainWindow()
        {
            InitializeComponent();
        }

		private void winMain_Loaded(object sender, RoutedEventArgs e)
		{
			dtgCentroids.ItemsSource = _centroids;
			dtgPoints.ItemsSource = _points;
		}

		private void btnAddFeature_Click(object sender, RoutedEventArgs e)
		{
			string nameFeature = txtFeature.Text;

			foreach (Centroid c in _centroids)
				c.Coordinates.Add(0);
			foreach (KMeans.Point p in _points)
				p.Coordinates.Add(0);

			DataGridTextColumn tc1 = new DataGridTextColumn();
			DataGridTextColumn tc2 = new DataGridTextColumn();
			tc1.Header = nameFeature;
			tc2.Header = nameFeature;
			tc1.Binding = new Binding("Coordinates[0]");
			tc2.Binding = new Binding("Coordinates[0]");

			dtgCentroids.Columns.Add(tc1);
			dtgPoints.Columns.Add(tc2);

			dtgCentroids.ItemsSource = _centroids;
			dtgPoints.ItemsSource = _points;
		}

		private void btnRemoveFeature_Click(object sender, RoutedEventArgs e)
		{
			int i = 0;
		}
	}
}
