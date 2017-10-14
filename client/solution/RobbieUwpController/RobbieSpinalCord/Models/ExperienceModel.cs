using System;

namespace RobbieSpinalCord.Models
{
    public class ExperienceModel
    {
        public Visits Visits { get; set; }
        public Personalinfo PersonalInfo { get; set; }
        public Onsitebehavior OnsiteBehavior { get; set; }
        public Referral Referral { get; set; }
    }

    public class Visits
    {
        public int EngagementValue { get; set; }
        public Pageview[] PageViews { get; set; }
        public int TotalPageViews { get; set; }
        public int TotalVisits { get; set; }
        public object[] EngagementPlanStates { get; set; }
    }

    public class Pageview
    {
        public string FullPath { get; set; }
        public string Path { get; set; }
        public string Duration { get; set; }
        public bool HasEngagementValue { get; set; }
        public bool HasMvTest { get; set; }
        public bool HasPersonalisation { get; set; }
    }

    public class Personalinfo
    {
        public string FullName { get; set; }
        public bool IsIdentified { get; set; }
        public Property1[] Properties { get; set; }
        public object PhotoUrl { get; set; }
        public object Location { get; set; }
        public object Device { get; set; }
    }

    public class Property1
    {
        public string Key { get; set; }
        public string Value { get; set; }
    }

    public class Onsitebehavior
    {
        public Activeprofile[] ActiveProfiles { get; set; }
        public Historicprofile[] HistoricProfiles { get; set; }
        public Goal[] Goals { get; set; }
        public object[] Outcomes { get; set; }
        public object[] PageEvents { get; set; }
    }

    public class Activeprofile
    {
        public string Name { get; set; }
        public Patternmatch[] PatternMatches { get; set; }
    }

    public class Patternmatch
    {
        public string Profile { get; set; }
        public string PatternName { get; set; }
        public string Image { get; set; }
        public float MatchPercentage { get; set; }
    }

    public class Historicprofile
    {
        public string Name { get; set; }
        public Patternmatch1[] PatternMatches { get; set; }
    }

    public class Patternmatch1
    {
        public string Profile { get; set; }
        public string PatternName { get; set; }
        public string Image { get; set; }
        public float MatchPercentage { get; set; }
    }

    public class Goal
    {
        public int EngagementValue { get; set; }
        public string Title { get; set; }
        public DateTime Date { get; set; }
        public bool IsCurrentVisit { get; set; }
        public object Data { get; set; }
    }

    public class Referral
    {
        public string ReferringSite { get; set; }
        public int TotalNoOfCampaigns { get; set; }
        public object[] Campaigns { get; set; }
    }

}
