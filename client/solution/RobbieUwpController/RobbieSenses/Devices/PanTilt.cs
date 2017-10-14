using Windows.Foundation;

namespace RobbieSenses.Devices
{
    /// <summary>
    /// Class controlling the whole pan tilt mechanism.
    /// </summary>
    public class PanTilt
    {
        /// <summary>
        /// If the difference between the focal point and the center point of the screen is smaller than the smallest duty cycle step,
        /// the servos will start jittering trying to exactly focus on the focal point, never being able to center exactly.
        /// Hence, we need a tolerance, allowing the servos to stop moving when their center point is close enough to the focal point.
        /// Additionaly, the focal point may move ever so slightly between frames, and we don't want the servos to chase these small movements.
        /// </summary>
        private const double AntiJitterTolerance = 20d;

        /// <summary>
        /// The horizontal pan axis to control.
        /// </summary>
        private readonly PanTiltAxis horizontalAxis;

        /// <summary>
        /// The vertical tilt axis to control.
        /// </summary>
        private readonly PanTiltAxis verticalAxis;

        /// <summary>
        /// The center point of the (camera) viewport, used to determine which way the camera should move.
        /// </summary>
        private Point centerPoint;

        /// <summary>
        /// The point to focus on, being either the largest face in the current viewport, or a negative coordinate if no faces are found.
        /// </summary>
        public Point FocalPoint;

        /// <summary>
        /// Constructs a new PanTilt class, currently containing fixed values for servo pins and duty cycle ranges.
        /// </summary>
        public PanTilt()
        {
            // todo: consider if these values should be configurable or moved to a constant for example
            horizontalAxis = new PanTiltAxis(0, 246, 368, 490, 1);
            verticalAxis = new PanTiltAxis(1, 287, 353, 400, 1);

            FocalPoint = new Point(-1, -1);
        }

        /// <summary>
        /// Moves the camera towards a certain given point. Use this for each frame, updating it with the new focal point.
        /// If the focal point is negative (no faces are found or no focus point is given), the pan tilt mechanism centers its servos on both axes.
        /// </summary>
        /// <remarks>
        /// Note that it not immediately moves the servos to the exact given point, because that would be very hard or even impossible to determine,
        /// but most of all, would lead to very choppy servo movements. Instead, we move the camera one step towards the desired point over both axes,
        /// resulting in a slow and steady motion towards the target location, while still constantly adjusting its position, but much more fluent.
        /// </remarks>
        public void MoveTowardsFocalPoint()
        {
            // if the center point isn't set yet, we need to calibrate the center point first
            // to avoid equality issues due to loss of precision upon rounding, let's check if less than 1 instead of equal to 0
            if (centerPoint.X < 1 || centerPoint.Y < 1)
            {
                // if calibration doesn't succeed, there's no point in continuing and moving any servos
                if (!Calibrate()) return;
            }

            // if the focal point is a negative coordinate, there are no faces in the current viewport
            // so hold your position until receiving new focal point information
            if (FocalPoint.X < 0)
            {
                return;
            }

            // if calibration succeeded and we have a focal point, move towards it on both axes
            // mind that if the center point matches the focal point, Robbie is exactly looking towards it already, so don't move!
            if (FocalPoint.X < centerPoint.X - AntiJitterTolerance)
            {
                horizontalAxis.Move(PanTiltAxis.Direction.Right);
            }
            else if (FocalPoint.X > centerPoint.X + AntiJitterTolerance)
            {
                horizontalAxis.Move(PanTiltAxis.Direction.Left);
            }

            if (FocalPoint.Y < centerPoint.Y - AntiJitterTolerance)
            {
                verticalAxis.Move(PanTiltAxis.Direction.Down);
            }
            else if (FocalPoint.Y > centerPoint.Y + AntiJitterTolerance)
            {
                verticalAxis.Move(PanTiltAxis.Direction.Up);
            }
        }

        /// <summary>
        /// Calibrates the center point by splitting the dimensions of the viewport in two.
        /// </summary>
        /// <remarks>
        /// If the camera isn't finished initializing, the viewport size and thus the center point will still be 0.
        /// When that is the case, this method will return a boolean false value, so further servo movement could be halted.
        /// </remarks>
        /// <returns>True if the viewport size could be determined.</returns>
        private bool Calibrate()
        {
            centerPoint = new Point(Camera.Instance.ViewPortSize.Width / 2,
                Camera.Instance.ViewPortSize.Height / 2);

            // check if the calibration succeeded by checking the x coordinate of the center point, which should've been set now
            // to avoid equality issues due to loss of precision upon rounding, let's check if less than 1 instead of equal to 0
            return centerPoint.X > 1 && centerPoint.Y > 1;
        }
    }
}
