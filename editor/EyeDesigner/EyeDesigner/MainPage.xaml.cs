using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

namespace EyeDesigner
{
    public sealed partial class MainPage
    {
        private const int MatrixSize = 8;

        private readonly SolidColorBrush onColorBrush = new SolidColorBrush(Colors.Red);
        private readonly SolidColorBrush offColorBrush = new SolidColorBrush(Colors.White);

        private readonly TextBlock[] matrixRowValue = new TextBlock[MatrixSize];

        private readonly byte[,] matrixData = new byte[MatrixSize, MatrixSize];

        public MainPage()
        {
            InitializeComponent();

            for (var i = 0; i < MatrixSize; i++)
            {
                var tb = new TextBlock
                {
                    Text = "0x00",
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };
                tb.SetValue(Grid.RowProperty, i);
                tb.SetValue(Grid.ColumnProperty, MatrixSize);
                matrix.Children.Add(tb);

                matrixRowValue[i] = tb;

                for (var j = 0; j < MatrixSize; j++)
                {
                    var led = new Ellipse
                    {
                        Width = 40,
                        Height = 40,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        Fill = offColorBrush
                    };
                    led.SetValue(Grid.RowProperty, i);
                    led.SetValue(Grid.ColumnProperty, j);
                    led.PointerPressed += Led_PointerPressed;
                    matrix.Children.Add(led);

                    SetMatrixData(i, j, 0);
                }
            }
        }
        
        private void SetMatrixData(int row, int column, byte state)
        {
            // shift columns in grid to columns on device
            column = (column + 7) & 7;

            // write state to matrix
            matrixData[row, column] = state;

            byte rowData = 0x00;

            // calculate byte            
            for (var i = 0; i < MatrixSize; i++)
            {
                rowData |= (byte)(matrixData[row, i] << (byte)i);
            }

            // show byte value for row
            matrixRowValue[row].Text = string.Format("0x{0:X2}", rowData);
        }

        private void Led_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            var led = (Ellipse)sender;

            if (led.Fill == offColorBrush)
            {
                led.Fill = onColorBrush;
                SetMatrixData((int)led.GetValue(Grid.RowProperty),
                    (int)led.GetValue(Grid.ColumnProperty),
                    0x01);
            }
            else
            {
                led.Fill = offColorBrush;
                SetMatrixData((int)led.GetValue(Grid.RowProperty),
                    (int)led.GetValue(Grid.ColumnProperty),
                    0x00);
            }
        }
    }
}
