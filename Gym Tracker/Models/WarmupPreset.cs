namespace Gym_Tracker.Models
{
    public class WarmupPreset
    {
        public string Label { get; set; } = "";   // z. B. "70% × 3"
        public decimal Percent { get; set; }      // 0.70m
        public int Reps { get; set; }             // 3
    }
}
