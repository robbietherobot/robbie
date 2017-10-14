namespace RobbieBehaviour.Models
{
    /// <summary>
    /// View model used to communicate the profile card data, currently only used for passing Emotion Scores.
    /// todo: should be named specifically for this emotion profile card if it ain't a generic profile card class
    /// </summary>
    public class ProfileCardViewModel
    {
        public float Anger { get; set; }
        public float Contempt { get; set; }
        public float Disgust { get; set; }
        public float Fear { get; set; }
        public float Happiness { get; set; }
        public float Neutral { get; set; }
        public float Sadness { get; set; }
        public float Surprise { get; set; }
    }
}