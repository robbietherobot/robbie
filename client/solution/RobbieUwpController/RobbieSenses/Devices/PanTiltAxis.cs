using System;

namespace RobbieSenses.Devices
{
    /// <summary>
    /// Class to control the PWM signal of one axis / servo.
    /// </summary>
    public class PanTiltAxis
    {
        /// <summary>
        /// The pin number of the servo to control.
        /// </summary>
        private readonly int pinNumber;

        /// <summary>
        /// The minimum duty cycle of the PWM signal, determining the lower bound of the servo travel.
        /// </summary>
        private readonly int minDutyCycle;

        /// <summary>
        /// The duty cycle of the PWM signal which should be used as the center position.
        /// </summary>
        private readonly int centerDutyCycle;

        /// <summary>
        /// The maximum duty cycle of the PWM signal, determining the upper bound of the servo travel.
        /// </summary>
        private readonly int maxDutyCycle;

        /// <summary>
        /// The size of each step when moving towards the desired position.
        /// The frequency of the steps taken is equal to the frame rate of the video processing.
        /// </summary>
        private readonly int stepSize;

        /// <summary>
        /// Indicates whether the polarity of the servo should be inverted.
        /// </summary>
        private readonly bool invertPolarity;

        /// <summary>
        /// The current duty cycle value, indicating the current servo arm position.
        /// </summary>
        private int dutyCycle;

        /// <summary>
        /// Enumeration of the supported directions a servo arm could move into.
        /// </summary>
        public enum Direction
        {
            Left,
            Right,
            Up,
            Down
        }

        /// <summary>
        /// Constructs a new PanTiltAxis object based on the given servo configuration. 
        /// </summary>
        /// <param name="pin">The pin number of the servo to control for this axis.</param>
        /// <param name="minValue">The minimum duty cycle of the PWM signal, determining the lower bound of the servo travel.</param>
        /// <param name="centerValue">The duty cycle of the PWM signal which should be used as the center position.</param>
        /// <param name="maxValue">The maximum duty cycle of the PWM signal, determining the upper bound of the servo travel.</param>
        /// <param name="step">The size of each duty cycle step when moving towards the desired position.</param>
        /// <param name="invert">Indicates whether the polarity of the servo should be inverted; false by default.</param>
        public PanTiltAxis(int pin, int minValue, int centerValue, int maxValue, int step, bool invert = false)
        {
            pinNumber = pin;
            minDutyCycle = minValue;
            centerDutyCycle = centerValue;
            maxDutyCycle = maxValue;
            stepSize = step;
            invertPolarity = invert;

            Center();
        }

        /// <summary>
        /// Centers the servo arm on this axis.
        /// </summary>
        public void Center()
        {
            SetServoPulse(centerDutyCycle);
        }

        /// <summary>
        /// Moves the servo arm one step into the desired direction.
        /// Mind that the lower and upper duty cycle bounds are always respected (stopping the motion when reaching those values).
        /// </summary>
        /// <param name="direction">A Direction enumeration value indicating the direction to move into.</param>
        public void Move(Direction direction)
        {
            int newCycleValue;
            switch (direction)
            {
                case Direction.Left:
                case Direction.Down:
                    newCycleValue = Math.Max(dutyCycle - stepSize, minDutyCycle);
                    break;
                case Direction.Right:
                case Direction.Up:
                    newCycleValue = Math.Min(dutyCycle + stepSize, maxDutyCycle);
                    break;
                default:
                    return;
            }

            SetServoPulse(newCycleValue);
        }

        /// <summary>
        /// Sets the PWM servo pulse value of the currently controlled servo to the new desired servo arm position.
        /// </summary>
        /// <param name="newCycleValue">The new duty cycle value to set the servo pulse to.</param>
        private void SetServoPulse(int newCycleValue)
        {
            // only proceed if position of servo actually changes
            if (newCycleValue == dutyCycle) return;

            // move the servo to its new position
            ServoHat.Instance.SetPulseParameters(pinNumber, newCycleValue, invertPolarity);
            dutyCycle = newCycleValue;
        }
    }
}
