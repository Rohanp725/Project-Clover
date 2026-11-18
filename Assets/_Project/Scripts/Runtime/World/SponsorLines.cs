namespace Clover.World
{
    /// <summary>
    /// Everything the sponsor says in the hub, in one place. It is only a voice: few words, plain and heavy, no jokes.
    /// It says what it wants the player to believe. Which lines are spoken depends on how the last run ended.
    /// </summary>
    public static class SponsorLines
    {
        public const string Name = "The Voice";

        public static readonly string[] FirstWaking =
        {
            "You are awake. Good.",
            "You do not know your name. I took it, with everything behind it. That was the price of pulling you out.",
            "The gods still stand. You will go back, and you will take from them what they took.",
            "Your hands are enough to begin with. Fire I can lend you.",
            "The gate leads to a castle under a storm. Its god was a swordsman once.",
            "If you fall, I will turn the hours back. They will remember nothing. You will remember all of it.",
        };

        private static readonly string[][] AfterAFall =
        {
            new[]
            {
                "You fell. I have turned it back.",
                "Do not trade blows with them. Break their balance, then finish it.",
            },
            new[]
            {
                "Again. The hours are mine to spend, and I am spending them on you.",
                "A guard raised at the last instant costs you nothing, and costs them their footing.",
            },
            new[]
            {
                "You fell. It is turned back.",
                "The Shards of the dead are only worth something at a fire. Rest. Spend them.",
            },
            new[]
            {
                "Again.",
                "When the ground turns red under you, no guard will hold it. Be somewhere else.",
            },
            new[]
            {
                "Still here. So am I.",
                "They remember nothing of you. That is your whole advantage. Use it.",
            },
        };

        public static readonly string[] AfterAGod =
        {
            "The storm is quiet. One of them is gone.",
            "You took from him as he once took. Do not think about that yet.",
            "There are others. Their roads are not open to me. Not yet.",
            "Go again, if you want the practice. I will turn it back.",
        };

        public static readonly string[] CameBackAlive =
        {
            "You came back on your own feet. That is rare.",
            "The gate is still there. So is he.",
        };

        public static readonly string[] NothingMore =
        {
            "The gate is open.",
        };

        /// <summary>What the sponsor says when the player first speaks to it on this visit to the hub.</summary>
        public static string[] For(bool alreadyMet, int runsEnded, bool lastRunFell, int lastRunGods)
        {
            if (!alreadyMet || runsEnded == 0) return alreadyMet ? NothingMore : FirstWaking;
            if (lastRunGods > 0) return AfterAGod;
            if (!lastRunFell) return CameBackAlive;
            return AfterAFall[(runsEnded - 1) % AfterAFall.Length];
        }
    }
}
