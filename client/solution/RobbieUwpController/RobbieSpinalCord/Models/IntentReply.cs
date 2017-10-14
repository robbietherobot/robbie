// ReSharper disable InconsistentNaming due to following json format serialization

namespace RobbieSpinalCord.Models
{
    public class IntentReply
    {
        public class Rootobject
        {
            public Response response { get; set; }
        }

        public class Response
        {
            public string reply { get; set; }
            public string emotion { get; set; }
            public string action { get; set; }
        }
    }
}
