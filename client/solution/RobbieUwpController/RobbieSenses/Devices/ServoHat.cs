using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.I2c;
using Windows.Devices.Pwm.Provider;

namespace RobbieSenses.Devices
{
    public class ServoHat : IDisposable, IPwmControllerProvider
    {
        /// <summary>
        /// The default address of a PWM / Servo HAT.
        /// </summary>
        private const byte PwmServoHatI2CAddress = 0x40;

        /// <summary>
        /// The PWM Servo HAT I2C device.
        /// </summary>
        private I2cDevice pwmServoHat;

        /// <summary>
        /// The base address the I2C device is configured at.
        /// </summary>
        private readonly byte baseAddress;

        /// <summary>
        /// A boolean value indicating whether the I2C device is already initialized or not.
        /// </summary>
        private bool initialized;

        /// <summary>
        /// The single instance of the ServoHat class used by the Singleton pattern.
        /// </summary>
        private static ServoHat instance;

        /// <summary>
        /// Enumeration containing byte values used when sending commands to the PWM controller.
        /// </summary>
        public enum Registers
        {
            Mode1 = 0x00,
            Mode2 = 0x01,
            SubAddress1 = 0x02,
            SubAddress2 = 0x03,
            SubAddress3 = 0x04,
            Prescale = 0xFE,
            LED0OnLow = 0x06,
            LED0OnHigh = 0x07,
            LED0OffLow = 0x08,
            LED0OffHigh = 0x09,
            AllLEDOnLow = 0xFA,
            AllLEDOnHigh = 0xFB,
            AllLEDOffLow = 0xFC,
            AllLEDOffHigh = 0xFD
        }

        /// <summary>
        /// Constructs a new PWM Servo HAT controller object on the given I2C base address.
        /// </summary>
        /// <param name="baseAddress">The address this I2C device is configured to.</param>
        private ServoHat(byte baseAddress)
        {
            this.baseAddress = baseAddress;

            MinFrequency = 40;
            MaxFrequency = 1000;
            PinCount = 16;

            initialized = false;
            Initialize();
        }

        /// <summary>
        /// Constructs a new PWM Servo HAT controller object using the default I2C address.
        /// </summary>
        private ServoHat() : this(PwmServoHatI2CAddress)
        {
        }

        /// <summary>
        /// Retrieve the Singleton instance of the ServoHat class, creating a new object if necessary.
        /// </summary>
        public static ServoHat Instance => instance ?? (instance = new ServoHat());

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

            // find the PWM Servo HAT I2C device
            Task.Run(async () =>
            {
                var selector = I2cDevice.GetDeviceSelector();
                devices = await DeviceInformation.FindAllAsync(selector);

                if (devices.Any())
                {
                    pwmServoHat = await I2cDevice.FromIdAsync(devices[0].Id, settings);
                }

            }).Wait();

            try
            {
                if (pwmServoHat != null)
                {
                    // if a PWM Servo HAT is found, configure the desired frequency
                    SetDesiredFrequency(60);
                }
            }
            catch
            {
                // ignored
            }

            initialized = true;
            Reset();
        }

        /// <summary>
        /// Reset the PWM Servo HAT by centering all servos.
        /// </summary>
        public void Reset()
        {
            if (pwmServoHat == null) return;

            pwmServoHat.Write(new byte[] {(byte) Registers.Mode1, 0x0});
            SetAllPwm(0, 0);
        }

        /// <summary>
        /// The actual frequency the PWM signal has been set to.
        /// </summary>
        public double ActualFrequency { get; private set; }
        
        /// <summary>
        /// The minimum frequency available to set the PWM signal frequency to (40 Hz by default).
        /// </summary>
        public double MinFrequency { get; }

        /// <summary>
        /// The minimum frequency available to set the PWM signal frequency to (1.000 Hz by default).
        /// </summary>
        public double MaxFrequency { get; }

        /// <summary>
        /// The number of pins the PWM / Servo HAT contains (16 by default).
        /// </summary>
        public int PinCount { get; }

        /// <summary>
        /// Toggles the pulse for a given pin.
        /// </summary>
        /// <param name="pin">The pin number of the pin to set the pulse for.</param>
        /// <param name="dutyCycle">The value to set the pin to.</param>
        /// <param name="invertPolarity">A boolean value indicating whether to invert the polarity or not (inverse servo).</param>
        public void SetPulseParameters(int pin, double dutyCycle, bool invertPolarity)
        {
            if (pwmServoHat == null) return;

            dutyCycle = Math.Min(dutyCycle, 4095);

            var value = (ushort) dutyCycle;
            var channel = (byte) pin;

            if (channel > PinCount - 1)
            {
                throw new ArgumentOutOfRangeException(nameof(channel), "Channel must be between 0 and 15");
            }

            // Use special value (4096) for signal fully on/off. 
            switch (value)
            {
                case 4095:
                    SetPwm(channel, 4096, 0);
                    break;
                case 0:
                    SetPwm(channel, 0, 4096);
                    break;
                default:
                    if (invertPolarity)
                    {
                        SetPwm(channel, 0, (ushort)(4095 - value));
                    }
                    else
                    {
                        SetPwm(channel, 0, value);
                    }
                    break;
            }
        }
        
        /// <summary>
         /// Toggles the pulse for all pins.
         /// </summary>
         /// <param name="dutyCycle">The value to set the pins to.</param>
         /// <param name="invertPolarity">A boolean value indicating whether to invert the polarity or not (inverse servos).</param>
        public void SetPulseParameters(double dutyCycle, bool invertPolarity)
        {
            if (pwmServoHat == null) return;

            dutyCycle = Math.Min(dutyCycle, 4095);

            var value = (ushort)dutyCycle;

            // Use special value (4096) for signal fully on/off. 
            switch (value)
            {
                case 4095:
                    SetAllPwm(4096, 0);
                    break;
                case 0:
                    SetAllPwm(0, 4096);
                    break;
                default:
                    if (invertPolarity)
                    {
                        SetAllPwm(0, (ushort)(4095 - value));
                    }
                    else
                    {
                        SetAllPwm(0, value);
                    }
                    break;
            }
        }

        /// <summary>
        /// Sets the PWM signal for a given pin.
        /// </summary>
        /// <param name="channel">The channel byte value of the pin to set.</param>
        /// <param name="on">The ushort on value.</param>
        /// <param name="off">The ushort off value.</param>
        private void SetPwm(byte channel, ushort on, ushort off)
        {
            pwmServoHat.Write(new[] { (byte)(Registers.LED0OnLow + 4 * channel), (byte)(on & 0xFF) });
            pwmServoHat.Write(new[] { (byte)(Registers.LED0OnHigh + 4 * channel), (byte)(on >> 8) });
            pwmServoHat.Write(new[] { (byte)(Registers.LED0OffLow + 4 * channel), (byte)(off & 0xFF) });
            pwmServoHat.Write(new[] { (byte)(Registers.LED0OffHigh + 4 * channel), (byte)(off >> 8) });
        }

        /// <summary>
        /// Sets the PWM signal for all pins.
        /// </summary>
        /// <param name="on">The ushort on value.</param>
        /// <param name="off">The ushort off value.</param>
        private void SetAllPwm(ushort on, ushort off)
        {
            pwmServoHat.Write(new[] { (byte)Registers.AllLEDOnLow, (byte)(on & 0xFF) });
            pwmServoHat.Write(new[] { (byte)Registers.AllLEDOnHigh, (byte)(on >> 8) });
            pwmServoHat.Write(new[] { (byte)Registers.AllLEDOffLow, (byte)(off & 0xFF) });
            pwmServoHat.Write(new[] { (byte)Registers.AllLEDOffHigh, (byte)(off >> 8) });
        }

        /// <summary>
        /// Specifies the frequency; defaults to 60hz if not set. This determines how many full pulses per second are generated.
        /// </summary>
        /// <param name="frequency">A number representing the frequency in Hz, between the configured minimum and maximum frequency (40 and 1.000 Hz by default).</param>
        /// <returns>The actual frequency the PWM signal has been set to.</returns>
        public double SetDesiredFrequency(double frequency)
        {
            if (frequency < MinFrequency || frequency > MaxFrequency)
            {
                throw new ArgumentOutOfRangeException(nameof(frequency), string.Format("Frequency must be between {0} and {1}hz", MinFrequency, MaxFrequency));
            }

            frequency *= 0.9f; // correct for overshoot in the frequency setting (see issue #11)
            double prescaleValue = 25000000f;
            prescaleValue /= 4096;
            prescaleValue /= frequency;
            prescaleValue -= 1;

            byte prescale = (byte) Math.Floor(prescaleValue + 0.5f);

            var readBuffer = new byte[1];
            pwmServoHat.WriteRead(new [] { (byte)Registers.Mode1 }, readBuffer);

            byte oldMode = readBuffer[0];
            byte newMode = (byte)((oldMode & 0x7F) | 0x10); //sleep
            pwmServoHat.Write(new [] { (byte)Registers.Mode1, newMode });
            pwmServoHat.Write(new [] { (byte)Registers.Prescale, prescale });
            pwmServoHat.Write(new [] { (byte)Registers.Mode1, oldMode });
            Task.Delay(TimeSpan.FromMilliseconds(5)).Wait();
            pwmServoHat.Write(new [] { (byte)Registers.Mode1, (byte)(oldMode | 0xa1) });

            ActualFrequency = frequency;
            return ActualFrequency;
        }


        /// <summary>
        /// Dispose the I2C device after centering all servos (by resetting the device).
        /// </summary>
        public void Dispose()
        {
            if (pwmServoHat == null) return;

            try
            {
                pwmServoHat.Dispose();
            }
            catch (Exception)
            {
                // ignored
            }
        }

        /// <summary>
        /// Required by the IPwmControllerProvider interface, but not (yet) required for controlling servos.
        /// </summary>
        /// <remarks>
        /// Not implemented!
        /// </remarks>
        /// <param name="pin">The pin number of the pin to acquire.</param>
        public void AcquirePin(int pin)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Required by the IPwmControllerProvider interface, but not (yet) required for controlling servos.
        /// </summary>
        /// <remarks>
        /// Not implemented!
        /// </remarks>
        /// <param name="pin">The pin number of the pin to enable.</param>
        public void EnablePin(int pin)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Required by the IPwmControllerProvider interface, but not (yet) required for controlling servos.
        /// </summary>
        /// <remarks>
        /// Not implemented!
        /// </remarks>
        /// <param name="pin">The pin number of the pin to disable.</param>
        public void DisablePin(int pin)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Required by the IPwmControllerProvider interface, but not (yet) required for controlling servos.
        /// </summary>
        /// <remarks>
        /// Not implemented!
        /// </remarks>
        /// <param name="pin">The pin number of the pin to release.</param>
        public void ReleasePin(int pin)
        {
            throw new NotImplementedException();
        }
    }
}
