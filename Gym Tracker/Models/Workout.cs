namespace Gym_Tracker.Models
{
    public class Workout
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = "Workout";
        public DateOnly Created { get; set; } = DateOnly.FromDateTime(DateTime.Now);
        public List<ExercisePlan> Exercises { get; set; } = new();
        public DateOnly Date { get; set; } = DateOnly.FromDateTime(DateTime.Now);
        public List<SetEntry> Sets { get; set; } = new();
    }
}
