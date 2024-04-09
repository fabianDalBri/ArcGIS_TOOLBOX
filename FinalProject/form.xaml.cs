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
        string path = @"\\hig-ad\student\homes\gis-applikationer\FinalProject\data\";

        public form()
        {
            InitializeComponent();
            //first push
        }

        public static class Global
        {
            public static string type = "";
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
            string outputSlope = path + "slope.tif";
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
                CalculateConstraint(outputSlope, path + "slopeCalculated.tif", "slope");
            });
        }
        private void CalculateDEM()
        {
            string filepath = txtHojddata.Text;
            CalculateConstraint(filepath,path + "heightCalculated.tif", "height");
        }
        private void CalculateAspect()
        {
            string filepath = txtHojddata.Text;
            string outputAspect = path + "aspect.tif";
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
                CalculateConstraint(outputAspect, path + "directionCalculated.tif", "direction");
            });
        }
        public void CalculateBuffer()
        {
            string filepath = txtVagData.Text;
            string outputbuff = path + "buffer.shp";
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
                    EraseBuffer(outputbuff);
            });

        }

        private void EraseBuffer(string input)
        {
            string rutnatpath = path + "rutnat.shp";
            string bufferpath = input;
            string output = path + "mergedBuffer.shp";
            //Uri defaultGeodatabasePath = new Uri(Project.Current.DefaultGeodatabasePath);
            // Create a raster layer using a path to an image.
            // Note: You can create a raster layer from a url, project item, or data connection.
            QueuedTask.Run(() =>
            {
                // Run the Slope geoprocessing tool
                var parameters = Geoprocessing.MakeValueArray(rutnatpath, bufferpath, output);
                var gpSlope = Geoprocessing.ExecuteToolAsync("Analysis.erase", parameters);
                // Check if the tool executed successfully
                if (gpSlope.Result.IsFailed)
                {
                    MessageBox.Show("erase calculation failed.", "Error");
                    return;
                }
                MessageBox.Show("erase calculation completed successfully.", "Success");
                BufferToRaster(output);
            });
        }

        public void BufferToRaster(string input)
        {
            string filepath = input;
            string outputRaster = path + "bufferRaster.tif";
            //Uri defaultGeodatabasePath = new Uri(Project.Current.DefaultGeodatabasePath);
            // Create a raster layer using a path to an image.
            // Note: You can create a raster layer from a url, project item, or data connection.
            QueuedTask.Run(() =>
            {
                // Run the Slope geoprocessing tool
                var parameters = Geoprocessing.MakeValueArray(filepath, null, outputRaster);
                var gpSlope = Geoprocessing.ExecuteToolAsync("conversion.PolygonToRaster", parameters);

                // Check if the tool executed successfully
                if (gpSlope.Result.IsFailed)
                {
                    MessageBox.Show("Buffer to raster calculation failed.", "Error");
                    return;
                }
                MessageBox.Show("Buffer to raster calculation completed successfully.", "Success");
                if (Global.type.Equals("roads"))
                {
                    CalculateConstraint(outputRaster, path + "buffCalcRasterRoads.tif", "bufferedRoads");
                }
                else if (Global.type.Equals("water"))
                {
                    CalculateConstraint(outputRaster, path + "buffCalcRasterWater.tif", "buffered");
                }
                else if (Global.type.Equals("rivers"))
                {
                    CalculateConstraint(outputRaster, path + "buffCalcRasterRivers.tif", "buffered");
                }
                else if (Global.type.Equals("buildings"))
                {
                    CalculateConstraint(outputRaster, path + "buffCalcRasterBuildings.tif", "buffered");
                }

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
                else if (type.Equals("direction"))
                {
                    maExpression = $"Con((\"{bandnameArray[0]}\" > 112.5) & (\"{bandnameArray[0]}\" < 360), 0, 1)";
                }
                else if (type.Equals("bufferedRoads"))
                {
                    maExpression = $"Con((IsNull(\"{bandnameArray[0]}\")), 1, 0)";
                }
                else if (type.Equals("buffered"))
                {
                    maExpression = $"Con((IsNull(\"{bandnameArray[0]}\")), 0, 1)";
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

        private void createMCA()
        {
            string slope = path + "slopeCalculated.tif";
            string height = path + "heightCalculated.tif";
            string direction = path + "directionCalculated.tif";
            string bufferRivers = path + "buffCalcRasterRivers.tif";
            string bufferWater = path + "buffCalcRasterWater.tif";
            string bufferRoads = path + "buffCalcRasterRoads.tif";
            string bufferBuildings = path + "buffCalcRasterBuildings.tif";
            string windData = path + "VindRasterUpdated.tif";



            string maExpression = $"Int(\"{slope}\") * Int(\"{height}\") * Int(\"{direction}\") * Int(\"{bufferRivers}\") * Int(\"{bufferRoads}\") * Int(\"{bufferBuildings}\") * Int(\"{bufferWater}\") * Int(\"{windData}\")";
            string output = path + "MCA.tif";

            MessageBox.Show(maExpression);  

            var valueArray = Geoprocessing.MakeValueArray(maExpression, output);
            var gpTool = Geoprocessing.ExecuteToolAsync("RasterCalculator_sa", valueArray);

            if (gpTool.Result.IsFailed)
            {
                // Get the tool errors
                var errors = gpTool.Result.ErrorMessages.Select(err => err.Text);
                // Display errors
                MessageBox.Show("Error executing MCA calculator: " + string.Join(Environment.NewLine, errors));
            }
            else
            {
                MessageBox.Show("MCA Calculator execution successful");
                RastertoPolygon();
            }
        }
        private void RastertoPolygon()
        {
            Map map = MapView.Active.Map;
            var firstRasterLayer = MapView.Active.Map.GetLayersAsFlattenedList()[0] as RasterLayer;
            // Specify the path to the input raster dataset
            var uri = firstRasterLayer.GetPath();
            // Convert the Uri.LocalPath to string
            string rasterPath = uri.LocalPath;

            MessageBox.Show(rasterPath);
            string outputShapefilePath = path + "finalMCApolygon.shp";
            MessageBox.Show(outputShapefilePath);
            // Perform the conversion
            QueuedTask.Run(() =>
            {
                // Convert raster to polygon
                var parameters = Geoprocessing.MakeValueArray(rasterPath, outputShapefilePath);
                var gpResult = Geoprocessing.ExecuteToolAsync("RasterToPolygon_conversion", parameters,
               null).Result;
            });
        }
        private void txtHojddata_TextChanged(object sender, TextChangedEventArgs e)
        {

        } 
        private void Slope_Click(object sender, RoutedEventArgs e)
        {
            CalculateSlope();
            txtSlope.Text = "Slope";
        }

        private void Aspect_Click(object sender, RoutedEventArgs e)
        {
            CalculateAspect();
            txtAspect.Text = "Aspect";
        }

        private void Height_Click(object sender, RoutedEventArgs e)
        {
            CalculateDEM();
            txtHeight.Text = "Height";
        }

        private void Buffer_Click(object sender, RoutedEventArgs e)
        {
            checkRadio();
            CalculateBuffer();
            if (Global.type == "roads")
            {
               txtRoads.Text = "Road";
            }else if (Global.type == "water")
            {
               txtWater.Text = "Water";
            }else if(Global.type == "rivers")
            {
               txtRivers.Text = "River";
            }else if(Global.type == "buildings")
            {
               txtBuildings.Text = "Building";
            }
        }
        public void checkRadio()
        {
            if (roads.IsChecked == true)
            {
                Global.type = "roads";
            }
            else if (water.IsChecked == true)
            {
                Global.type = "water";
            }
            else if (rivers.IsChecked == true)
            {
                Global.type = "rivers";
            }
            else if (buildings.IsChecked == true)
            {
                Global.type = "buildings";
            }
        }
        private void MCA_Click(object sender, RoutedEventArgs e)
        {
            createMCA();
        }

    }
}
