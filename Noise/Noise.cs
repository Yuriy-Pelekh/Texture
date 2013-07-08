using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Noise
{
    [TemplatePart(Name = ContainerCanvasElementName, Type = typeof(Canvas))]
    public sealed class Noise : Control
    {
        private const string ContainerCanvasElementName = "container";

        private Canvas _container;
        private Canvas Container
        {
            get { return _container; }
            set
            {
                _container = value;
                if (Container != null)
                {
                    CalcImageSize(Source);
                    SizeChanged += ExImage_SizeChanged;
                }
            }
        }

        public ImageSource Source
        {
            get { return (ImageSource)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register("Source", typeof(ImageSource), typeof(Noise),
            new PropertyMetadata(null, SourcePropertyChangedCallback));

        private static void SourcePropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var exImage = d as Noise;
            if (exImage != null)
            {
                exImage.SourceChanged((ImageSource)e.NewValue);
            }
        }

        private int _imgHeight;
        private int _imgWidth;

        public Noise()
        {
            DefaultStyleKey = typeof(Noise);
        }

        // When size changed
        private void ExImage_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (_imgHeight != 0 && _imgWidth != 0)
            {
                CreateImage();
            }
        }

        // list of image
        private readonly List<Image> _imagesList = new List<Image>();

        /// <summary>
        /// Create the image
        /// </summary>
        private void CreateImage()
        {
            if (Container != null && Source != null)
            {
                _imagesList.Clear();
                Container.Children.Clear();

                int countX = (int)Math.Ceiling(Container.ActualWidth / _imgWidth);
                int countY = (int)Math.Ceiling(Container.ActualHeight / _imgHeight);
                double totalUsedHeight = 0.0; //Already used height
                for (int i = 0; i < countY; i++)
                {
                    double remainHeight = Container.ActualHeight - totalUsedHeight;
                    double totalUsedWidth = 0.0; //Already used Width
                    for (int j = 0; j < countX; j++)
                    {
                        double remainWidth = Container.ActualWidth - totalUsedWidth;
                        var img = new Image
                        {
                            Stretch = Stretch.None,
                            Width = remainWidth >= _imgWidth ? _imgWidth : remainWidth,
                            Height = remainHeight >= _imgHeight ? _imgHeight : remainHeight,
                            Source = Source
                        };
                        Canvas.SetLeft(img, _imgWidth * j);
                        Canvas.SetTop(img, _imgHeight * i);
                        _imagesList.Add(img);
                        totalUsedWidth += _imgWidth;
                    }
                    totalUsedHeight += _imgHeight;
                }

                foreach (var item in _imagesList)
                {
                    Container.Children.Add(item);
                }
            }
        }

        private void SourceChanged(ImageSource imageSource)
        {
            if (Container != null)
            {
                CalcImageSize(imageSource);
            }
        }

        private void CalcImageSize(ImageSource imageSource)
        {
            if (imageSource != null)
            {
                var img = new Image();
                img.ImageOpened += img_ImageOpened;
                img.Source = imageSource;
                Container.Children.Clear();
                // If we don't add this image to the visual tree, the image will never be loaded
                // so, the ImageOpened event will never happened
                Container.Children.Add(img);
            }
        }

        void img_ImageOpened(object sender, RoutedEventArgs e)
        {
            var img = sender as Image;
            var bmp = img.Source as BitmapSource;

            _imgHeight = bmp.PixelHeight;
            _imgWidth = bmp.PixelWidth;
            Container.Children.Remove(img);
            img = null;
            CreateImage();
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            Container = GetTemplateChild(ContainerCanvasElementName) as Canvas;
        }
    }
}
