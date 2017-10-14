using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RobbieSenses.Devices;

namespace RobbieSenses.Output
{
    /// <summary>
    /// Class representing a set of eyes, rendered by two LED Matrices.
    /// </summary>
    public class EyesDisplay : IDisposable
    {
        /// <summary>
        /// Enumeration of all available expressions, relating to the different emotions Robbie can reflect.
        /// </summary>
        public enum Emotions
        {
            Anger,
            Contempt,
            Disgust,
            Fear,
            Happiness,
            Neutral,
            Sadness,
            Surprise,
            Sleep,
            Blink
        }

        /// <summary>
        /// The time to hold a different emotion than neutral.
        /// </summary>
        private const int EmotionFallbackTimespan = 3000;

        /// <summary>
        /// A dictionary holding all byte values for the LED matrices per available expression / emotion.
        /// </summary>
        private readonly Dictionary<Emotions, byte[]> expressions;
        
        /// <summary>
        /// The I2C Device of the LED Matrix representing the left eye.
        /// </summary>
        private readonly LedMatrix leftEye;

        /// <summary>
        /// The I2C Device of the LED Matrix representing the right eye.
        /// </summary>
        private readonly LedMatrix rightEye;

        /// <summary>
        /// Holds the current expression displayed, mainly used to be able to return to this emotion after blinking.
        /// </summary>
        private Emotions currentExpression;

        /// <summary>
        /// Constructs a new set of eyes.
        /// </summary>
        public EyesDisplay()
        {
            // mind that you can use the separate application called EyeDesigner (also in this repository) to calculate the desired byte values
            // the first eight byte values are used for the left eye and the second eight byte values for the right eye
            expressions = new Dictionary<Emotions, byte[]>
            {
                {Emotions.Anger, new byte[] { 0x00, 0x07, 0x8F, 0x99, 0xB1, 0xB1, 0x1F, 0x00, 0x00, 0x38, 0x7C, 0x66, 0x63, 0x63, 0x3E, 0x00 }},
                {Emotions.Contempt, new byte[] { 0x00, 0x00, 0xBF, 0xA3, 0xA3, 0xBF, 0x1F, 0x00, 0x00, 0x00, 0x7F, 0x47, 0x47, 0x7F, 0x3E, 0x00 }},
                {Emotions.Disgust, new byte[] { 0x00, 0x1F, 0xBF, 0xA3, 0xA3, 0xBF, 0x1F, 0x00, 0x00, 0x00, 0x7F, 0x7F, 0x71, 0x3E, 0x00, 0x00 }},
                {Emotions.Fear, new byte[] { 0x00, 0x1F, 0xBF, 0xB1, 0xB1, 0xB1, 0x1F, 0x00, 0x00, 0x3E, 0x7F, 0x63, 0x63, 0x63, 0x3E, 0x00 }},
                {Emotions.Happiness, new byte[] { 0x0E, 0x1F, 0xBF, 0xB1, 0xB1, 0xBF, 0x1F, 0x00, 0x1C, 0x3E, 0x7F, 0x63, 0x63, 0x7F, 0x3E, 0x00 }},
                {Emotions.Neutral, new byte[] { 0x00, 0x1F, 0xBF, 0xB1, 0xB1, 0xBF, 0x1F, 0x00, 0x00, 0x3E, 0x7F, 0x63, 0x63, 0x7F, 0x3E, 0x00 }},
                {Emotions.Sadness, new byte[] { 0x00, 0x00, 0xBF, 0xBF, 0xB1, 0x1F, 0x00, 0x00, 0x00, 0x00, 0x7F, 0x7F, 0x63, 0x3E, 0x00, 0x00 }},
                {Emotions.Surprise, new byte[] { 0x0E, 0x1F, 0xB1, 0xB1, 0xB1, 0xB1, 0x1F, 0x0E, 0x1C, 0x3E, 0x63, 0x63, 0x63, 0x63, 0x3E, 0x1C }},
                {Emotions.Sleep, new byte[] { 0x00, 0x00, 0x00, 0x00, 0xA1, 0xBF, 0x1E, 0x00, 0x00, 0x00, 0x00, 0x00, 0x61, 0x7F, 0x1E, 0x00 }},
                {Emotions.Blink, new byte[] { 0x00, 0x00, 0x00, 0xBF, 0xBF, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x7F, 0x7F, 0x00, 0x00, 0x00 }}
            };

            leftEye = new LedMatrix(0x71);
            rightEye = new LedMatrix(0x72);

            currentExpression = Emotions.Neutral;
            SetEmotion(currentExpression);
        }

        /// <summary>
        /// Sets the emotion to a given expression based on a string value.
        /// If the emotion is not available in the Emotions enumeration, the neutral emotion will be used.
        /// </summary>
        /// <param name="emotion">A string value representing the desired emotion.</param>
        public void SetEmotion(string emotion)
        {
            Emotions expression;
            if (!Enum.TryParse(emotion, out expression))
            {
                expression = Emotions.Neutral;
            }
            SetEmotion(expression);
        }

        /// <summary>
        /// Sets the emotion to a given expression using the Emotions enumeration as input.
        /// </summary>
        /// <param name="emotion">An Emotions enumeration value representing the desired emotion.</param>
        public void SetEmotion(Emotions emotion)
        {
            for (var row = 0; row < 8; row++)
            {
                leftEye.SetRowState(row, expressions[emotion][row]);
                rightEye.SetRowState(row, expressions[emotion][row + 8]);
            }
            currentExpression = emotion;

            // if the emotion is set to any other emotion than neutral, sleep or blink, switch back to neutral after the emotion fallback timespan has elapsed
            if (emotion != Emotions.Neutral && emotion != Emotions.Sleep && emotion != Emotions.Blink)
            {
                Task.Run(async delegate
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(EmotionFallbackTimespan));
                    SetEmotion(Emotions.Neutral);
                });
            }
        }

        /// <summary>
        /// Lets Robbie blink for a given amount of time, of which the default is 100 ms.
        /// </summary>
        /// <param name="duration">The duration Robbie should blink in milliseconds; if not provided, the default value of 100 ms will be used.</param>
        public void Blink(double duration = 100)
        {
            var returnToEmotion = currentExpression;
            SetEmotion(Emotions.Blink);
            Task.Delay(TimeSpan.FromMilliseconds(duration)).Wait();
            SetEmotion(returnToEmotion);
        }

        /// <summary>
        /// Disposes the Eyes object, turning off all LEDs and disposing the correspoding I2C devices.
        /// </summary>
        public void Dispose()
        {
            leftEye.Dispose();
            rightEye.Dispose();
        }
    }
}
