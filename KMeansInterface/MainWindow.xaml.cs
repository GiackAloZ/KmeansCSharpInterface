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

using System.Threading;

using KMeans;
using System.Windows.Threading;

namespace KMeansInterface
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private KMeansAlgorithm alg;

        private DispatcherTimer _dt;

        private bool _conv = false;

		private List<Centroid> _centroids = new List<Centroid>();
		private List<KMeans.Point> _points = new List<KMeans.Point>();

		private int _featuresNumber = 0;
        private int _pointsNumber = 0;
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
			tc1.Binding = new Binding("Coordinates[+" + (++_featuresNumber) + "]") { Mode = BindingMode.TwoWay };
			tc2.Binding = new Binding("Coordinates[" + (++_featuresNumber) + "]") { Mode = BindingMode.TwoWay };

			dtgCentroids.Columns.Add(tc1);
			dtgPoints.Columns.Add(tc2);

			dtgCentroids.ItemsSource = _centroids;
			dtgPoints.ItemsSource = _points;
		}

		private void btnRemoveFeature_Click(object sender, RoutedEventArgs e)
		{
            dtgCentroids.Columns.RemoveAt(dtgCentroids.Columns.Count - 1);
            dtgPoints.Columns.RemoveAt(dtgPoints.Columns.Count - 1);

            _centroids.RemoveAt(_centroids.Count - 1);
            _points.RemoveAt(_points.Count - 1);
        }

        private void cnvGraphic_MouseUp(object sender, MouseButtonEventArgs e)
        {
            double x = e.GetPosition(e.Device.Target).X;
            double y = e.GetPosition(e.Device.Target).Y;

            _points.Add(new KMeans.Point {Name = "Point " + (++_pointsNumber) , Coordinates = new List<double> { x, y } });

            SolidColorBrush dark = new SolidColorBrush();
            dark.Color = Color.FromArgb(255, 0, 0, 0);
            Ellipse ell = new Ellipse { Width = 10, Height = 10, Fill = dark, StrokeThickness = 2, Stroke = Brushes.Black };
            cnvGraphic.Children.Add(ell);
            Canvas.SetTop(ell, y-5);
            Canvas.SetLeft(ell, x-5);

            dtgPoints.Items.Refresh();
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            int nc = int.Parse(txtCentroidN.Text);
            alg = new KMeansAlgorithm(nc);
            foreach (KMeans.Point p in _points)
                alg.AddPoint(p);

            alg.InitializeAlgorithm();

            _dt = new DispatcherTimer();
            _dt.Interval = new TimeSpan(100000);
            _dt.Tick += Dt_Tick;
            _dt.Start();

            int cont = 0;
            foreach (Centroid c in _centroids)
            {
                c.Name = "Centroid " + (++cont);
            }

            dtgCentroids.ItemsSource = _centroids;
            dtgCentroids.SelectedIndex = 0;
            Binding bb = new Binding("SelectedItem.MyPoints");
            bb.ElementName = "dtgCentroids";
            dtgPoints.SetBinding(DataGrid.ItemsSourceProperty, bb);
        }

        private void Dt_Tick(object sender, EventArgs e)
        {
            List<Centroid> cc = alg.NextStep(out _conv);

            _centroids = cc;

            if (_conv)
                _dt.Stop();
            else
            {
                foreach (Centroid c in cc)
                {
                    SolidColorBrush dark = new SolidColorBrush();
                    dark.Color = Color.FromArgb(255, 255, 0, 0);
                    Ellipse ell = new Ellipse { Width = 10, Height = 10, Fill = dark, StrokeThickness = 2, Stroke = Brushes.Red };
                    cnvGraphic.Children.Add(ell);
                    Canvas.SetTop(ell, c.Coordinates[1] - 5);
                    Canvas.SetLeft(ell, c.Coordinates[0] - 5);
                }
                foreach (KMeans.Point p in _points)
                {
                    SolidColorBrush dark = new SolidColorBrush();
                    dark.Color = Color.FromArgb(255, 0, 0, 0);
                    Ellipse ell = new Ellipse { Width = 10, Height = 10, Fill = dark, StrokeThickness = 2, Stroke = Brushes.Black };
                    cnvGraphic.Children.Add(ell);
                    Canvas.SetTop(ell, p.Coordinates[1] - 5);
                    Canvas.SetLeft(ell, p.Coordinates[0] - 5);
                }
            }

            cnvGraphic.Children.Clear();
        }
    }
}
