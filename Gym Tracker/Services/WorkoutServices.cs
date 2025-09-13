namespace Gym_Tracker.Services
{
    using Blazored.LocalStorage;
    using Gym_Tracker.Models;

    public class WorkoutService
    {
        const string Key = "gymtracker.v1";
        private readonly ILocalStorageService _ls;
        public List<string> Exercises { get; private set; } = new() { "KH-Bankdrücken", "Latziehen", "Seitheben", "Beinpresse" };
        public List<Workout> Workouts { get; private set; } = new();

        public WorkoutService(ILocalStorageService ls) => _ls = ls;

        public async Task LoadAsync()
        {
            var state = await _ls.GetItemAsync<AppState>(Key) ?? new();
            Exercises = state.Exercises; Workouts = state.Workouts;
        }
        public async Task SaveAsync() => await _ls.SetItemAsync(Key, new AppState { Exercises = Exercises, Workouts = Workouts });

        public Workout EnsureToday(string name)
        {
            var today = DateOnly.FromDateTime(DateTime.Now);
            var wk = Workouts.FirstOrDefault(w => w.Date == today && w.Name == name);
            if (wk == null) { wk = new Workout { Name = name, Date = today }; Workouts.Add(wk); }
            return wk;
        }

        class AppState { public List<string> Exercises { get; set; } = new(); public List<Workout> Workouts { get; set; } = new(); }
    }

}
