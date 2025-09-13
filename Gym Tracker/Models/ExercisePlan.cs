namespace Gym_Tracker.Models
{
    public class ExercisePlan
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Title { get; set; } = string.Empty; // z.B. KH-Bankdrücken
        public int TargetSets { get; set; } = 3; // Soll-Sätze
        public int TargetReps { get; set; } = 8; // Soll-Wdh
        public decimal StartingWeight { get; set; } = 0m; // Start kg (Vorschlag)
        public List<SetEntry> Log { get; set; } = new(); // echte Sätze
    }
}
