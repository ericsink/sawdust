using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Media3D;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Xps;
using System.Windows.Xps.Packaging;
using System.Windows.Xps.Serialization;
using System.Windows.Markup;
using System.Printing;
using System.IO;
using System.ComponentModel;

using sd;

namespace sdwpf
{
    public class StepViewTransform
    {
        private static Dictionary<Step, StepViewTransform> vts = new Dictionary<Step, StepViewTransform>();

        public static StepViewTransform GetViewTransformFor(Step st)
        {
            if (!vts.ContainsKey(st))
            {
                StepViewTransform vt = new StepViewTransform(st, true);
                vts[st] = vt;
            }
            return vts[st];
        }

        private Step step;
        private AxisAngleRotation3D rot_x;
        private AxisAngleRotation3D rot_y;
        private AxisAngleRotation3D rot_z;
        private ScaleTransform3D zoom;

        public TranslateTransform3D center;
        public Transform3D tran;

        public StepViewTransform(Step st, bool bKeepTheStepUpdated)
        {
            if (bKeepTheStepUpdated)
            {
                step = st;
            }
            else
            {
                step = null;
            }

            xyz c = st.Result.GetCenter();
            center = new TranslateTransform3D(c.x, c.y, c.z);
            rot_x = new AxisAngleRotation3D(new Vector3D(1, 0, 0), st.ViewRotX);
            rot_y = new AxisAngleRotation3D(new Vector3D(0, 1, 0), st.ViewRotY);
            rot_z = new AxisAngleRotation3D(new Vector3D(0, 0, 1), st.ViewRotZ);
            zoom = new ScaleTransform3D(st.ViewZoom, st.ViewZoom, st.ViewZoom);

            Transform3DGroup t = new Transform3DGroup();

            t.Children.Add(zoom);

            // the order of the following three is significant
            t.Children.Add(new RotateTransform3D(rot_y));
            t.Children.Add(new RotateTransform3D(rot_x));
            t.Children.Add(new RotateTransform3D(rot_z));

            t.Children.Add(center);

            tran = t;
        }

        public double Zoom
        {
            get
            {
                return 1 / zoom.ScaleX;
            }
            set
            {
                double zm = 1 / value;
                zoom.ScaleX = zoom.ScaleY = zoom.ScaleZ = zm;
                if (step != null)
                {
                    step.ViewZoom = zm;
                }
            }
        }

        public double RotZ
        {
            get
            {
                return rot_z.Angle;
            }
            set
            {
                rot_z.Angle = value;
                if (step != null)
                {
                    step.ViewRotZ = value;
                }
            }
        }

        public double RotX
        {
            get
            {
                return rot_x.Angle;
            }
            set
            {
                rot_x.Angle = value;
                if (step != null)
                {
                    step.ViewRotX = value;
                }
            }
        }

        public double RotY
        {
            get
            {
                return rot_y.Angle;
            }
            set
            {
                rot_y.Angle = value;
                if (step != null)
                {
                    step.ViewRotY = value;
                }
            }
        }

        public void AddToZoom(double v)
        {
            double nv = Zoom + v;
            if (nv > 0)
            {
                Zoom = nv;
            }
        }
    }

}
