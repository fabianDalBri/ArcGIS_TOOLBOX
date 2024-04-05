using ArcGIS.Core.CIM;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Core.Geoprocessing;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using Microsoft.Win32;
using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

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
            txtVagData.Text = ChooseFile();
            string uriShp = txtVagData.Text;
            Map map = MapView.Active.Map;
            Uri uri = new Uri(uriShp);

            QueuedTask.Run(() =>
            {
                LayerFactory.Instance.CreateLayer(uri, map);
            });
        }

        private void CalculateSlope()
        {
            string filepath = txtHojddata.Text;
            //MessageBox.Show(txtHojddata.Text);
            string outputSlope = @"\\hig-ad\student\homes\gis-applikationer\FinalProject\slope.tif";
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
                CalculateConstraint(outputSlope, @"\\hig-ad\student\homes\gis-applikationer\FinalProject\slopeCalculated.tif", "slope");
            });
        }
        private void CalculateAspect()
        {
            string filepath = txtHojddata.Text;
            string outputAspect = @"\\hig-ad\student\homes\gis-applikationer\FinalProject\aspect.tif";
            //Uri defaultGeodatabasePath = new Uri(Project.Current.DefaultGeodatabasePath);
            // Create a raster layer using a path to an image.
            // Note: You can create a raster layer from a url, project item, or data connection.
            QueuedTask.Run(() =>
            {
                // Run the Slope geoprocessing tool
                var parameters = Geoprocessing.MakeValueArray(filepath, outputAspect);
                var gpSlope = Geoprocessing.ExecuteToolAsync("Aspect_3d", parameters);
                // Check if the tool executed successfully
                if (gpSlope.Result.IsFailed)
                {
                    MessageBox.Show("Aspect calculation failed.", "Error");
                    return;
                }
                MessageBox.Show("Aspect calculation completed successfully.", "Success");
                CalculateConstraint(outputAspect, @"\\hig-ad\student\homes\gis-applikationer\FinalProject\directionCalculated.tif", "direction");
            });
        }
        public void CalculateBuffer()
        {
            string filepath = txtVagData.Text;
            string outputbuff = @"\\hig-ad\student\homes\gis-applikationer\FinalProject\buffer.shp";
            //Uri defaultGeodatabasePath = new Uri(Project.Current.DefaultGeodatabasePath);
            // Create a raster layer using a path to an image.
            // Note: You can create a raster layer from a url, project item, or data connection.
            QueuedTask.Run(() =>
            {
                // Run the Slope geoprocessing tool
                var parameters = Geoprocessing.MakeValueArray(filepath, outputbuff, 200);
                var gpSlope = Geoprocessing.ExecuteToolAsync("Analysis.buffer", parameters);
                // Check if the tool executed successfully
                if (gpSlope.Result.IsFailed)
                {
                    MessageBox.Show("Buffer calculation failed.", "Error");
                    return;
                }
                MessageBox.Show("Buffer calculation completed successfully.", "Success");
                BufferToRaster(outputbuff);
            });

        }

        private void CalculateDEM()
        {
            string filepath = txtHojddata.Text;
            CalculateConstraint(filepath, @"\\hig-ad\student\homes\gis-applikationer\upg7\heightCalculated.tif", "height");
        }

        public void BufferToRaster(string input)
        {
            string filepath = input;
            string outputRaster = @"\\hig-ad\student\homes\gis-applikationer\FinalProject\bufferRaster.tif";
            //Uri defaultGeodatabasePath = new Uri(Project.Current.DefaultGeodatabasePath);
            // Create a raster layer using a path to an image.
            // Note: You can create a raster layer from a url, project item, or data connection.
            QueuedTask.Run(() =>
            {
                // Run the Slope geoprocessing tool
                var parameters = Geoprocessing.MakeValueArray(filepath, "BUFF_DIST", outputRaster);
                var gpSlope = Geoprocessing.ExecuteToolAsync("conversion.PolygonToRaster", parameters);

                // Check if the tool executed successfully
                if (gpSlope.Result.IsFailed)
                {
                    MessageBox.Show("Buffer calculation failed.", "Error");
                    return;
                }
                MessageBox.Show("Buffer calculation completed successfully.", "Success");

                CalculateConstraint(outputRaster, @"\\hig-ad\student\homes\gis-applikationer\FinalProject\buffCalcRaster.tif","buffer");
            });
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
                    maExpression = $"Con(\"{bandnameArray[0]}\" < 200, 1, 0)";
                }
                else if (type.Equals("slope"))
                {
                    maExpression = $"Con((\"{bandnameArray[0]}\" < 30), 1, 0)";
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
                else if (type.Equals("buffer"))
                {
                    maExpression = $"Con((\"{bandnameArray[0]}\" > 1), 1, 0)";
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
        private void txtHojddata_TextChanged(object sender, TextChangedEventArgs e)
        {

        } 
        private void Slope_Click(object sender, RoutedEventArgs e)
        {
            CalculateSlope();
        }

        private void Aspect_Click(object sender, RoutedEventArgs e)
        {
            CalculateAspect();  
        }

        private void Height_Click(object sender, RoutedEventArgs e)
        {
            CalculateDEM();
        }

        private void Buffer_Click(object sender, RoutedEventArgs e)
        {
            CalculateBuffer();
        }
    }
}
