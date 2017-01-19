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
using System.Collections.ObjectModel;

namespace KMeansInterface
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private KMeansAlgorithm _alg;
        private KNNAlgorithm _knnAlg;

        private DispatcherTimer _dt;

        private bool _conv = false;

		private double _timerSecondsDrawingLength = 1;

		private SolidColorBrush _pointFillColor = new SolidColorBrush(Color.FromArgb(255, 0, 0, 0));
		private SolidColorBrush _centroidFillColor = new SolidColorBrush(Color.FromArgb(255, 255, 0, 0));
		private SolidColorBrush _pointStrokeColor = new SolidColorBrush(Color.FromArgb(255, 0, 0, 0));
		private SolidColorBrush _centroidStrokeColor = new SolidColorBrush(Color.FromArgb(255, 255, 0, 0));
		private SolidColorBrush _lineStrokeColor = new SolidColorBrush(Color.FromArgb(255, 255, 0, 0));

        private List<SolidColorBrush> _centroidsFillAndStrokeColors = new List<SolidColorBrush>();
        private bool _differciateCentroids = false;

		private double _pointRadius = 10;
		private double _centroidRadius = 8;
		private double _pointThickness = 2;
		private double _centroidThickness = 2;
		private double _lineThickness = 1;

		private List<Centroid> _centroids = new List<Centroid>();
		private List<KMeans.Point> _points = new List<KMeans.Point>();

		private int _featuresNumber = 0;
        private int _pointsNumber = 0;
        private int _centroidsNumber = 0;

        private int _mode = 0;
        private List<string> _modes = new List<string> { "KMeans mode", "KNN mode" };

        private bool _calculatedAndSet = false;
        private List<SolidColorBrush> _pointsColorList;

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
            if (e.ChangedButton == MouseButton.Left)
            {
                double x = e.GetPosition((Canvas)sender).X;
                double y = e.GetPosition((Canvas)sender).Y;
                if (_calculatedAndSet)
                {
                    Centroid ddd = _knnAlg.GetPointFeature(new KMeans.Point(new ObservableCollection<double> { x, y }));
                    int ccc = 0;
                    foreach(Centroid c in _centroids)
                    {
                        if(c == ddd)
                        {
                            Ellipse ell = new Ellipse { Width = _pointRadius, Height = _pointRadius, Fill = _pointsColorList[ccc], StrokeThickness = _pointThickness, Stroke = _pointsColorList[ccc] };
                            cnvGraphic.Children.Add(ell);
                            Canvas.SetTop(ell, y - 5);
                            Canvas.SetLeft(ell, x - 5);
                        }
                        ccc++;
                    }
                }
                else
                {
                    _points.Add(new KMeans.Point("Point " + (++_pointsNumber), new ObservableCollection<double> { x, y }));

                    Ellipse ell = new Ellipse { Width = _pointRadius, Height = _pointRadius, Fill = _pointFillColor, StrokeThickness = _pointThickness, Stroke = _pointStrokeColor };
                    cnvGraphic.Children.Add(ell);
                    Canvas.SetTop(ell, y - 5);
                    Canvas.SetLeft(ell, x - 5);

                    _points[_pointsNumber - 1].Coordinates.CollectionChanged += (s, args) =>
                    {
                        ObservableCollection<double> d = (ObservableCollection<double>)s;
                        Canvas.SetTop(ell, d[1] - 5);
                        Canvas.SetLeft(ell, d[0] - 5);
                    };

                    dtgPoints.Items.Refresh();
                }
            }
            else if(e.ChangedButton == MouseButton.Right)
            {
                if (!_calculatedAndSet)
                {
                    double x = e.GetPosition((Canvas)sender).X;
                    double y = e.GetPosition((Canvas)sender).Y;

                    _centroids.Add(new Centroid("Centroid " + (++_centroidsNumber), new ObservableCollection<double> { x, y }));

                    Ellipse ell = new Ellipse { Width = _centroidRadius, Height = _centroidRadius, Fill = _centroidFillColor, StrokeThickness = _centroidThickness, Stroke = _centroidStrokeColor };
                    cnvGraphic.Children.Add(ell);
                    Canvas.SetTop(ell, y - 5);
                    Canvas.SetLeft(ell, x - 5);

                    _centroids[_centroidsNumber - 1].Coordinates.CollectionChanged += (s, args) =>
                    {
                        ObservableCollection<double> d = (ObservableCollection<double>)s;
                        Canvas.SetTop(ell, d[1] - 5);
                        Canvas.SetLeft(ell, d[0] - 5);
                    };

                    dtgCentroids.Items.Refresh();

                    txtCentroidN.Text = _centroidsNumber.ToString();
                }
            }
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            if(_mode == 0)
            {
                int nc = int.Parse(txtCentroidN.Text); //va messo a posto

                _differciateCentroids = (bool)chbDiff.IsChecked;

                _timerSecondsDrawingLength = sldVel.Value;
                _pointRadius = sldGP.Value;
                _centroidRadius = sldGC.Value;

                _alg = new KMeansAlgorithm(nc);
                foreach (KMeans.Point p in _points)
                    _alg.AddPoint(p);
                foreach (Centroid c in _centroids)
                    _alg.AddCentroid(c);

                if (_differciateCentroids)
                    GenerateRandomCentroidsColor(nc);

                _alg.InitializeAlgorithm();

                _dt = new DispatcherTimer();
                _dt.Interval = TimeSpan.FromSeconds(_timerSecondsDrawingLength);
                _dt.Tick += Dt_Tick;
                _dt.Start();
            }
            else if(_mode == 1)
            {
                int nc = int.Parse(txtCentroidN.Text); //va messo a posto

                _alg = new KMeansAlgorithm(nc);
                foreach (KMeans.Point p in _points)
                    _alg.AddPoint(p);
                foreach (Centroid c in _centroids)
                    _alg.AddCentroid(c);

                _centroids = _alg.CalculateResult();
                UpdateDataGrids();
                cnvGraphic.Children.Clear();
                _pointsColorList = ColorCalculatedPoints();
                _calculatedAndSet = true;
                _knnAlg = new KNNAlgorithm(_centroids);
            }
            
        }

        private List<SolidColorBrush> ColorCalculatedPoints()
        {
            List<SolidColorBrush> res = new List<SolidColorBrush>();
            Random aa = new Random();
            foreach(Centroid c in _centroids)
            {
                SolidColorBrush aaa = new SolidColorBrush(Color.FromArgb(255, (byte)aa.Next(0, 256), (byte)aa.Next(0, 256), (byte)aa.Next(0, 256)));
                foreach (KMeans.Point p in c.MyPoints)
                {
                    Ellipse ell = new Ellipse { Width = _pointRadius, Height = _pointRadius, Fill = aaa, StrokeThickness = _pointThickness, Stroke = aaa };
                    cnvGraphic.Children.Add(ell);
                    Canvas.SetTop(ell, p.Coordinates[1] - _pointRadius / 2);
                    Canvas.SetLeft(ell, p.Coordinates[0] - _pointRadius / 2);
                }
                res.Add(aaa);
            }
            return res;
        }

        private void GenerateRandomCentroidsColor(int n)
        {
            Random a = new Random();
            for (int i = 0; i < n; i++)
            {
                byte r = (byte)a.Next(0, 255);
                byte g = (byte)a.Next(0, 255);
                byte b = (byte)a.Next(0, 255);
                _centroidsFillAndStrokeColors.Add(new SolidColorBrush(Color.FromArgb(255, r, g, b)));
            }  
        }

        private void Dt_Tick(object sender, EventArgs e)
        {
            List<Centroid> cc = _alg.NextStep(out _conv);

            _centroids = cc;

			if (_conv)
			{
                _dt.Stop();
				UpdateDataGrids();
			}
            else
            {
                Ellipse ell;
                cnvGraphic.Children.Clear();
                foreach (KMeans.Point p in _points)
                {
                    ell = new Ellipse { Width = _pointRadius, Height = _pointRadius, Fill = _pointFillColor, StrokeThickness = _pointThickness, Stroke = _pointStrokeColor };
                    cnvGraphic.Children.Add(ell);
                    Canvas.SetTop(ell, p.Coordinates[1] - _pointRadius / 2);
                    Canvas.SetLeft(ell, p.Coordinates[0] - _pointRadius / 2);
                }
                int counter = 0;
                foreach (Centroid c in _centroids)
				{
                    if (_differciateCentroids)
                    {
                        ell = new Ellipse { Width = _centroidRadius, Height = _centroidRadius, Fill = _centroidsFillAndStrokeColors[counter], StrokeThickness = _centroidThickness, Stroke = _centroidsFillAndStrokeColors[counter] };
                    }
                    else
                    {
                        ell = new Ellipse { Width = _centroidRadius, Height = _centroidRadius, Fill = _centroidFillColor, StrokeThickness = _centroidThickness, Stroke = _centroidStrokeColor };
                    }
                    cnvGraphic.Children.Add(ell);
					Canvas.SetTop(ell, c.Coordinates[1] - _centroidRadius / 2);
					Canvas.SetLeft(ell, c.Coordinates[0] - _centroidRadius / 2);
                    Line l;
					foreach(KMeans.Point p in c.MyPoints)
					{
                        if (_differciateCentroids)
                        {
                            l = new Line { Stroke = _centroidsFillAndStrokeColors[counter], StrokeThickness = _lineThickness };
                        }
                        else
                        {
						    l = new Line { Stroke = _lineStrokeColor, StrokeThickness = _lineThickness };
                        }
						l.X1 = c.Coordinates[0];
						l.Y1 = c.Coordinates[1];
						l.X2 = p.Coordinates[0];
						l.Y2 = p.Coordinates[1];
						cnvGraphic.Children.Add(l);
					}
                    counter++;
				}
			}
        }

		private void UpdateDataGrids()
		{
			int cont = 0;
			foreach (Centroid c in _centroids)
			{
                if(c.Name == null)
				    c.Name = "Centroid " + (++cont);
			}

			dtgCentroids.ItemsSource = _centroids;
			dtgCentroids.SelectedIndex = 0;
			Binding bb = new Binding("SelectedItem.MyPoints");
			bb.ElementName = "dtgCentroids";
			dtgPoints.SetBinding(DataGrid.ItemsSourceProperty, bb);
        }

		private void btnClearAll_Click(object sender, RoutedEventArgs e)
		{
            if(_dt?.IsEnabled == true)
                _dt.Stop();
			_centroids = new List<Centroid>();
			_points = new List<KMeans.Point>();
            _pointsNumber = 0;
            _centroidsNumber = 0;
            _calculatedAndSet = false;
            txtCentroidN.Text = _centroidsNumber.ToString();
			dtgCentroids.ItemsSource = _centroids;
			dtgPoints.ItemsSource = _points;
			cnvGraphic.Children.Clear();
		}

        private void sldGC_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            _centroidRadius = sldGC.Value;
        }

        private void sldGP_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            _pointRadius = sldGP.Value;
        }

        private void btnChangeMode_Click(object sender, RoutedEventArgs e)
        {
            _mode = _mode == 1 ? 0 : 1;
            lblMode.Content = _modes[_mode];

            if (_dt?.IsEnabled == true)
                _dt.Stop();
            _centroids = new List<Centroid>();
            _points = new List<KMeans.Point>();
            _pointsNumber = 0;
            _centroidsNumber = 0;
            _calculatedAndSet = false;
            txtCentroidN.Text = _centroidsNumber.ToString();
            dtgCentroids.ItemsSource = _centroids;
            dtgPoints.ItemsSource = _points;
            cnvGraphic.Children.Clear();
        }
    }
}
