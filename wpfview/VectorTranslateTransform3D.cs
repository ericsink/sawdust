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
    public class VectorTranslateTransform3D : UIElement
    {
        private xyz _direction;
        private TranslateTransform3D _tt;

        private void update()
        {
            xyz vec = _direction * Distance;

            _tt.OffsetX = vec.x;
            _tt.OffsetY = vec.y;
            _tt.OffsetZ = vec.z;
        }

        public xyz Direction
        {
            get
            {
                return _direction;
            }
            set
            {
                _direction = value.normalize();
            }
        }

        public double Distance
        {
            get
            {
                return (double)this.GetValue(DistanceProperty);
            }
            set
            {
                this.SetValue(DistanceProperty, value);
            }
        }

        public VectorTranslateTransform3D(TranslateTransform3D tt, xyz dir, double dist)
        {
            _tt = tt;
            Direction = dir;
            Distance = dist;
        }

        private static void DistanceChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            VectorTranslateTransform3D v = (VectorTranslateTransform3D)d;

            v.update();
        }

        public static readonly DependencyProperty DistanceProperty = DependencyProperty.Register("Distance", typeof(double), typeof(VectorTranslateTransform3D), new PropertyMetadata(new PropertyChangedCallback(DistanceChangedCallback)));
    }

}
