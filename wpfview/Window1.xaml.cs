using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading;
using System.Text;
using System.Xml;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Media3D;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Markup;
using System.IO;
using System.ComponentModel;
using System.Windows.Xps;
using System.Windows.Xps.Packaging;
using System.Printing;
using Microsoft.Win32;

using sd;
using sdwpf;

namespace wpfview
{
    public partial class Window1 : Window
    {
        /// <summary>
        /// On mouse click, select the specific board 
        /// where the click happened.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        public void OnViewportMouseDown(
            object sender,
            System.Windows.Input.MouseEventArgs args)
        {
            if (vstuff.models == null)
            {
                return;
            }

            if (
                Keyboard.IsKeyDown(Key.LeftCtrl)
                || Keyboard.IsKeyDown(Key.RightCtrl)
                )
            {
                // extending the selection.  
                // don't unselect all first.
            }
            else
            {
                UnselectAll();
            }

            RayMeshGeometry3DHitTestResult rayMeshResult =
                (RayMeshGeometry3DHitTestResult)
                  VisualTreeHelper.HitTest(myVP, args.GetPosition(myVP));

            if (rayMeshResult != null)
            {
                PartialModel found = null;
                foreach (PartialModel pm in vstuff.models)
                {
                    if (pm.mesh == rayMeshResult.MeshHit)
                    {
                        found = pm;
                        break;
                    }
                }

                if (found != null)
                {
                    if (IsSelected(found.bag.solid))
                    {
                        Unselect(found.bag.solid);
                    }
                    else
                    {
                        Select(found.bag.solid);
                    }
                }
            }
        }

        private void UpdateTransparencies()
        {
            foreach (string s in vstuff.transparencies)
            {
                Solid sol = GetCurrentStep().Result.FindSub(s);
                if (sol != null)
                {
                    MakeTransparent(sol);
                }
            }
        }

        private void MakeTransparent(Solid s)
        {
            if (!vstuff.transparencies.Contains(s.name))
            {
                vstuff.transparencies.Add(s.name);
            }

            foreach (PartialModel pm in vstuff.models)
            {
                if (pm.bag.solid == s)
                {
                    DiffuseMaterial mat = (DiffuseMaterial)pm.gm3d.Material;
                    SolidColorBrush br = (SolidColorBrush)mat.Brush;
                    br.Opacity = wpfmisc.TRANSPARENT_OPACITY_VALUE;
                    vstuff.m3dgrp.Children.Remove(pm.gm3d);
                    vstuff.m3dgrp.Children.Add(pm.gm3d);
                }
            }
        }

        private void MakeOpaque(Solid s)
        {
            if (vstuff.transparencies.Contains(s.name))
            {
                vstuff.transparencies.Remove(s.name);
            }

            foreach (PartialModel pm in vstuff.models)
            {
                if (pm.bag.solid == s)
                {
                    DiffuseMaterial mat = (DiffuseMaterial)pm.gm3d.Material;
                    SolidColorBrush br = (SolidColorBrush)mat.Brush;
                    br.Opacity = 1;
                    // TODO we *could* move this gm3d to the beginning
                }
            }
        }

        private bool IsSelected(Solid s)
        {
            foreach (MaterialsListItem lvi in myPieces.SelectedItems)
            {
                if (s == lvi.sol)
                {
                    return true;
                }
            }
            return false;
        }

        private void UnselectAll()
        {
            myPieces.SelectedItems.Clear();
        }

        private void Select(Solid s)
        {
            foreach (MaterialsListItem lvi in myPieces.Items)
            {
                Solid q = lvi.sol;
                if (q == s)
                {
                    myPieces.SelectedItems.Add(lvi);
                    myPieces.ScrollIntoView(lvi);
                    break;
                }
            }
        }

        private void Unselect(Solid s)
        {
            foreach (MaterialsListItem lvi in myPieces.Items)
            {
                Solid q = lvi.sol;
                if (q == s)
                {
                    myPieces.SelectedItems.Remove(lvi);
                    break;
                }
            }
        }

        public void OnViewportMouseUp(object sender, System.Windows.Input.MouseEventArgs args)
        {
        }

        public void OnViewportMouseMove(object sender, System.Windows.Input.MouseEventArgs args)
        {
            if (vstuff.models == null)
            {
                return;
            }

            RayMeshGeometry3DHitTestResult rayMeshResult =
                (RayMeshGeometry3DHitTestResult)
                VisualTreeHelper.HitTest(myVP, args.GetPosition(myVP));

            if (rayMeshResult != null)
            {
                PartialModel found = null;
                foreach (PartialModel pm in vstuff.models)
                {
                    if (pm.mesh == rayMeshResult.MeshHit)
                    {
                        found = pm;
                        break;
                    }
                }

                if (found != null)
                {
                    int ndx = rayMeshResult.VertexIndex1;
                    int trindx = ndx / 3;
                    Triangle3d t = found.tris[trindx];
#if DEBUG
                    myHoverFaceInfo.Content = string.Format("Pt: {3} -- {0}.{1}: {2}", t.face.solid.name, t.face.name, t.face.GetQuality(), ptstring(rayMeshResult.PointHit));
#else
                        myHoverFaceInfo.Content = string.Format("{0}.{1}: {2}", t.face.solid.name, t.face.name, t.face.GetQuality());
#endif
                }
            }
        }

        private string ptstring(Point3D p)
        {
            return string.Format("({0:0.0}, {1:0.0}, {2:0.0})", p.X, p.Y, p.Z);
        }

        public object MakeListViewPicture(Step st)
        {
            ViewMaker vo = new ViewMaker();
            vo.bShowHighlights = true;
            vo.bShowLines = true;
            vo.bShowEndGrain = true;
            vo.bShowFaceLabels = false;
            vo.bShowAnnotations = false;
            vo.bAnimate = false;
            vo.bAutoZoom = st.bDefaultZoom;
            vo.width = 200;
            vo.height = 120;

            vo.MakeView(st);

            vo.vp.HorizontalAlignment = HorizontalAlignment.Left;

            Label lab = new Label();
            lab.Content = st.Description;
            if (st.Errors.Count > 0)
            {
                lab.Foreground = Brushes.Red;
                lab.ToolTip = st.Errors[0];
            }
            else if (st.Warnings.Count > 0)
            {
                lab.Foreground = Brushes.Yellow;
                lab.ToolTip = st.Warnings[0];
            }
            else
            {
            }

            StackPanel sp = new StackPanel();
            sp.Tag = vo;
            sp.Children.Add(lab);
            sp.Children.Add(vo.vp);

            return sp;
        }

        public ListViewItem MakeListViewItem(Step st)
        {
            ListViewItem lvi = new ListViewItem();
            lvi.Content = MakeListViewPicture(st);
            lvi.Tag = st;
            return lvi;
        }

        Plan myPlan;

        bool bAutoAnimate = true;

        public class VarSlider
        {
            public VariableDefinition vd;
            public Label label;
            private Slider _slider;
            private double myValue;
            public TabItem tab;
            private bool dirty;

            private void setdirty(bool b)
            {
                dirty = b;
                if (dirty)
                {
                    tab.Header = vd.name.Replace('_', ' ') + "*";
                }
                else
                {
                    tab.Header = vd.name.Replace('_', ' ');
                }
            }

            public Slider slider
            {
                set
                {
                    _slider = value;
                    myValue = _slider.Value;
                }
            }

            internal void apply()
            {
                if (dirty)
                {
                    vd.Value = myValue = _slider.Value;
                    setdirty(false);
                }
            }

            internal void changed()
            {
                double d = vd.RoundToPrecision(_slider.Value);
                label.Content = utpub.FormatDimension(d);
                if (!dirty)
                {
                    setdirty(true);
                }
            }

            internal void cancel()
            {
                if (dirty)
                {
                    _slider.Value = myValue;
                    setdirty(false);
                }
            }
        }

        private List<VarSlider> varsliders = new List<VarSlider>();

        public class MaterialsListItem
        {
            private string name;
            private string material;
            private double boardfeet;
            private string dimensions;
            public Solid sol;

            public string Name
            {
                get { return name; }
                //set { name = value; }
            }

            public string Material
            {
                get { return material; }
                //set { material = value; }
            }

            public double BoardFeet
            {
                get { return boardfeet; }
                //set { boardfeet = value; }
            }

            public string Dimensions
            {
                get { return dimensions; }
            }

            public MaterialsListItem(Solid s, string _name, string _mat, double _bf, string _dim)
            {
                sol = s;
                this.name = _name;
                this.material = _mat;
                this.boardfeet = _bf;
                this.dimensions = _dim;
            }
        }

        private ObservableCollection<MaterialsListItem> materials = new ObservableCollection<MaterialsListItem>();

        public Window1()
        {
            InitializeComponent();

            vstuff.vp = myVP;
            vstuff.bAnimate = true;
            //vstuff.bShowAxes = true;

            myPieces.ItemsSource = materials;
        }

        // Declare scene objects.

        ViewMaker vstuff = new ViewMaker();

        private void FixLines()
        {
#if true
            wpfmisc.FixLines(vstuff.vp);
            ListViewItem lvi = mySteps.SelectedItem as ListViewItem;
            if (lvi != null)
            {
                StackPanel sp = lvi.Content as StackPanel;
                ViewMaker vo = sp.Tag as ViewMaker;
                wpfmisc.FixLines(vo.vp);
            }
#endif
        }

        public void ShowStep(Step st)
        {
            if (st != null)
            {
                if (st.Result != null)
                {
                    vstuff.MakeView(st);

                    slider_y.Value = vstuff.tran.RotY;
                    slider_x.Value = vstuff.tran.RotX;
                    slider_z.Value = vstuff.tran.RotZ;
                    slider_zoom.Value = vstuff.tran.Zoom;

                    wpfmisc.FixLines(vstuff.vp);

                    EnableAnimationControls(vstuff.bunch.moving != null);

                    myWeightInfo.Content = string.Format("Weight: {0:0.0} pounds", st.Result.Weight());

#if not
                    int c = 0;
                    foreach (PartialModel pm in vstuff.models)
                    {
                        c += pm.tris.Count;
                    }
                    myWeightInfo.Content = string.Format("{0} triangles", c);
#endif

                    List<Solid> plist = new List<Solid>();
                    plist.AddRange(st.Result.Subs);
                    //plist.Sort(new CompareSolidsByName());

                    List<Step> newboards = st.plan.FindAllNewBoards();

                    this.materials.Clear();
                    foreach (Solid s in plist)
                    {
                        Step origstep = null;
                        foreach (Step otherstep in newboards)
                        {
                            if (s.name == otherstep.Get_String("newname"))
                            {
                                origstep = otherstep;
                                break;
                            }
                        }
                        double length = origstep.Get_Eval("length");
                        double width = origstep.Get_Eval("width");
                        double thickness = origstep.Get_Eval("thickness");

                        double bf = width * length * thickness / 144.0;

                        string dim = string.Format("{0} x {1} x {2}", length, width, thickness);

                        bf = (((int)(bf * 10)) / 10.0);

                        MaterialsListItem mi = new MaterialsListItem(s, s.name, s.material.GetProse(), bf, dim);
                        materials.Add(mi);
                    }
                }

                myTitle.Content = st.Description;

                StuffLinks(st);

                // TODO try/catch around this?
                if (
                    (st.Notes != null)
                    && (st.Notes.Length > 0)
                    )
                {
                    try
                    {
                        myNotes.Document = XamlReader.Load(new XmlTextReader(new StringReader(st.Notes))) as FlowDocument;
                        myNotes.Document.FontFamily = new FontFamily("Arial");
                        myNotes.Document.FontSize = 12;
                    }
                    catch
                    {
                        myNotes.Document = null;
                    }
                }
                else
                {
                    myNotes.Document = null;
                }

                myInstructions.Text = st.GetProse();
            }
            else
            {
                vstuff.vp.Children.Clear();
                vstuff.models = null;
                vstuff.movers = null;
                vstuff.m3dgrp = null;

                myTitle.Content = "";

                StuffLinks(null);

                myWeightInfo.Content = "";

                myNotes.Document = null;
                myInstructions.Text = "";
            }
        }

        Storyboard myStoryboard;

        void start_animation()
        {
            if (myStoryboard == null)
            {
                myStoryboard = new Storyboard();
                DoubleAnimation da = new DoubleAnimation(0, 10, new Duration(new TimeSpan(0, 0, 2)));
                da.AutoReverse = true;
                da.RepeatBehavior = RepeatBehavior.Forever;
                myStoryboard.Children.Add(da);
                NameScope.SetNameScope(this, new NameScope());
                this.RegisterName("whyistherumalwaysgone", mySlider);
                Storyboard.SetTargetName(da, "whyistherumalwaysgone");
                Storyboard.SetTargetProperty(da, new PropertyPath(Slider.ValueProperty));
                myStoryboard.Begin(this, true);
            }
        }

        void stop_animation()
        {
            if (myStoryboard != null)
            {
                myStoryboard.Stop(this);
                myStoryboard = null;
            }
        }

        private void EnableAnimationControls(bool p)
        {
            tbAnimate.IsEnabled = p;

            if (p)
            {
                if (bAutoAnimate)
                {
                    start_animation();
                }
            }
            else
            {
                stop_animation();
            }
        }

        void OnClick_Stop(object sender, RoutedEventArgs args)
        {
            bAutoAnimate = false;
            stop_animation();
        }

        void OnClick_Go(object sender, RoutedEventArgs args)
        {
            bAutoAnimate = true;
            start_animation();
        }

        public Hyperlink MakeLink(string text, string target)
        {
            // TODO tooltip?

            Hyperlink link = new Hyperlink(new Run(text));
            link.NavigateUri = new Uri(target);
            link.Click += new RoutedEventHandler(OpenLinkInBrowser);
            return link;
        }

        public void StuffLinks(Step st)
        {
#if false // TODO links
            MyLinks.Items.Clear();

            if (st != null)
            {
                List<WebLinkInfo> links = st.GetLinks();

                foreach (WebLinkInfo li in links)
                {
                    MyLinks.Items.Add(MakeLink(li.text, li.target));
                }
            }
#endif
        }

        void OnActivated_MainWindow(object sender, EventArgs args)
        {
            // TODO should we FixLines here?
        }

        void OnLoaded_MainWindow(object sender, RoutedEventArgs args)
        {
            //myPlan = Builtin_Plans.CreateNewFamilyRoomShelf();
            myPlan = Builtin_Plans.CreateWorkbench();
            //myPlan = test_plan.CreateTenonInFaceWithHole();
            //myPlan = Builtin_Plans.CreateBookShelf();
            //myPlan = Builtin_Plans.CreateTable();
            //myPlan = test_plan.CreateMortiseInPlywood();
            //myPlan = test_plan.CreateNonManifold();
            //myPlan = test_plan.CreateCubeWithSmallHole();
            //myPlan = test_plan.CreateGlueTestPlan();
            //myPlan = test_plan.CreateSimpleTenonForShadow();
            //myPlan = test_plan.CreateTenonInFaceWithHoleBool3dBug();
            //myPlan = Builtin_Plans.CreateThreeMortisesInACube();
            //myPlan = test_plan.CreateTenonBug();
            //myPlan = test_plan.create_two_mortises_overlapping();
            //myPlan = test_plan.CreateWBTopProblem();
            //myPlan = test_plan.CreateTwoDrilledHolesInACube();
            //myPlan = Builtin_Plans.DrillHoleProblem();
            //myPlan = Builtin_Plans.CreateOneBoard();
            //myPlan = Builtin_Plans.CreateMiteredBoard();
            //myPlan = Builtin_Plans.CreateSpecifiedDovetail();
            //myPlan = Builtin_Plans.CreateDoubleDovetail();
            //myPlan = test_plan.CreateCubeIntoHole_Impossible();
            //myPlan = Builtin_Plans.CreateTestBlocks();

#if false
            StringBuilder sb = new StringBuilder();
            myPlan.WriteXML(sb);
            string xml = sb.ToString();

            XmlTextReader xr = new XmlTextReader(new StringReader(xml));
            XmlDocument xd = new XmlDocument();
            xd.Load(xr);
            xr.Close();

            myPlan = Plan.FromXML(xd);
#endif

            Stopwatch sw = new Stopwatch();
            sw.Start();
            myPlan.Execute();
            sw.Stop();
            this.Title = string.Format("Sawdust ({0} seconds)", sw.ElapsedMilliseconds / 1000.0);

            // put variables into the tabs
            foreach (VariableDefinition vd in myPlan.Variables.Values)
            {
                if (vd.user)
                {
                    VarSlider vs = new VarSlider();

                    vs.vd = vd;

                    TabItem ti = new TabItem();
                    ti.Header = vd.name.Replace('_', ' ');

                    vs.tab = ti;

                    Label labMin = new Label();
                    labMin.Content = vd.min.ToString();
                    labMin.HorizontalContentAlignment = HorizontalAlignment.Left;
                    labMin.FontSize = 9;

                    Label labMax = new Label();
                    labMax.Content = vd.max.ToString();
                    labMax.HorizontalContentAlignment = HorizontalAlignment.Right;
                    labMax.FontSize = 9;

                    Label labValue = new Label();
                    labValue.Tag = vd;
                    labValue.Content = utpub.FormatDimension(vd.Value);
                    labValue.Width = 170;
                    labValue.HorizontalContentAlignment = HorizontalAlignment.Center;
                    labValue.FontSize = 14;

                    vs.label = labValue;

                    TextBox labHelp = new TextBox();
                    labHelp.Text = vd.help;
                    labHelp.TextWrapping = TextWrapping.Wrap;
                    labHelp.IsReadOnly = true;

                    Slider slid = new Slider();
                    // TODO tickmarks?
                    slid.Minimum = vd.min;
                    slid.Maximum = vd.max;
                    slid.Value = vd.Value;
                    slid.ValueChanged += OnVariableChange;
                    slid.Tag = vs;

                    vs.slider = slid;

                    StackPanel splabs = new StackPanel();
                    splabs.Orientation = Orientation.Horizontal;
                    splabs.Children.Add(labMin);
                    splabs.Children.Add(labValue);
                    splabs.Children.Add(labMax);

                    StackPanel sp = new StackPanel();
                    sp.Orientation = Orientation.Vertical;
                    sp.Children.Add(splabs);
                    sp.Children.Add(slid);
                    sp.Children.Add(labHelp);

                    ti.Content = sp;
                    myVariables.Items.Add(ti);

                    varsliders.Add(vs);
                }
            }

            ComboBoxItem cbiAll = new ComboBoxItem();
            cbiAll.Content = "Show all steps";
            cbiAll.Tag = "__all";
            cbSteps.Items.Add(cbiAll);

            cbSteps.SelectedIndex = 0;

            ComboBoxItem cbiNotes = new ComboBoxItem();
            cbiNotes.Content = "Show steps with designer notes";
            cbiNotes.Tag = "__notes";
            cbSteps.Items.Add(cbiNotes);

            Dictionary<sd.Action, int> ax = new Dictionary<sd.Action, int>();
            foreach (Step st in myPlan.Steps)
            {
                if (!ax.ContainsKey(st.action))
                {
                    ax[st.action] = 1;
                }
            }

            foreach (sd.Action a in ax.Keys)
            {
                if (a != sd.Action.INTRO)
                {
                    ComboBoxItem cbi = new ComboBoxItem();
                    cbi.Content = string.Format("Show {0} steps", Step.GetActionString(a));
                    cbi.Tag = a.ToString();
                    cbSteps.Items.Add(cbi);
                }
            }
        }

        void OnSizeChanged_Viewport(object sender, RoutedEventArgs args)
        {
            FixLines();
        }

        void Slider_Changed(object sender, RoutedEventArgs args)
        {
            Slider s = (Slider)sender;

            double dist = s.Value;

            foreach (VectorTranslateTransform3D vtt in vstuff.movers)
            {
                vtt.Distance = dist;
                FixLines();
            }
        }

        void slider_y_changed(object sender, RoutedEventArgs args)
        {
            if (vstuff.tran != null)
            {
                vstuff.tran.RotY = slider_y.Value;
                FixLines();
                label_slider_y.Text = string.Format("Y Rotation: {0:0.0}", vstuff.tran.RotY);
            }
        }

        void slider_x_changed(object sender, RoutedEventArgs args)
        {
            if (vstuff.tran != null)
            {
                vstuff.tran.RotX = slider_x.Value;
                FixLines();
                label_slider_x.Text = string.Format("X Rotation: {0:0.0}", vstuff.tran.RotX);
            }
        }

        void slider_z_changed(object sender, RoutedEventArgs args)
        {
            if (vstuff.tran != null)
            {
                vstuff.tran.RotZ = slider_z.Value;
                FixLines();
                label_slider_z.Text = string.Format("Z Rotation: {0:0.0}", vstuff.tran.RotZ);
            }
        }

        void slider_zoom_changed(object sender, RoutedEventArgs args)
        {
            if (vstuff.tran != null)
            {
                vstuff.tran.Zoom = slider_zoom.Value;
                FixLines();
                label_slider_zoom.Text = string.Format("Zoom: {0:0.0}", vstuff.tran.Zoom);
            }
        }

        void Endgrain_Checked(object sender, RoutedEventArgs args)
        {
            vstuff.bShowEndGrain = true;
            refresh();
        }

        void Endgrain_Unchecked(object sender, RoutedEventArgs args)
        {
            vstuff.bShowEndGrain = false;
            refresh();
        }

        void Highlights_Checked(object sender, RoutedEventArgs args)
        {
            vstuff.bShowHighlights = true;
            refresh();
        }

        void Highlights_Unchecked(object sender, RoutedEventArgs args)
        {
            vstuff.bShowHighlights = false;
            refresh();
        }

        void Annotations_Checked(object sender, RoutedEventArgs args)
        {
            vstuff.bShowAnnotations = true;
            refresh();
        }

        void Annotations_Unchecked(object sender, RoutedEventArgs args)
        {
            vstuff.bShowAnnotations = false;
            refresh();
        }

        void FaceLabels_Checked(object sender, RoutedEventArgs args)
        {
            vstuff.bShowFaceLabels = true;
            refresh();
        }

        void FaceLabels_Unchecked(object sender, RoutedEventArgs args)
        {
            vstuff.bShowFaceLabels = false;
            refresh();
        }

        void EdgeLines_Checked(object sender, RoutedEventArgs args)
        {
            vstuff.bShowLines = true;
            refresh();
        }

        void EdgeLines_Unchecked(object sender, RoutedEventArgs args)
        {
            vstuff.bShowLines = false;
            refresh();
        }

        void OnClick_Document(object sender, RoutedEventArgs args)
        {
            Plan p = GetCurrentStep().plan;

            SaveFileDialog dlg = new SaveFileDialog();
            dlg.FileName = p.name; // Default file name
            dlg.DefaultExt = ".xps"; // Default file extension
            dlg.Filter = "XPS Documents (.xps)|*.xps"; // Filter files by extension
            dlg.AddExtension = true;
            dlg.CheckFileExists = false;
            dlg.CreatePrompt = false;
            dlg.OverwritePrompt = true;

            // Show save file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            // Process save file dialog box results
            if (result == true)
            {
                // Save document
                string file = dlg.FileName;

                Mouse.OverrideCursor = Cursors.Wait;
                FixedDocument doc = sdwpf.OutputXPS.CreateFixedDocument(GetCurrentStep().plan);

                if (File.Exists(file))
                {
                    File.Delete(file);
                }

                XpsDocument _xpsDocument = new XpsDocument(file, FileAccess.ReadWrite);

                XpsDocumentWriter xw = XpsDocument.CreateXpsDocumentWriter(_xpsDocument);

                xw.Write(doc);

                _xpsDocument.Close();

                Mouse.OverrideCursor = null;
            }
        }

        void OnClick_Transparent(object sender, RoutedEventArgs args)
        {
            foreach (MaterialsListItem lvi in myPieces.SelectedItems)
            {
                MakeTransparent(lvi.sol);
            }
        }

        void OnClick_Opaque(object sender, RoutedEventArgs args)
        {
            foreach (MaterialsListItem lvi in myPieces.SelectedItems)
            {
                MakeOpaque(lvi.sol);
            }
        }

        void Fit()
        {
            wpfmisc.AutoZoom(vstuff.vp, vstuff.tran);
            if (vstuff.tran.Zoom > slider_zoom.Maximum)
            {
                slider_zoom.Maximum = vstuff.tran.Zoom;
            }
            slider_zoom.Value = vstuff.tran.Zoom;

#if not
            Rect bb = sdwpf.Get2DBoundingBox(vstuff.vp);

            Rectangle r = new Rectangle();
            r.Width = bb.Width;
            r.Height = bb.Height;
            Canvas.SetLeft(r, bb.Left);
            Canvas.SetTop(r, bb.Top);
            r.Fill = Brushes.Red;
            r.Opacity = 0.3;
            Overlay.Children.Clear();
            Overlay.Children.Add(r);
#endif
        }

        void OnClick_Fit(object sender, RoutedEventArgs args)
        {
            Fit();
        }

        void OnClick_CopyXAML(object sender, RoutedEventArgs args)
        {
            Mouse.OverrideCursor = Cursors.Wait;

            try
            {
                ViewMaker vo = vstuff.CloneSettings();
                vo.vp = null;
                vo.bAnimate = false;
                vo.bStaticLines = true;
                vo.width = 1600;
                vo.height = 1200;

                vo.MakeView(GetCurrentStep());

                StringBuilder sb = new StringBuilder();
                TextWriter tw = new StringWriter(sb);
                XmlTextWriter xw = new XmlTextWriter(tw);
                xw.Formatting = Formatting.Indented;
                XamlWriter.Save(vo.vp, xw);
                xw.Close();

                string xaml = sb.ToString();

                // string x2 = XamlWriter.Save(vo.vp);

                xaml = xaml.Replace(string.Format("<Viewport3D Height=\"{0}\" Width=\"{1}\" ", vo.height, vo.width), "<Viewport3D ");

                Clipboard.SetText(xaml);

                //Clipboard.SetText(XamlWriter.Save(vo.vp));
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
        }

        void OnClick_CopyImage(object sender, RoutedEventArgs args)
        {
            Mouse.OverrideCursor = Cursors.Wait;

            try
            {
                ViewMaker vo = vstuff.CloneSettings();
                vo.vp = null;
                vo.bAnimate = false;
                vo.width = 1600;
                vo.height = 1200;
                vo.bCloneTran = true;
                vo.bAutoZoom = true;

                BitmapSource bmp = vo.MakeBitmap(GetCurrentStep());

#if false
                Rect bb = sdwpf.Get2DBoundingBox(vo.vp);
                Int32Rect ir = new Int32Rect((int)bb.X, (int)bb.Y, (int)bb.Width, (int)bb.Height);
                CroppedBitmap cb = new CroppedBitmap(bmp, ir);
                Clipboard.SetImage(cb);
#else
                Clipboard.SetImage(bmp);
#endif

            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
        }

        void OnClick_Print(object sender, RoutedEventArgs args)
        {
            PrintDialog dlg = new PrintDialog();
            if ((bool)dlg.ShowDialog().GetValueOrDefault())
            {
                FixedDocument doc = OutputXPS.CreateFixedDocument(GetCurrentStep());

                XpsDocumentWriter writer = PrintQueue.CreateXpsDocumentWriter(dlg.PrintQueue);
                writer.Write(doc);
            }
        }

        void OnVariableChange(object sender, RoutedEventArgs args)
        {
            Slider slid = (Slider)sender;
            VarSlider vs = (VarSlider)slid.Tag;

            vs.changed();

            CancelVarChange.IsEnabled = true;
            ApplyVarChange.IsEnabled = true;
        }

        void OnCancelVarChange(object sender, RoutedEventArgs args)
        {
            foreach (VarSlider vs in varsliders)
            {
                vs.cancel();
            }
            CancelVarChange.IsEnabled = false;
            ApplyVarChange.IsEnabled = false;
        }

        void OnApplyVarChange(object sender, RoutedEventArgs args)
        {
            Mouse.OverrideCursor = Cursors.Wait;

            try
            {
                foreach (VarSlider vs in varsliders)
                {
                    vs.apply();
                }
                myPlan.Execute();
                foreach (ListViewItem lvi in this.mySteps.Items)
                {
                    lvi.Content = MakeListViewPicture(lvi.Tag as Step);
                }
                refresh();
                CancelVarChange.IsEnabled = false;
                ApplyVarChange.IsEnabled = false;
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
        }

        void OpenLinkInBrowser(object sender, RoutedEventArgs args)
        {
            Hyperlink link = (Hyperlink)sender;

            System.Diagnostics.Process.Start(link.NavigateUri.ToString());
        }

        void cbSteps_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            mySteps.Items.Clear();
            ComboBoxItem cbi = (ComboBoxItem)cbSteps.SelectedItem;
            string sa = (string)cbi.Tag;
            if (sa == "__all")
            {
                foreach (Step st in myPlan.Steps)
                {
                    ListViewItem lvi = MakeListViewItem(st);
                    mySteps.Items.Add(lvi);
                }
            }
            else if (sa == "__notes")
            {
                foreach (Step st in myPlan.Steps)
                {
                    if (
                        (st.Notes != null)
                        && (st.Notes.Length > 0)
                        )
                    {
                        ListViewItem lvi = MakeListViewItem(st);
                        mySteps.Items.Add(lvi);
                    }
                }
            }
            else
            {
                sd.Action a = Step.ParseAction(sa);
                foreach (Step st in myPlan.Steps)
                {
                    if (st.action == a)
                    {
                        ListViewItem lvi = MakeListViewItem(st);
                        mySteps.Items.Add(lvi);
                    }
                }
            }
            mySteps.SelectedItem = mySteps.Items[0];
            mySteps.ScrollIntoView(mySteps.SelectedItem);
        }

        void mySteps_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListViewItem lvi = mySteps.SelectedItem as ListViewItem;
            if (lvi != null)
            {
                Step st = lvi.Tag as Step;
                ShowStep(st);
            }
            else
            {
                ShowStep(null);
            }
        }

        void OnSelectionChanged_Pieces(object sender, SelectionChangedEventArgs e)
        {
            foreach (MaterialsListItem lvi in myPieces.Items)
            {
                Solid s = lvi.sol;
                if (myPieces.SelectedItems.Contains(lvi))
                {
                    DrawSelected(s);
                }
                else
                {
                    DrawUnselected(s);
                }
            }
            btn_Transparent.IsEnabled = (myPieces.SelectedItems.Count > 0);
            btn_Opaque.IsEnabled = (myPieces.SelectedItems.Count > 0);
        }

        private void DrawSelected(Solid s)
        {
            foreach (PartialModel pm in vstuff.models)
            {
                if (pm.bag.solid == s)
                {
                    DiffuseMaterial mat = (DiffuseMaterial)pm.gm3d.Material;
                    SolidColorBrush br = (SolidColorBrush)mat.Brush;
                    br.Color = Colors.Green;
                }
            }
        }

        private void DrawUnselected(Solid s)
        {
            foreach (PartialModel pm in vstuff.models)
            {
                if (pm.bag.solid == s)
                {
                    DiffuseMaterial mat = (DiffuseMaterial)pm.gm3d.Material;
                    SolidColorBrush br = (SolidColorBrush)mat.Brush;
                    br.Color = pm.clr;
                }
            }
        }

        Step GetCurrentStep()
        {
            ListViewItem lvi2 = mySteps.SelectedItem as ListViewItem;
            if (lvi2 != null)
            {
                Step st = lvi2.Tag as Step;
                return st;
            }
            else
            {
                return null;
            }
        }

        void refresh()
        {
            // TODO this hack forces redisplay
            ListViewItem lvi2 = mySteps.SelectedItem as ListViewItem;
            if (lvi2 != null)
            {
                Step st = lvi2.Tag as Step;
                ShowStep(st);
            }
        }
    }
}
