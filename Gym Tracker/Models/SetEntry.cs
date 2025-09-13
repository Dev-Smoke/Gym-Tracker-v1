namespace Gym_Tracker.Models
{
    public class SetEntry
    {
        public int Reps { get; set; }
        public decimal Weight { get; set; }
        public DateTime Ts { get; set; } = DateTime.Now;
        public int Rpe { get; set; } = 7; // optional
        public bool IsWarmup { get; set; } = false; // optional
        public string? Note { get; set; }
        public bool IsPR { get; set; } = false;

    }
}

