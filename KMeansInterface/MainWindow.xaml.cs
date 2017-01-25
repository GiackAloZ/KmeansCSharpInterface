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
using KMeans.Exceptions;

using System.ComponentModel;
using System.Runtime.Serialization;
using System.Runtime.CompilerServices;
using System.Xml;
using Microsoft.Win32;
using System.IO;

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
        private int _evaluatedCounter = 0;

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
                    KMeans.Point p = new KMeans.Point("Evaluated point " + (++_evaluatedCounter), new ObservableCollection<double> { x, y });
                    Centroid ddd = _knnAlg.GetPointFeature(p);
                    int ccc = 0;
                    foreach(Centroid c in _centroids)
                    {
                        if(c == ddd)
                        {
                            Ellipse ell = new Ellipse { Width = _pointRadius, Height = _pointRadius, Fill = _pointsColorList[ccc], StrokeThickness = _pointThickness, Stroke = _pointsColorList[ccc] };
                            cnvGraphic.Children.Add(ell);
                            Canvas.SetTop(ell, y - 5);
                            Canvas.SetLeft(ell, x - 5);
							c.MyPoints.Add(p);
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
            int nc;
            bool ok = int.TryParse(txtCentroidN.Text, out nc); //va messo a posto
            if (!ok)
            {
                MessageBox.Show("Inserire un numero intero!", "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            try
            {

                if (_mode == 0)
                {
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
                    _knnAlg = new KNNAlgorithm(4, _centroids);
                }
            }
            catch(KMeansException ex)
            {
                MessageBox.Show(ex.Message, "Algorithm error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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

		private void DrawPoint(double x, double y)
		{
			Ellipse ell = new Ellipse { Width = _pointRadius, Height = _pointRadius, Fill = _pointFillColor, StrokeThickness = _pointThickness, Stroke = _pointStrokeColor };
			cnvGraphic.Children.Add(ell);
			Canvas.SetTop(ell, y - 5);
			Canvas.SetLeft(ell, x - 5);
		}

		private void DrawCentroid(double x, double y)
		{
			Ellipse ell = new Ellipse { Width = _centroidRadius, Height = _centroidRadius, Fill = _centroidFillColor, StrokeThickness = _centroidThickness, Stroke = _centroidStrokeColor };
			cnvGraphic.Children.Add(ell);
			Canvas.SetTop(ell, y - 5);
			Canvas.SetLeft(ell, x - 5);
		}

		private void DrawCentroid(double x, double y, Ellipse ell)
		{
			cnvGraphic.Children.Add(ell);
			Canvas.SetTop(ell, y - 5);
			Canvas.SetLeft(ell, x - 5);
		}

		private void DrawLine(double x1, double y1, double x2, double y2)
		{
			Line l = new Line { Stroke = _lineStrokeColor, StrokeThickness = _lineThickness };
			l.X1 = x1;
			l.Y1 = y1;
			l.X2 = x2;
			l.Y2 = y2;
			cnvGraphic.Children.Add(l);
		}

		private void DrawLine(double x1, double y1, double x2, double y2, Line l)
		{
			l.X1 = x1;
			l.Y1 = y1;
			l.X2 = x2;
			l.Y2 = y2;
			cnvGraphic.Children.Add(l);
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
					DrawPoint(p.Coordinates[0], p.Coordinates[1]);
					/*
                    ell = new Ellipse { Width = _pointRadius, Height = _pointRadius, Fill = _pointFillColor, StrokeThickness = _pointThickness, Stroke = _pointStrokeColor };
                    cnvGraphic.Children.Add(ell);
                    Canvas.SetTop(ell, p.Coordinates[1] - _pointRadius / 2);
                    Canvas.SetLeft(ell, p.Coordinates[0] - _pointRadius / 2);
					*/
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

					DrawCentroid(c.Coordinates[0], c.Coordinates[1], ell);
					/*
                    cnvGraphic.Children.Add(ell);
					Canvas.SetTop(ell, c.Coordinates[1] - _centroidRadius / 2);
					Canvas.SetLeft(ell, c.Coordinates[0] - _centroidRadius / 2);
					*/


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
						DrawLine(c.Coordinates[0], c.Coordinates[1], p.Coordinates[0], p.Coordinates[1], l);
						/*
						l.X1 = c.Coordinates[0];
						l.Y1 = c.Coordinates[1];
						l.X2 = p.Coordinates[0];
						l.Y2 = p.Coordinates[1];
						cnvGraphic.Children.Add(l);
						*/
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
            dtgCentroids.IsReadOnly = true;
            dtgPoints.IsReadOnly = true;
        }
		
		private void ClearAll()
		{
			_centroids = new List<Centroid>();
			_points = new List<KMeans.Point>();
			_pointsNumber = 0;
			_centroidsNumber = 0;
			_calculatedAndSet = false;
			_centroidsFillAndStrokeColors.Clear();
			txtCentroidN.Text = _centroidsNumber.ToString();
			dtgCentroids.ItemsSource = _centroids;
			dtgPoints.ItemsSource = _points;
			cnvGraphic.Children.Clear();

			if (_mode == 0)
			{
				dtgCentroids.IsReadOnly = false;
				dtgPoints.IsReadOnly = false;
			}
		}

		private void btnClearAll_Click(object sender, RoutedEventArgs e)
		{
            if(_dt?.IsEnabled == true)
                _dt.Stop();

			ClearAll();
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
            if(_mode == 1)
            {
                _mode = 0;
                dtgCentroids.IsReadOnly = false;
                dtgPoints.IsReadOnly = false;
            }
            else if(_mode == 0)
            {
                _mode = 1;
                dtgCentroids.IsReadOnly = true;
                dtgPoints.IsReadOnly = true;
            }
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

        private void btnHelp_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Modalità Kmeans:\n- Per aggiungere i punti, cliccare destro sul grafico\n- Per aggiungere centroidi, cliccare sinistro sul grafico\n- Per modificare nome e posizione di punti e centroidi, modificare i valori nelle combobox\n- Modificare le opzioni a piacimento\n- Decidere il numero di centroidi (features o raggruppamenti) necessari\n- Cliccare sul bottone Start per eseguire l'algoritmo\n- Cliccare sul bottone Clear All per fermare l'esecuzione dell'algoritmo e cancellare tutti i dati inseriti\n\nModalità KNN:\n- Eseguire tutte le istruzioni della modalità Kmeans per inserire i punti e creare i raggruppamenti\n- Ora si possono inserire nuovi punti cliccando destro sul grafico che verranno evaluati e assegnati al raggruppamento corretto\n- Cliccare sul bottone Clear All per cancellare tutti i dati inseriti");
        }

		private void btnSaveData_Click(object sender, RoutedEventArgs e)
		{
			if(_dt?.IsEnabled == false)
			{
				SaveFileDialog dial = new SaveFileDialog();
				dial.Filter = "bho|*.xml|Tutti i file|*.*";
				if ((bool)dial.ShowDialog())
				{
					XmlWriterSettings set = new XmlWriterSettings();
					set.Indent = true;
					using (FileStream stream = new FileStream(dial.FileName, FileMode.Create, FileAccess.Write, FileShare.None))
					using (XmlWriter xmlw = XmlWriter.Create(stream, set))
					{
						DataContractSerializer dc = new DataContractSerializer(typeof(List<Centroid>));
						dc.WriteObject(xmlw, _centroids);
					}
				}
			}
		}

		private void btnLoadData_Click(object sender, RoutedEventArgs e)
		{
			if (_dt?.IsEnabled == false)
			{
				ClearAll();
				OpenFileDialog dial = new OpenFileDialog();
				dial.Filter = "bho|*.xml|Tutti i file|*.*";
				if ((bool)dial.ShowDialog())
				{
					using (FileStream stream = new FileStream(dial.FileName, FileMode.Open, FileAccess.Read, FileShare.None))
					using (XmlReader xmlr = XmlReader.Create(stream))
					{
						DataContractSerializer dc = new DataContractSerializer(typeof(List<Centroid>));
						_centroids = (List<Centroid>)dc.ReadObject(xmlr);
					}
				}
				UpdateDataGrids();
				foreach(Centroid c in _centroids)
				{
					DrawCentroid(c.Coordinates[0], c.Coordinates[1]);
					foreach(KMeans.Point p in c.MyPoints)
					{
						DrawPoint(p.Coordinates[0], p.Coordinates[1]);
						DrawLine(c.Coordinates[0], c.Coordinates[1], p.Coordinates[0], p.Coordinates[1]);
					}
				}
			}
		}
	}
}
