namespace Application.LearningDashboards.Model
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
  

    public class BBBData
    {
        public Metadata metadata { get; set; }
        public int duration { get; set; }
        public DateTime start { get; set; }
        public DateTime finish { get; set; }
        public List<Attendee> attendees { get; set; }
        public List<string> files { get; set; }
        public List<Poll> polls { get; set; }
    }

    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class Attendee
    {
        public string ext_user_id { get; set; }
        public string name { get; set; }
        public bool moderator { get; set; }
        public List<DateTime> joins { get; set; }
        public List<DateTime> leaves { get; set; }
        public int duration { get; set; }
        public string recent_talking_time { get; set; }
        public Engagement engagement { get; set; }
        public List<Session> sessions { get; set; }
    }

    public class Data
    {
        public Metadata metadata { get; set; }
        public int duration { get; set; }
        public DateTime start { get; set; }
        public DateTime finish { get; set; }
        public List<Attendee> attendees { get; set; }
        public List<string> files { get; set; }
        public List<Poll> polls { get; set; }
    }

    public class Engagement
    {
        public int chats { get; set; }
        public int talks { get; set; }
        public int raisehand { get; set; }
        public int emojis { get; set; }
        public int poll_votes { get; set; }
        public int talk_time { get; set; }
    }

    public class Join
    {
        public DateTime timestamp { get; set; }
        public string userid { get; set; }
        public string ext_userid { get; set; }
        public string @event { get; set; }
        public bool remove { get; set; }
    }

    public class Left
    {
        public DateTime timestamp { get; set; }
        public string userid { get; set; }
        public string ext_userid { get; set; }
        public string @event { get; set; }
        public bool remove { get; set; }
    }

    public class Metadata
    {
        public string is_breakout { get; set; }
        public string meeting_name { get; set; }
    }

    public class Poll
    {
        public string id { get; set; }
        public string type { get; set; }
        public string question { get; set; }
        public bool published { get; set; }
        public List<string> options { get; set; }
        public DateTime start { get; set; }
        public Dictionary<string, string> votes { get; set; }


    }

    public class Root
    {
        public string version { get; set; }
        public string meeting_id { get; set; }
        public string internal_meeting_id { get; set; }
        public Data data { get; set; }
    }

    public class Session
    {
        public List<Join> joins { get; set; }
        public List<Left> lefts { get; set; }
    }

    public class Votes
    {
        //public string w_mak9ha7n8gam { get; set; }
    }



}
