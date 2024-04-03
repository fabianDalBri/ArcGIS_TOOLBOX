using ArcGIS.Desktop.Core.Geoprocessing;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using Microsoft.Win32;
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

namespace FinalProject
{
    /// <summary>
    /// Interaction logic for form.xaml
    /// </summary>
    public partial class form : Window
    {
        public form()
        {
            InitializeComponent();
            //first push
        }
        public String ChooseFile()
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.ShowDialog();
            ofd.InitialDirectory = @"H:\";
            //MessageBox.show(ofd.SafeFileName);
            ofd.Filter = "Files (*.png;*.shp;*.tif)|*.png;*.shp;*.tif|All files(*.*) | *.* ";

            return ofd.FileName;

        }
        private void Bebyggelse_Click(object sender, RoutedEventArgs e)
        {

        }
        private void Markdata_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Hojddata_Click(object sender, RoutedEventArgs e)
        {
            txtHojddata.Text = ChooseFile();
            string uriShp = txtHojddata.Text;
            Map map = MapView.Active.Map;
            Uri uri = new Uri(uriShp);

            QueuedTask.Run(() =>
            {
                LayerFactory.Instance.CreateLayer(uri, map);
            });
        }

        private void Vagdata_Click(object sender, RoutedEventArgs e)
        {

        }

        private void CalculateSlope()
        {
            Map map = MapView.Active.Map;
            string filepath = txtHojddata.Text;
            //MessageBox.Show(txtHojddata.Text);
            string outputSlope = @"H:\gis-applikationer\FinalProject\slope.tif";
            // Create a raster layer using a path to an image.
            // Note: You can create a raster layer from a url, project item, or data connection.
            QueuedTask.Run(() =>
            {
                //Execution of the tool.
                string inMeasures = "DEGREE";
                var parameters = Geoprocessing.MakeValueArray(filepath, outputSlope, inMeasures);
                var gpSlope = Geoprocessing.ExecuteToolAsync("Slope_3d", parameters);

                // Check if the tool executed successfully
                if (gpSlope.Result.IsFailed)
                {
                    MessageBox.Show("Slope calculation failed.", "Error");
                    return;
                }
                MessageBox.Show("Slope calculation completed successfully.", "Success");
            });
            //CalculateConstraint(outputSlope, @"\\hig-ad\student\homes\gis-applikationer\FinalProject\slopeCalculated.tif", "slope");
        }
        private void CalculateConstraint(string inRaster, string outRaster, string type)
        {
            Map map = MapView.Active.Map;
            string filepath = inRaster;


            //Create an output file name for the layer
            // Create a raster layer using a path to an image.
            // Note: You can create a raster layer from a url, project item, or data connection.
            QueuedTask.Run(() =>
            {
                //original RGB file as layer
                RasterLayer rasterLayer = LayerFactory.Instance.CreateLayer(new Uri(filepath), map) as RasterLayer;
                // Get the raster from the layer
                var raster = rasterLayer.GetRaster();
                // Perform raster calculation
                // In for-loop we store each band's path i a string array
                var bandnameArray = new string[3];
                for (int i = 0; i < raster.GetBandCount(); i++)
                {

                    var rasterBand = raster.GetBand(i);
                    //Get the name of each band in RGB raster
                    //Ban1_1 Band_2 and Band_3
                    var rasterBandName = raster.GetBand(i).GetName();

                    // Note: You can create a raster layer from a url, project item, or data connection.
                    // we get a Band number from rasterBandName and add it to the raster path
                    //RasterLayer rasterBandLayer = LayerFactory.Instance.CreateLayer(new Uri(filepath + "\\" + rasterBandName), map) as RasterLayer;

                    //create string array where we store path to each band
                    bandnameArray[i] = filepath + "\\" + rasterBandName;
                    MessageBox.Show(bandnameArray[i]);
                }

                string maExpression = string.Empty;
                //calculate medium to high height from min max values
                if (type.Equals("height"))
                {
                    maExpression = $"Con(\"{bandnameArray[0]}\" > 8.5, 1, 0)";
                }
                else if (type.Equals("slope"))
                {
                    maExpression = $"Con((\"{bandnameArray[0]}\" > 25) & (\"{bandnameArray[0]}\" < 45), 1, 0)";
                }
                //calculate NDVI/NDWI from min max values
                else if (type.Equals("NDVI"))
                {
                    maExpression = $"Con(\"{bandnameArray[0]}\" > 0.53, 1, 0)";
                }
                else if (type.Equals("NDWI"))
                {
                    maExpression = "(" + bandnameArray[0] + "-" + bandnameArray[2] + ")" + "/" + "(" + bandnameArray[0] + "+" + bandnameArray[2] + ")";
                }
                else if (type.Equals("direction"))
                {
                    maExpression = $"Con((\"{bandnameArray[0]}\" > 112.5) & (\"{bandnameArray[0]}\" < 247.5), 0, 1)";
                }


                MessageBox.Show(maExpression);
                // make the input parameter values array
                var valueArray = Geoprocessing.MakeValueArray(maExpression, outRaster);

                // execute the Raster calculator tool to process the map algebra expression
                var gpToolCalc = Geoprocessing.ExecuteToolAsync("RasterCalculator_sa", valueArray);

                // Check if the execution was successful
                if (gpToolCalc.Result.IsFailed)
                {
                    // Get the tool errors
                    var errors = gpToolCalc.Result.ErrorMessages.Select(err => err.Text);
                    // Display errors
                    MessageBox.Show("Error executing constraint Calculator: " + string.Join(Environment.NewLine,
                   errors));
                }
                else
                {
                    // Raster calculator executed successfully
                    MessageBox.Show("constraint Calculator execution successful.");
                    if (type.Equals("direction"))
                    {
                        //createMCA();
                    }
                }
            });
        }

        private void Slope_Click(object sender, RoutedEventArgs e)
        {
            CalculateSlope();
        }

        private void txtHojddata_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}
