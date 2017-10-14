using Sitecore.Analytics.OmniChannel.Pipelines.DetermineInteractionChannel;
using Sitecore.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Robbie.Feature.Channel.Pipelines.DetermineInteractionChannel
{
    /// <summary>
    /// channel pipeline processor. Detects if channel is robbie.
    /// </summary>
    public class RobbieChannel: DetermineChannelProcessorBase
    {
        private readonly string RobbieUserAgentString = "Robbie/1.0 (Windows 10 IoT Core; Raspberry Pi 3 Model B)";
        private readonly string channelId;

        public RobbieChannel(string channelId)
        {
            this.channelId = channelId;
        }

        /// <summary>
        /// proces to execute
        /// </summary>
        /// <param name="args">actionContext</param>
        public override void Process(DetermineChannelProcessorArgs args)
        {
            if(this.IsRobbieTraffic())
            {
                args.ChannelId = new ID(this.channelId);
            }
        }

        /// <summary>
        /// determine if traffic is a robbie channel
        /// </summary>
        /// <returns>true if IsRobbie</returns>
        private bool IsRobbieTraffic()
        {
            var isRobbie = false;
            var ua = HttpContext.Current.Request.UserAgent;
            if (String.Equals(ua, RobbieUserAgentString, StringComparison.OrdinalIgnoreCase))
                isRobbie = true;
            return isRobbie;
        }
    }
}