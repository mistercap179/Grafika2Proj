using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Xml;
using WpfApp1.Logic;
using WpfApp1.Model;
namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region structs
        public struct objectEES
        {
            public int x, y;
            public long id;
            public int viewPortId;
            public string name;
            public ToolTip tool;
        };

        public struct lineEES
        {
            public long id;
            public float RLine;
            public int viewPortId;
            public string Material;
            public long firstId;
            public long secondId;
            public ToolTip tool;
        };


        #endregion

        #region variables

        public Dictionary<long, objectEES> objects = new Dictionary<long, objectEES>();
        public Dictionary<long, lineEES> lines = new Dictionary<long, lineEES>();

        private Dictionary<long, GeometryModel3D> models = new Dictionary<long, GeometryModel3D>();
        private GeometryModel3D hitgeo;

        private DiffuseMaterial brush = new DiffuseMaterial();
        private DiffuseMaterial brush1 = new DiffuseMaterial();
        public long firstId;
        public long secondId;
        public long lineId;
        public long idModel_Tooltip;

        public string radioButton;

        public const int widthMap = 1175;
        public const int heightMap = 775;


        const double PI = 3.14159;
        const double angleInc = 0.001;


        public double minX = 19.793909, minY = 45.2325 // donji levi
            , maxX = 19.894459, maxY = 45.277031; // gornji desni

        private Point start = new Point(700 / 2.0, 900 / 2.0);
        public double noviX, noviY;
        public int[,] matrica = new int[235, 155];

        public List<SwitchEntity> switchEntitiesClosed = new List<SwitchEntity>();
        public List<SwitchEntity> switchEntitiesOpen = new List<SwitchEntity>();

        public List<long> ids = new List<long>();


        private bool middleMouseCaptured = false;
        private bool leftMouseCapture = false;

        private List<Point3D> conePoints = new List<Point3D>(); // xz coordinates for cone circle base

        private Point startMiddle = new Point();
        private int circleIndex = 0;

        private Point3D mapCenter = new Point3D();

        #endregion

        public MainWindow()
        {
            InitializeComponent();
            DetermineMapCenter();
            UpdateConePoints();
        }

        private void DetermineMapCenter()
        {
            /*
            O = (o1, o2, o3) - pozicija kamere
            D = (d1, d2, d3) - direkcija kamere

            (x,y,z) = (
                o1 + d1 * t,
                o2 + d2 * t,
                o3 + d3 * t )

            (0, 1, 0)

            0 = o2 + d2 * t

            t = -o2 / d2
            */
            Point3D O = cam.Position;
            Vector3D D = cam.LookDirection;

            var t = -O.Y / D.Y;

            mapCenter = new Point3D(
                O.X + D.X * t,
                0,
                O.Z + D.Z * t
            );
        }

        private void UpdateConePoints()
        {
            conePoints.Clear();

            Point3D circleCenter = mapCenter;
            circleCenter.Y += cam.Position.Y;

            var coneRadius = Math.Abs(Math.Sqrt(
                Math.Pow(cam.Position.X - circleCenter.X, 2) +
                Math.Pow(cam.Position.Z - circleCenter.Z, 2)
                ));

            for (double i = 0; i <= 2 * PI; i += angleInc)
            {
                conePoints.Add(
                    new Point3D(
                        circleCenter.X + coneRadius * Math.Cos(i),
                        cam.Position.Y,
                        circleCenter.Z + coneRadius * Math.Sin(i)
                        )
                );
            }
        }

        private void check()
        {
            for (int i = 0; i < ids.Count; i++)
            {
                ModelVisual3D Model = viewport1.Children[objects[ids[i]].viewPortId] as ModelVisual3D;
                GeometryModel3D GeoModel = Model.Content as GeometryModel3D;
                DiffuseMaterial Material = GeoModel.Material as DiffuseMaterial;
                Material.Brush = Brushes.Transparent;
            }
            
        }

        private void viewport1_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.MiddleButton == MouseButtonState.Pressed)
            {
                viewport1.CaptureMouse();
                startMiddle = e.GetPosition(this);
                middleMouseCaptured = true;
            }
        }

        private void viewport1_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.MiddleButton == MouseButtonState.Released)
            {
                viewport1.ReleaseMouseCapture();
                middleMouseCaptured = false;
            }
        }

        private void viewport1_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            viewport1.CaptureMouse();
            leftMouseCapture = true;
            start = e.GetPosition(this);
        }

        private void ChangeColorSwitch_Checked(object sender, RoutedEventArgs e)
        {
            foreach (var item in objects)
            {
                for (int i = 0; i < switchEntitiesClosed.Count; i++)
                {
                    if(switchEntitiesClosed[i].Id == item.Value.id)
                    {
                        ModelVisual3D switchClosedModel = viewport1.Children[objects[item.Value.id].viewPortId] as ModelVisual3D;
                        GeometryModel3D switchClosedGeoModel = switchClosedModel.Content as GeometryModel3D;
                        DiffuseMaterial switchClosedMaterial = switchClosedGeoModel.Material as DiffuseMaterial;
                        switchClosedMaterial.Brush = Brushes.Red;
                    }
                }
                for (int i = 0; i < switchEntitiesOpen.Count; i++)
                {
                    if (switchEntitiesOpen[i].Id == item.Value.id)
                    {
                        ModelVisual3D switchOpenModel = viewport1.Children[objects[item.Value.id].viewPortId] as ModelVisual3D;
                        GeometryModel3D switchOpenGeoModel = switchOpenModel.Content as GeometryModel3D;
                        DiffuseMaterial switchOpenMaterial = switchOpenGeoModel.Material as DiffuseMaterial;
                        switchOpenMaterial.Brush = Brushes.Green;
                    }
                }
            }
          
        }

        private void CancelClick_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < switchEntitiesClosed.Count; i++)
            {
                ModelVisual3D switchModel = viewport1.Children[objects[switchEntitiesClosed[i].Id].viewPortId] as ModelVisual3D;
                GeometryModel3D switchGeoModel = switchModel.Content as GeometryModel3D;
                DiffuseMaterial switchMaterial = switchGeoModel.Material as DiffuseMaterial;
                switchMaterial.Brush = Brushes.RosyBrown;
            }
            for (int i = 0; i < switchEntitiesOpen.Count; i++)
            {
                ModelVisual3D switchModel = viewport1.Children[objects[switchEntitiesOpen[i].Id].viewPortId] as ModelVisual3D;
                GeometryModel3D switchGeoModel = switchModel.Content as GeometryModel3D;
                DiffuseMaterial switchMaterial = switchGeoModel.Material as DiffuseMaterial;
                switchMaterial.Brush = Brushes.RosyBrown;
            }
        }

        private void changeLineC_Checked(object sender, RoutedEventArgs e)
        {
            foreach (var item in lines)
            {
                if(item.Value.RLine < 1)
                {
                    ModelVisual3D lineModel = viewport1.Children[lines[item.Value.id].viewPortId] as ModelVisual3D;
                    GeometryModel3D lineGeoModel = lineModel.Content as GeometryModel3D;
                    DiffuseMaterial lineMaterial = lineGeoModel.Material as DiffuseMaterial;
                    lineMaterial.Brush = Brushes.Red;
                }
                else if(item.Value.RLine >= 1 && item.Value.RLine <= 2)
                {
                    ModelVisual3D lineModel = viewport1.Children[lines[item.Value.id].viewPortId] as ModelVisual3D;
                    GeometryModel3D lineGeoModel = lineModel.Content as GeometryModel3D;
                    DiffuseMaterial lineMaterial = lineGeoModel.Material as DiffuseMaterial;
                    lineMaterial.Brush = Brushes.Orange;
                }
                else if (item.Value.RLine > 2)
                {
                    ModelVisual3D lineModel = viewport1.Children[lines[item.Value.id].viewPortId] as ModelVisual3D;
                    GeometryModel3D lineGeoModel = lineModel.Content as GeometryModel3D;
                    DiffuseMaterial lineMaterial = lineGeoModel.Material as DiffuseMaterial;
                    lineMaterial.Brush = Brushes.Yellow;
                }
            }


        }

        private void cancelLine_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in lines)
            {
                switch (item.Value.Material)
                {
                    case "Steel":
                        ModelVisual3D lineModel = viewport1.Children[lines[item.Value.id].viewPortId] as ModelVisual3D;
                        GeometryModel3D lineGeoModel = lineModel.Content as GeometryModel3D;
                        DiffuseMaterial lineMaterial = lineGeoModel.Material as DiffuseMaterial;
                        lineMaterial.Brush = Brushes.LightSteelBlue;
                        break;

                    case "Acsr":
                        ModelVisual3D lineModel1 = viewport1.Children[lines[item.Value.id].viewPortId] as ModelVisual3D;
                        GeometryModel3D lineGeoModel1 = lineModel1.Content as GeometryModel3D;
                        DiffuseMaterial lineMaterial1 = lineGeoModel1.Material as DiffuseMaterial;
                        lineMaterial1.Brush = Brushes.LightYellow;
                        break;

                    case "Copper":
                        ModelVisual3D lineModel2 = viewport1.Children[lines[item.Value.id].viewPortId] as ModelVisual3D;
                        GeometryModel3D lineGeoModel2 = lineModel2.Content as GeometryModel3D;
                        DiffuseMaterial lineMaterial2 = lineGeoModel2.Material as DiffuseMaterial;
                        lineMaterial2.Brush = (Brush)new BrushConverter().ConvertFromString("#b87333");
                        break;
                }
            }

        }

        private void hideLine_Checked(object sender, RoutedEventArgs e)
        {
            foreach (var item in lines)
            {
                for (int i = 0; i < switchEntitiesOpen.Count; i++)
                {
                    if(switchEntitiesOpen[i].Id == item.Value.firstId)
                    {
                        ModelVisual3D lineModel = viewport1.Children[lines[item.Value.id].viewPortId] as ModelVisual3D;
                        GeometryModel3D lineGeoModel = lineModel.Content as GeometryModel3D;
                        DiffuseMaterial lineMaterial = lineGeoModel.Material as DiffuseMaterial;
                        lineMaterial.Brush = Brushes.Transparent;
                        ids.Add(item.Value.secondId);
                    }
                }
            }
            check();
        }

        private void showLine_Checked(object sender, RoutedEventArgs e)
        {
            foreach (var item in lines)
            {
                switch (item.Value.Material)
                {
                    case "Steel":
                        ModelVisual3D lineModel = viewport1.Children[lines[item.Value.id].viewPortId] as ModelVisual3D;
                        GeometryModel3D lineGeoModel = lineModel.Content as GeometryModel3D;
                        DiffuseMaterial lineMaterial = lineGeoModel.Material as DiffuseMaterial;
                        lineMaterial.Brush = Brushes.LightSteelBlue;
                        break;

                    case "Acsr":
                        ModelVisual3D lineModel1 = viewport1.Children[lines[item.Value.id].viewPortId] as ModelVisual3D;
                        GeometryModel3D lineGeoModel1 = lineModel1.Content as GeometryModel3D;
                        DiffuseMaterial lineMaterial1 = lineGeoModel1.Material as DiffuseMaterial;
                        lineMaterial1.Brush = Brushes.LightYellow;
                        break;

                    case "Copper":
                        ModelVisual3D lineModel2 = viewport1.Children[lines[item.Value.id].viewPortId] as ModelVisual3D;
                        GeometryModel3D lineGeoModel2 = lineModel2.Content as GeometryModel3D;
                        DiffuseMaterial lineMaterial2 = lineGeoModel2.Material as DiffuseMaterial;
                        lineMaterial2.Brush = (Brush)new BrushConverter().ConvertFromString("#b87333");
                        break;
                }
            }
            for (int i = 0; i < ids.Count; i++)
            {
                if (objects[ids[i]].name == "substation")
                {
                    ModelVisual3D Model = viewport1.Children[objects[ids[i]].viewPortId] as ModelVisual3D;
                    GeometryModel3D GeoModel = Model.Content as GeometryModel3D;
                    DiffuseMaterial Material = GeoModel.Material as DiffuseMaterial;
                    Material.Brush = Brushes.Blue;
                }
                else if(objects[ids[i]].name == "switch")
                {
                    ModelVisual3D Model = viewport1.Children[objects[ids[i]].viewPortId] as ModelVisual3D;
                    GeometryModel3D GeoModel = Model.Content as GeometryModel3D;
                    DiffuseMaterial Material = GeoModel.Material as DiffuseMaterial;
                    Material.Brush = Brushes.RosyBrown;
                }
                else if(objects[ids[i]].name == "node")
                {
                    ModelVisual3D Model = viewport1.Children[objects[ids[i]].viewPortId] as ModelVisual3D;
                    GeometryModel3D GeoModel = Model.Content as GeometryModel3D;
                    DiffuseMaterial Material = GeoModel.Material as DiffuseMaterial;
                    Material.Brush = Brushes.Yellow;
                }
                    
            }
            

        }

        private void R1Hide_Checked(object sender, RoutedEventArgs e)
        {
            foreach (var item in lines)
            {
                if(item.Value.RLine >= 0 && item.Value.RLine < 1)
                {
                    ModelVisual3D lineModel = viewport1.Children[lines[item.Value.id].viewPortId] as ModelVisual3D;
                    GeometryModel3D lineGeoModel = lineModel.Content as GeometryModel3D;
                    DiffuseMaterial lineMaterial = lineGeoModel.Material as DiffuseMaterial;
                    lineMaterial.Brush = Brushes.Transparent;
                }
            }
        }

        private void R2Hide_Checked(object sender, RoutedEventArgs e)
        {
            foreach (var item in lines)
            {
                if (item.Value.RLine >= 1 && item.Value.RLine <= 2)
                {
                    ModelVisual3D lineModel = viewport1.Children[lines[item.Value.id].viewPortId] as ModelVisual3D;
                    GeometryModel3D lineGeoModel = lineModel.Content as GeometryModel3D;
                    DiffuseMaterial lineMaterial = lineGeoModel.Material as DiffuseMaterial;
                    lineMaterial.Brush = Brushes.Transparent;
                }
            }

        }

        private void R3Hide_Checked(object sender, RoutedEventArgs e)
        {
            foreach (var item in lines)
            {
                if (item.Value.RLine > 2)
                {
                    ModelVisual3D lineModel = viewport1.Children[lines[item.Value.id].viewPortId] as ModelVisual3D;
                    GeometryModel3D lineGeoModel = lineModel.Content as GeometryModel3D;
                    DiffuseMaterial lineMaterial = lineGeoModel.Material as DiffuseMaterial;
                    lineMaterial.Brush = Brushes.Transparent;
                }
            }
        }

        private void CancelHide_Checked(object sender, RoutedEventArgs e)
        {
            foreach (var item in lines)
            {
                switch (item.Value.Material)
                {
                    case "Steel":
                        ModelVisual3D lineModel = viewport1.Children[lines[item.Value.id].viewPortId] as ModelVisual3D;
                        GeometryModel3D lineGeoModel = lineModel.Content as GeometryModel3D;
                        DiffuseMaterial lineMaterial = lineGeoModel.Material as DiffuseMaterial;
                        lineMaterial.Brush = Brushes.LightSteelBlue;
                        break;

                    case "Acsr":
                        ModelVisual3D lineModel1 = viewport1.Children[lines[item.Value.id].viewPortId] as ModelVisual3D;
                        GeometryModel3D lineGeoModel1 = lineModel1.Content as GeometryModel3D;
                        DiffuseMaterial lineMaterial1 = lineGeoModel1.Material as DiffuseMaterial;
                        lineMaterial1.Brush = Brushes.LightYellow;
                        break;

                    case "Copper":
                        ModelVisual3D lineModel2 = viewport1.Children[lines[item.Value.id].viewPortId] as ModelVisual3D;
                        GeometryModel3D lineGeoModel2 = lineModel2.Content as GeometryModel3D;
                        DiffuseMaterial lineMaterial2 = lineGeoModel2.Material as DiffuseMaterial;
                        lineMaterial2.Brush = (Brush)new BrushConverter().ConvertFromString("#b87333");
                        break;
                }
            }
        }

        private void viewport1_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Point mouseposition = e.GetPosition(viewport1);
            Point3D testpoint3D = new Point3D(mouseposition.X, mouseposition.Y, 0);
            Vector3D testdirection = new Vector3D(mouseposition.X, mouseposition.Y, 10);

            PointHitTestParameters pointparams =
                     new PointHitTestParameters(mouseposition);
            RayHitTestParameters rayparams =
                     new RayHitTestParameters(testpoint3D, testdirection);

            //test for a result in the Viewport3D     
            hitgeo = null;
            VisualTreeHelper.HitTest(viewport1, null, HTResult, pointparams);
        }

        private HitTestResultBehavior HTResult(System.Windows.Media.HitTestResult rawresult)
        {

            RayHitTestResult rayResult = rawresult as RayHitTestResult;

            if (rayResult != null)
            {
                bool gasit = false;
                DiffuseMaterial material2 = new DiffuseMaterial();
                material2.Brush = Brushes.Transparent;

                foreach (var item in models)
                {
                    if ((GeometryModel3D)item.Value == rayResult.ModelHit)
                    {
                        if (lines.ContainsKey(item.Key))
                        {
                            hitgeo = (GeometryModel3D)rayResult.ModelHit;
                            gasit = true;

                            lineId = item.Key;
                            firstId = lines[lineId].firstId;
                            secondId = lines[lineId].secondId;

                            DiffuseMaterial material = new DiffuseMaterial();
                            material.Brush = Brushes.CadetBlue;

                            ModelVisual3D linemodel = viewport1.Children[lines[item.Key].viewPortId] as ModelVisual3D;
                            GeometryModel3D linegeoModel = linemodel.Content as GeometryModel3D;
                            DiffuseMaterial linematerial = linegeoModel.Material as DiffuseMaterial;

                            if (!(linematerial.Brush == material2.Brush))
                            {
                                models[firstId].Material = material;
                                models[secondId].Material = material;
                            }
                        }

                        if (objects.ContainsKey(item.Key))
                        {
                            ModelVisual3D model = viewport1.Children[objects[item.Key].viewPortId] as ModelVisual3D;
                            GeometryModel3D geoModel = model.Content as GeometryModel3D;
                            DiffuseMaterial material = geoModel.Material as DiffuseMaterial;

                            if (!(material.Brush == material2.Brush))
                            {
                                objects[item.Key].tool.IsOpen = true;
                                objects[item.Key].tool.Placement = System.Windows.Controls.Primitives.PlacementMode.MousePoint;
                                idModel_Tooltip = item.Key;
                            }
                        }
                    }
                }
                if (!gasit)
                {
                    hitgeo = null;
                }
            }

            return HitTestResultBehavior.Stop;
        }

        private void viewport1_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (objects.ContainsKey(idModel_Tooltip))
            {
                if (objects[idModel_Tooltip].tool.IsOpen == true)
                {
                    objects[idModel_Tooltip].tool.IsOpen = false;
                }

            }
            
        }

        private void viewport1_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            leftMouseCapture = false;
            viewport1.ReleaseMouseCapture();
        }

        private void viewport1_MouseMove(object sender, MouseEventArgs e)
        {
            if (viewport1.IsMouseCaptured && leftMouseCapture)
            {
                Point end = new Point(e.GetPosition(this).X - start.X, e.GetPosition(this).Y - start.Y);
                cam.Position += (cam.UpDirection * end.Y * 0.0000035 + Vector3D.CrossProduct(
                    cam.LookDirection, cam.UpDirection) * end.X * -0.000000035) * cam.Position.Z;

                // update points after pan
                UpdateConePoints();
            }
            else if (viewport1.IsMouseCaptured && middleMouseCaptured)
            {
                Vector diff = e.GetPosition(this) - startMiddle;

                circleIndex += (int)(diff.Length);
                circleIndex %= conePoints.Count;

                cam.Position = new Point3D(
                     conePoints[circleIndex].X,
                     cam.Position.Y,
                     conePoints[circleIndex].Z
                );

                cam.LookDirection = mapCenter - cam.Position;
                UpdateConePoints();
            }
        }

        private void viewport1_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (middleMouseCaptured) return;

            Point3D positon = cam.Position;
            Vector3D lookDir = cam.LookDirection;

            double xZoom, yZoom = 0, zZoom;
            xZoom = positon.X + lookDir.X * (-(positon.Y / lookDir.Y));
            zZoom = positon.Z + lookDir.Z * (-(positon.Y / lookDir.Y));

            Point3D intersection = new Point3D(xZoom, yZoom, zZoom);

            if (e.Delta > 0)
            {
                cam.Position += 0.04 * (intersection - cam.Position);
            }
            else
            {
                cam.Position -= 0.04 * (intersection - cam.Position);
            }

            // update points after zoom
            UpdateConePoints();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            #region substations

            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load("Geographic.xml");
            XmlNodeList nodeList;

            nodeList = xmlDocument.DocumentElement.SelectNodes("/NetworkModel/Substations/SubstationEntity");
            
            foreach (XmlNode node in nodeList)
            {
                SubstationEntity sub = new SubstationEntity();
                sub.Id = long.Parse(node.SelectSingleNode("Id").InnerText);
                sub.Name = node.SelectSingleNode("Name").InnerText;
                sub.X = double.Parse(node.SelectSingleNode("X").InnerText);
                sub.Y = double.Parse(node.SelectSingleNode("Y").InnerText);

                Conversions.ToLatLon(sub.X, sub.Y, 34, out noviY, out noviX);

                if (noviX > minX && noviX < maxX && noviY > minY && noviY < maxY)
                {
                    
                    double proportionX = Conversions.ProportionX(minX, maxX, noviX, widthMap);
                    double proportionY = Conversions.ProportionY(minY, maxY, noviY, heightMap);

                    MeshGeometry3D geometrySub3D = new MeshGeometry3D();

                    List<int> koordinate = MatrixPlacement.pronadjiMjesto(proportionX / 5, proportionY / 5, matrica);


                    geometrySub3D.Positions.Add(new Point3D(koordinate[0] * 0.2, matrica[koordinate[0], koordinate[1]] * 0.2, koordinate[1] * 0.2));
                    geometrySub3D.Positions.Add(new Point3D(koordinate[0] * 0.2 + 0.2, matrica[koordinate[0], koordinate[1]] * 0.2, koordinate[1] * 0.2));
                    geometrySub3D.Positions.Add(new Point3D(koordinate[0] * 0.2, 0.2 + matrica[koordinate[0], koordinate[1]] * 0.2, koordinate[1] * 0.2));
                    geometrySub3D.Positions.Add(new Point3D(koordinate[0] * 0.2 + 0.2, 0.2 + matrica[koordinate[0], koordinate[1]] * 0.2, koordinate[1] * 0.2));
                    geometrySub3D.Positions.Add(new Point3D(koordinate[0] * 0.2, matrica[koordinate[0], koordinate[1]] * 0.2, koordinate[1] * 0.2 + 0.2));
                    geometrySub3D.Positions.Add(new Point3D(koordinate[0] * 0.2 + 0.2, matrica[koordinate[0], koordinate[1]] * 0.2, koordinate[1] * 0.2 + 0.2));
                    geometrySub3D.Positions.Add(new Point3D(koordinate[0] * 0.2, 0.2 + 0.2 * matrica[koordinate[0], koordinate[1]], koordinate[1] * 0.2 + 0.2));
                    geometrySub3D.Positions.Add(new Point3D(koordinate[0] * 0.2 + 0.2, 0.2 + 0.2 * matrica[koordinate[0], koordinate[1]], koordinate[1] * 0.2 + 0.2));

                    Int32Collection tI = new Int32Collection
                    {0,2,1 , 2,3,1 , 2,4,6 , 2,0,4 , 6,5,7 , 4,5,6, 5,1,7
                    , 1,3,7 , 5,0,1 , 5,4,0 , 7,3,2 , 6,7,2 };

                    geometrySub3D.TriangleIndices = tI;

                    ToolTip toolTip = new ToolTip();

                    toolTip.Content = "Substation\nID: " + sub.Id + "  Name: " + sub.Name;
                    toolTip.Foreground = System.Windows.Media.Brushes.IndianRed;
                    toolTip.Background = System.Windows.Media.Brushes.White;

                    DiffuseMaterial material = new DiffuseMaterial { Brush = Brushes.Blue };

                    GeometryModel3D model3D = new GeometryModel3D { Geometry = geometrySub3D, Material = material };

                    models.Add(sub.Id, model3D);
                    viewport1.Children.Add(new ModelVisual3D() { Content = model3D });

                    objectEES objekat = new objectEES();

                    objekat.id = sub.Id;
                    objekat.x = koordinate[0];
                    objekat.y = koordinate[1];

                    objekat.viewPortId = viewport1.Children.Count - 1;
                    objekat.tool = toolTip;
                    objekat.name = "substation";
                    objects.Add(objekat.id, objekat);

                }
            }
            #endregion

            #region nodes
            nodeList = xmlDocument.DocumentElement.SelectNodes("/NetworkModel/Nodes/NodeEntity");

            int i = 0;
            foreach (XmlNode node in nodeList)
            {
                NodeEntity nodeobj = new NodeEntity();
                nodeobj.Id = long.Parse(node.SelectSingleNode("Id").InnerText);
                nodeobj.Name = node.SelectSingleNode("Name").InnerText;
                nodeobj.X = double.Parse(node.SelectSingleNode("X").InnerText);
                nodeobj.Y = double.Parse(node.SelectSingleNode("Y").InnerText);

                Conversions.ToLatLon(nodeobj.X, nodeobj.Y, 34, out noviY, out noviX);

                if (noviX > minX && noviX < maxX && noviY > minY && noviY < maxY)
                {
                    i++;
                    double proportionX = Conversions.ProportionX(minX, maxX, noviX, widthMap);
                    double proportionY = Conversions.ProportionY(minY, maxY, noviY, heightMap);

                    MeshGeometry3D geometrySub3D = new MeshGeometry3D();

                    List<int> koordinate = MatrixPlacement.pronadjiMjesto(proportionX / 5, proportionY / 5, matrica);

                    geometrySub3D.Positions.Add(new Point3D(koordinate[0] * 0.2, matrica[koordinate[0], koordinate[1]] * 0.2, koordinate[1] * 0.2));
                    geometrySub3D.Positions.Add(new Point3D(koordinate[0] * 0.2 + 0.2, matrica[koordinate[0], koordinate[1]] * 0.2, koordinate[1] * 0.2));
                    geometrySub3D.Positions.Add(new Point3D(koordinate[0] * 0.2, 0.2 + matrica[koordinate[0], koordinate[1]] * 0.2, koordinate[1] * 0.2));
                    geometrySub3D.Positions.Add(new Point3D(koordinate[0] * 0.2 + 0.2, 0.2 + matrica[koordinate[0], koordinate[1]] * 0.2, koordinate[1] * 0.2));
                    geometrySub3D.Positions.Add(new Point3D(koordinate[0] * 0.2, matrica[koordinate[0], koordinate[1]] * 0.2, koordinate[1] * 0.2 + 0.2));
                    geometrySub3D.Positions.Add(new Point3D(koordinate[0] * 0.2 + 0.2, matrica[koordinate[0], koordinate[1]] * 0.2, koordinate[1] * 0.2 + 0.2));
                    geometrySub3D.Positions.Add(new Point3D(koordinate[0] * 0.2, 0.2 + 0.2 * matrica[koordinate[0], koordinate[1]], koordinate[1] * 0.2 + 0.2));
                    geometrySub3D.Positions.Add(new Point3D(koordinate[0] * 0.2 + 0.2, 0.2 + 0.2 * matrica[koordinate[0], koordinate[1]], koordinate[1] * 0.2 + 0.2));


                    Int32Collection tI = new Int32Collection
                    {0,2,1 , 2,3,1 , 2,4,6 , 2,0,4 , 6,5,7 , 4,5,6, 5,1,7
                    , 1,3,7 , 5,0,1 , 5,4,0 , 7,3,2 , 6,7,2 };

                    geometrySub3D.TriangleIndices = tI;

                    ToolTip toolTip = new ToolTip();

                    toolTip.Content = "Node\nID: " + nodeobj.Id + "  Name: " + nodeobj.Name;
                    toolTip.Foreground = System.Windows.Media.Brushes.IndianRed;
                    toolTip.Background = System.Windows.Media.Brushes.White;

                    DiffuseMaterial material = new DiffuseMaterial { Brush = Brushes.Yellow };

                    GeometryModel3D model3D = new GeometryModel3D { Geometry = geometrySub3D, Material = material };

                    models.Add(nodeobj.Id, model3D);
                    viewport1.Children.Add(new ModelVisual3D() { Content = model3D });

                    objectEES objekat = new objectEES();

                    objekat.id = nodeobj.Id;
                    objekat.x = koordinate[0];
                    objekat.y = koordinate[1];

                    objekat.viewPortId = viewport1.Children.Count - 1;
                    objekat.tool = toolTip;
                    objekat.name = "node";
                    objects.Add(objekat.id, objekat);

                }
            }
            Console.WriteLine(i);
            #endregion

            #region switchers

            nodeList = xmlDocument.DocumentElement.SelectNodes("/NetworkModel/Switches/SwitchEntity");

            int j = 0;
            foreach (XmlNode node in nodeList)
            {
                SwitchEntity switchobj = new SwitchEntity();

                switchobj.Id = long.Parse(node.SelectSingleNode("Id").InnerText);
                switchobj.Name = node.SelectSingleNode("Name").InnerText;
                switchobj.X = double.Parse(node.SelectSingleNode("X").InnerText);
                switchobj.Y = double.Parse(node.SelectSingleNode("Y").InnerText);
                switchobj.Status = node.SelectSingleNode("Status").InnerText;

                

                Conversions.ToLatLon(switchobj.X, switchobj.Y, 34, out noviY, out noviX);

                if (noviX > minX && noviX < maxX && noviY > minY && noviY < maxY)
                {
                    j++;
                    if (switchobj.Status == "Closed")
                    {
                        switchEntitiesClosed.Add(switchobj);
                    }
                    else if (switchobj.Status == "Open")
                    {
                        switchEntitiesOpen.Add(switchobj);
                    }

                    double proportionX = Conversions.ProportionX(minX, maxX, noviX, widthMap);
                    double proportionY = Conversions.ProportionY(minY, maxY, noviY, heightMap);

                    MeshGeometry3D geometrySub3D = new MeshGeometry3D();

                    List<int> koordinate = MatrixPlacement.pronadjiMjesto(proportionX / 5, proportionY / 5, matrica);


                    geometrySub3D.Positions.Add(new Point3D(koordinate[0] * 0.2, matrica[koordinate[0], koordinate[1]] * 0.2, koordinate[1] * 0.2));
                    geometrySub3D.Positions.Add(new Point3D(koordinate[0] * 0.2 + 0.2, matrica[koordinate[0], koordinate[1]] * 0.2, koordinate[1] * 0.2));
                    geometrySub3D.Positions.Add(new Point3D(koordinate[0] * 0.2, 0.2 + matrica[koordinate[0], koordinate[1]] * 0.2, koordinate[1] * 0.2));
                    geometrySub3D.Positions.Add(new Point3D(koordinate[0] * 0.2 + 0.2, 0.2 + matrica[koordinate[0], koordinate[1]] * 0.2, koordinate[1] * 0.2));
                    geometrySub3D.Positions.Add(new Point3D(koordinate[0] * 0.2, matrica[koordinate[0], koordinate[1]] * 0.2, koordinate[1] * 0.2 + 0.2));
                    geometrySub3D.Positions.Add(new Point3D(koordinate[0] * 0.2 + 0.2, matrica[koordinate[0], koordinate[1]] * 0.2, koordinate[1] * 0.2 + 0.2));
                    geometrySub3D.Positions.Add(new Point3D(koordinate[0] * 0.2, 0.2 + 0.2 * matrica[koordinate[0], koordinate[1]], koordinate[1] * 0.2 + 0.2));
                    geometrySub3D.Positions.Add(new Point3D(koordinate[0] * 0.2 + 0.2, 0.2 + 0.2 * matrica[koordinate[0], koordinate[1]], koordinate[1] * 0.2 + 0.2));


                    Int32Collection tI = new Int32Collection
                {0,2,1 , 2,3,1 , 2,4,6 , 2,0,4 , 6,5,7 , 4,5,6, 5,1,7
                , 1,3,7 , 5,0,1 , 5,4,0 , 7,3,2 , 6,7,2 };

                    geometrySub3D.TriangleIndices = tI;

                    ToolTip toolTip = new ToolTip();

                    toolTip.Content = "Swtich\nID: " + switchobj.Id + "  Name: " + switchobj.Name;
                    toolTip.Foreground = System.Windows.Media.Brushes.IndianRed;
                    toolTip.Background = System.Windows.Media.Brushes.White;

                    DiffuseMaterial material = new DiffuseMaterial { Brush = Brushes.RosyBrown };

                    GeometryModel3D model3D = new GeometryModel3D { Geometry = geometrySub3D, Material = material };

                    models.Add(switchobj.Id, model3D);
                    viewport1.Children.Add(new ModelVisual3D() { Content = model3D });

                    objectEES objekat = new objectEES();

                    objekat.id = switchobj.Id;
                    objekat.x = koordinate[0];
                    objekat.y = koordinate[1];

                    objekat.viewPortId = viewport1.Children.Count - 1;
                    objekat.tool = toolTip;
                    objekat.name = "switch";
                    objects.Add(objekat.id, objekat);
                }
            }
            Console.WriteLine(j);
            #endregion
            #region lines
            nodeList = xmlDocument.DocumentElement.SelectNodes("/NetworkModel/Lines/LineEntity");
            int s = 0;
            foreach (XmlNode node in nodeList)
            {
                LineEntity l = new LineEntity();
                l.Id = long.Parse(node.SelectSingleNode("Id").InnerText);
                l.Name = node.SelectSingleNode("Name").InnerText;
                if (node.SelectSingleNode("IsUnderground").InnerText.Equals("true"))
                {
                    l.IsUnderground = true;
                }
                else
                {
                    l.IsUnderground = false;
                }
                l.R = float.Parse(node.SelectSingleNode("R").InnerText);
                l.ConductorMaterial = node.SelectSingleNode("ConductorMaterial").InnerText;
                l.LineType = node.SelectSingleNode("LineType").InnerText;
                l.ThermalConstantHeat = long.Parse(node.SelectSingleNode("ThermalConstantHeat").InnerText);
                l.FirstEnd = long.Parse(node.SelectSingleNode("FirstEnd").InnerText);
                l.SecondEnd = long.Parse(node.SelectSingleNode("SecondEnd").InnerText);

                if (objects.ContainsKey(l.FirstEnd) && objects.ContainsKey(l.SecondEnd))
                {
                    var firstEntity = objects.FirstOrDefault(x => x.Key == l.FirstEnd);
                    var secondEntity = objects.FirstOrDefault(x => x.Key == l.SecondEnd);

                    MeshGeometry3D line3D = new MeshGeometry3D();

                    line3D.Positions.Add(new Point3D(firstEntity.Value.x * 0.2 + 0.05, 0.1, firstEntity.Value.y * 0.2 + 0.2));
                    line3D.Positions.Add(new Point3D(firstEntity.Value.x * 0.2 + 0.15, 0.1, firstEntity.Value.y * 0.2 + 0.2));
                    line3D.Positions.Add(new Point3D(firstEntity.Value.x * 0.2 + 0.05, 0.2, firstEntity.Value.y * 0.2 + 0.2));
                    line3D.Positions.Add(new Point3D(firstEntity.Value.x * 0.2 + 0.15, 0.2, firstEntity.Value.y * 0.2 + 0.2));
                    Console.WriteLine(l.ConductorMaterial);
                    int br = 1;
                    s++;
                    foreach (XmlNode pointNode in node.ChildNodes[9].ChildNodes) // 9 posto je Vertices 9. node u jednom line objektu
                    {
                        Point p = new Point();

                        p.X = double.Parse(pointNode.SelectSingleNode("X").InnerText);
                        p.Y = double.Parse(pointNode.SelectSingleNode("Y").InnerText);

                        Conversions.ToLatLon(p.X, p.Y, 34, out noviY, out noviX);

                        if (noviX > minX && noviX < maxX && noviY > minY && noviY < maxY)
                        {
                            
                            double proportionX = Conversions.ProportionX(minX, maxX, noviX, widthMap);
                            double proportionY = Conversions.ProportionY(minY, maxY, noviY, heightMap);

                            List<int> koordinate = MatrixPlacement.pronadjiMjesto(proportionX / 5, proportionY / 5, matrica);

                            line3D.Positions.Add(new Point3D(koordinate[0] * 0.2 + 0.05, 0.1, koordinate[1] * 0.2 + 0.2));
                            line3D.Positions.Add(new Point3D(koordinate[0] * 0.2 + 0.15, 0.1, koordinate[1] * 0.2 + 0.2));
                            line3D.Positions.Add(new Point3D(koordinate[0] * 0.2 + 0.05, 0.2, koordinate[1] * 0.2 + 0.2));
                            line3D.Positions.Add(new Point3D(koordinate[0] * 0.2 + 0.15, 0.2, koordinate[1] * 0.2 + 0.2));

                            Int32Collection linesTI = new Int32Collection
                        {
                            4*br,4*br+1,4*br-3,
                            4*br,4*br-3,4*br-4,
                            4*br-1,4*br-3,4*br+1,
                            4*br-1,4*br+1,4*br+3,
                            4*br-2,4*br-1,4*br+2,
                            4*br+2,4*br-1,4*br+3,
                            4*br-3,4*br+1,4*br-4,
                            4*br-4,4*br+1,4*br,
                            4*br+3,4*br+1,4*br-3,
                            4*br+3,4*br-3,4*br-1,
                            4*br+3,4*br-1,4*br-2,
                            4*br+3,4*br-2,4*br+2
                        };

                            foreach (var item in linesTI)
                            {
                                line3D.TriangleIndices.Add(item);
                            }

                            br++;

                        }

                    }

                    line3D.Positions.Add(new Point3D(secondEntity.Value.x * 0.2 + 0.05, 0.1, secondEntity.Value.y * 0.2 + 0.2));
                    line3D.Positions.Add(new Point3D(secondEntity.Value.x * 0.2 + 0.15, 0.1, secondEntity.Value.y * 0.2 + 0.2));
                    line3D.Positions.Add(new Point3D(secondEntity.Value.x * 0.2 + 0.05, 0.2, secondEntity.Value.y * 0.2 + 0.2));
                    line3D.Positions.Add(new Point3D(secondEntity.Value.x * 0.2 + 0.15, 0.2, secondEntity.Value.y * 0.2 + 0.2));

                    Int32Collection linesTIEnd = new Int32Collection
                {
                            4*br,4*br+1,4*br-3,
                            4*br,4*br-3,4*br-4,
                            4*br-1,4*br-3,4*br+1,
                            4*br-1,4*br+1,4*br+3,
                            4*br-2,4*br-1,4*br+2,
                            4*br+2,4*br-1,4*br+3,
                            4*br-3,4*br+1,4*br-4,
                            4*br-4,4*br+1,4*br,
                            4*br+3,4*br+1,4*br-3,
                            4*br+3,4*br-3,4*br-1,
                            4*br+3,4*br-1,4*br-2,
                            4*br+3,4*br-2,4*br+2
                };

                    foreach (var item in linesTIEnd)
                    {
                        line3D.TriangleIndices.Add(item);
                    }

                    DiffuseMaterial material = new DiffuseMaterial();

                    switch (l.ConductorMaterial)
                    {
                        case "Steel":
                            material.Brush = Brushes.LightSteelBlue;
                            break;

                        case "Acsr":
                            material.Brush = Brushes.LightYellow;
                            break;

                        case "Copper":
                            material.Brush = (Brush)new BrushConverter().ConvertFromString("#b87333");
                            break;
                    }

                    ToolTip toolTip = new ToolTip();

                    toolTip.Content = "Line\nID: " + l.Id + "  Name: " + l.Name;
                    toolTip.Foreground = System.Windows.Media.Brushes.IndianRed;
                    toolTip.Background = System.Windows.Media.Brushes.White;

                    GeometryModel3D model3D = new GeometryModel3D { Geometry = line3D, Material = material };

                    models.Add(l.Id, model3D);
                    viewport1.Children.Add(new ModelVisual3D() { Content = model3D });

                    lineEES objekat = new lineEES();

                    objekat.id = l.Id;
                    objekat.viewPortId = viewport1.Children.Count - 1;
                    objekat.RLine = l.R;
                    objekat.Material = l.ConductorMaterial;
                    objekat.firstId = l.FirstEnd;
                    objekat.secondId = l.SecondEnd;
                    
                    objekat.tool = toolTip;
                    lines.Add(objekat.id, objekat);

                }

            }
            Console.WriteLine(s);
            #endregion

        }
    }
}
