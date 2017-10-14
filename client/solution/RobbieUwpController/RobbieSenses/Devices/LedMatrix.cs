using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.I2c;

namespace RobbieSenses.Devices
{
    /// <summary>
    /// Class to control an LED Matrix I2C device.
    /// </summary>
    public class LedMatrix : IDisposable
    {
        /// <summary>
        /// The default address of a LED Matrix with Backpack.
        /// </summary>
        /// <remarks>
        /// Mind that you cannot use this address when running a PWM / Servo HAT at the same time, because despite the different address (0x40),
        /// there still seems to be a conflict. Consequently, only address 0x71, 0x72 and 0x73 are available in this case.
        /// </remarks>
        private const byte LedMatrixI2CAddress = 0x70;

        /// <summary>
        /// The size (both width and height) of the LED Matrix.
        /// </summary>
        private const int MatrixSize = 8;
        
        /// <summary>
        /// The LED MAtrix I2C device.
        /// </summary>
        private I2cDevice ledMatrixI2CDevice;

        /// <summary>
        /// The base address the I2C device is configured at.
        /// </summary>
        private readonly byte baseAddress;

        /// <summary>
        /// A boolean value indicating whether the I2C device is already initialized or not.
        /// </summary>
        private bool initialized;

        /// <summary>
        /// A two dimensional byte array keeping track of the currently set LED values.
        /// </summary>
        private readonly byte[,] matrixData;

        /// <summary>
        /// Constructs a new LED Matrix object on the given I2C base address.
        /// </summary>
        /// <param name="baseAddress">The address this I2C device is configured to.</param>
        public LedMatrix(byte baseAddress)
        {
            this.baseAddress = baseAddress;
            matrixData = new byte[MatrixSize, MatrixSize];

            initialized = false;
            Initialize();
        }

        /// <summary>
        /// Constructs a new LED Matrix object using the default I2C address.
        /// </summary>
        public LedMatrix() : this(LedMatrixI2CAddress)
        {
        }

        /// <summary>
        /// Initializes the I2C device.
        /// </summary>
        private void Initialize()
        {
            if (initialized) return;

            // define both the device information collection as the device settings using the configured base address
            DeviceInformationCollection devices;
            var settings = new I2cConnectionSettings(baseAddress)
            {
                BusSpeed = I2cBusSpeed.StandardMode
            };

            // find the LED Matrix I2C device
            Task.Run(async () =>
            {
                var selector = I2cDevice.GetDeviceSelector("I2C1");
                devices = await DeviceInformation.FindAllAsync(selector);

                if (devices.Any())
                {
                    ledMatrixI2CDevice = await I2cDevice.FromIdAsync(devices[0].Id, settings);
                }

            }).Wait();

            if (ledMatrixI2CDevice != null)
            {
                // initialize display
                ledMatrixI2CDevice.Write(new byte[] { 0x21 });
                ledMatrixI2CDevice.Write(new byte[] { 0x81 });
            }

            initialized = true;
            Reset();
        }

        /// <summary>
        /// Reset the LED matrix by turning off all LEDs.
        /// </summary>
        public void Reset()
        {
            // switch all LEDs off
            for (var i = 0; i < (MatrixSize * 2); i = i + 2)
            {
                ledMatrixI2CDevice?.Write(new byte[] { (byte)i, 0x00 });
            }
        }

        /// <summary>
        /// Sets the state of an individual LED.
        /// </summary>
        /// <param name="row">The row number of the LED to set the state of.</param>
        /// <param name="column">The column number of the LED to set the state of.</param>
        /// <param name="state">The state to set as a byte value, being on (0x01) or off (0x00).</param>
        public void SetLEDState(int row, int column, byte state)
        {
            // shift columns because matrix column order is 70123456 instead of 01234567
            column = (column + 7) & 7;

            // store the new state locally
            matrixData[row, column] = state;

            // transform a row of single LED byte values into a single byte value for the whole row
            byte rowData = 0x00;
            for (var i = 0; i < MatrixSize; i++)
            {
                rowData |= (byte)(matrixData[row, i] << (byte)i);
            }
            
            SetRowState(row, rowData);
        }

        /// <summary>
        /// Sets the state of a whole row off LEDs.
        /// </summary>
        /// <param name="row">The row number of the LEDs to set the state of.</param>
        /// <param name="state">The byte value for the whole row.</param>
        /// <remarks>
        /// Mind that you can use the separate application called EyeDesigner (also in this repository) to calculate the desired byte value for a row.
        /// </remarks>
        public void SetRowState(int row, byte state)
        {
            // only write the value to the LED Matrix if a device has been found
            ledMatrixI2CDevice?.Write(new[] { (byte)(row * 2), state });
        }

        /// <summary>
        /// Dispose the I2C device after turning off all LEDs (by resetting the device).
        /// </summary>
        public void Dispose()
        {
            if (ledMatrixI2CDevice == null) return;

            try
            {
                Reset();
                ledMatrixI2CDevice.Dispose();
            }
            catch (Exception)
            {
                // ignored
            }
        }
    }
}
