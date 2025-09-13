namespace Gym_Tracker.Models
{
    public class CompletedSession
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid WorkoutId { get; set; }
        public string WorkoutName { get; set; } = "Workout";
        public DateTime CompletedAt { get; set; } = DateTime.Now;
        public List<CompletedExercise> Exercises { get; set; } = new();

        public int TotalSets => Exercises.Sum(e => e.Sets.Count);
        public decimal TotalVolume => Exercises.Sum(e => e.Sets.Sum(s => s.Weight * s.Reps));
    }

    public class CompletedExercise
    {
        public string Title { get; set; } = string.Empty;
        public List<SetEntry> Sets { get; set; } = new();
    }
}
