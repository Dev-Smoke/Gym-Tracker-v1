namespace Gym_Tracker.Services
{
    using Blazored.LocalStorage;
    using Gym_Tracker.Models;
    using System.Text.Json;

    public sealed class AppState
    {
        private const string Key = "gymtracker.v3"; // v3: enthält WarmupPresets
        private readonly ILocalStorageService _ls;

        public event Action? OnChange;

        public List<Workout> Workouts { get; private set; } = new();
        public List<CompletedSession> Sessions { get; private set; } = new();

        // NEU: globale Warmup-Schemata
        public List<WarmupPreset> WarmupPresets { get; private set; } = new();

        public AppState(ILocalStorageService ls) => _ls = ls;

        private class AppData
        {
            public List<Workout> Workouts { get; set; } = new();
            public List<CompletedSession> Sessions { get; set; } = new();
            public List<WarmupPreset> WarmupPresets { get; set; } = new();
        }

        public async Task InitializeAsync()
        {
            var data = await _ls.GetItemAsync<AppData>(Key);
            if (data is not null)
            {
                Workouts = data.Workouts ?? new();
                Sessions = data.Sessions ?? new();
                WarmupPresets = data.WarmupPresets ?? DefaultPresets();
            }
            else
            {
                // Migration von älteren Keys:
                Workouts = await _ls.GetItemAsync<List<Workout>>("gymtracker.v2") ?? DemoData();
                Sessions = new();
                WarmupPresets = DefaultPresets();
                await SaveAsync();
            }
            Notify();
        }

        public async Task SaveAsync()
        {
            var data = new AppData { Workouts = Workouts, Sessions = Sessions, WarmupPresets = WarmupPresets };
            await _ls.SetItemAsync(Key, data);
            Notify();
        }

        private void Notify() => OnChange?.Invoke();

        // ====== Workouts ======
        public Workout CreateWorkout(string name) { var w = new Workout { Name = name }; Workouts.Add(w); return w; }
        public void RemoveWorkout(Guid id) => Workouts.RemoveAll(w => w.Id == id);

        // WarmupPresets verwalten
        public async Task AddWarmupPreset(WarmupPreset p) { WarmupPresets.Add(p); await SaveAsync(); }
        public async Task RemoveWarmupPreset(int index)
        {
            if (index >= 0 && index < WarmupPresets.Count) WarmupPresets.RemoveAt(index);
            await SaveAsync();
        }

        // Sessions
        public async Task AddCompletedSession(CompletedSession session) { Sessions.Add(session); await SaveAsync(); }
        public async Task RemoveSession(Guid id) { Sessions.RemoveAll(s => s.Id == id); await SaveAsync(); }

        // Stats (Warmups exkl.)
        public (int totalSets, decimal totalVolume) Stats()
        {
            var workSets = Workouts.SelectMany(w => w.Exercises).SelectMany(e => e.Log).Where(s => !s.IsWarmup);
            return (workSets.Count(), workSets.Aggregate(0m, (a, s) => a + s.Weight * s.Reps));
        }

        private static List<WarmupPreset> DefaultPresets() => new()
        {
            new WarmupPreset{ Label = "50% × 5", Percent = 0.50m, Reps = 5 },
            new WarmupPreset{ Label = "70% × 3", Percent = 0.70m, Reps = 3 },
            new WarmupPreset{ Label = "85% × 1–2", Percent = 0.85m, Reps = 1 },
        };

        private static List<Workout> DemoData() => new()
        {
            new Workout
            {
                Name = "Push",
                Exercises = new()
                {
                    new ExercisePlan{ Title = "KH-Bankdrücken", TargetSets=3, TargetReps=8, StartingWeight=20 },
                    new ExercisePlan{ Title = "Schrägbank Kurzhantel", TargetSets=3, TargetReps=10, StartingWeight=16 },
                    new ExercisePlan{ Title = "Seitheben", TargetSets=3, TargetReps=15, StartingWeight=8 }
                }
            },
            new Workout
            {
                Name = "Pull",
                Exercises = new()
                {
                    new ExercisePlan{ Title = "Latziehen", TargetSets=4, TargetReps=10, StartingWeight=60 },
                    new ExercisePlan{ Title = "Rudern Kabel", TargetSets=3, TargetReps=12, StartingWeight=50 }
                }
            },
            new Workout
            {
                Name = "Beine",
                Exercises = new()
                {
                    new ExercisePlan{ Title = "Beinpresse", TargetSets=4, TargetReps=10, StartingWeight=120 },
                    new ExercisePlan{ Title = "Beinstrecker", TargetSets=3, TargetReps=12, StartingWeight=40 },
                    new ExercisePlan{ Title = "Beincurls", TargetSets=3, TargetReps=12, StartingWeight=35 }
                }
            }
        };

        // ====== Export/Backup ======
        public string ExportSessionsCsv()
        {
            var rows = new List<string> { "SessionId;WorkoutName;CompletedAt;Exercise;Reps;Weight;RPE;IsWarmup;Note;IsPR" };
            foreach (var s in Sessions)
            {
                foreach (var ex in s.Exercises)
                    foreach (var set in ex.Sets)
                    {
                        rows.Add(string.Join(';', new[]
                        {
                        s.Id.ToString(),
                        Escape(s.WorkoutName),
                        s.CompletedAt.ToString("o"),
                        Escape(ex.Title),
                        set.Reps.ToString(),
                        set.Weight.ToString(System.Globalization.CultureInfo.InvariantCulture),
                        set.Rpe.ToString(),
                        set.IsWarmup ? "1" : "0",
                        Escape(set.Note ?? ""),
                        set.IsPR ? "1" : "0"
                    }));
                    }
            }
            return string.Join("\n", rows);
        }

        public string ExportAllCsv()
        {
            var rows = new List<string> { "WorkoutName;Exercise;Ts;Reps;Weight;RPE;IsWarmup;Note;IsPR" };
            foreach (var w in Workouts)
                foreach (var ex in w.Exercises)
                    foreach (var set in ex.Log)
                    {
                        rows.Add(string.Join(';', new[]
                        {
                            Escape(w.Name),
                            Escape(ex.Title),
                            set.Ts.ToString("o"),
                            set.Reps.ToString(),
                            set.Weight.ToString(System.Globalization.CultureInfo.InvariantCulture),
                            set.Rpe.ToString(),
                            set.IsWarmup ? "1" : "0",
                            Escape(set.Note ?? ""),
                            set.IsPR ? "1" : "0"
                        }));
                    }
            return string.Join("\n", rows);
        }

        public string ExportBackupJson()
        {
            var data = new AppData { Workouts = Workouts, Sessions = Sessions, WarmupPresets = WarmupPresets };
            return JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
        }

        public async Task ImportBackupJson(string json)
        {
            var data = JsonSerializer.Deserialize<AppData>(json);
            if (data is null) return;
            Workouts = data.Workouts ?? new();
            Sessions = data.Sessions ?? new();
            WarmupPresets = data.WarmupPresets ?? DefaultPresets();
            await SaveAsync();
        }

        private static string Escape(string s) => "\"" + s.Replace("\"", "\"\"") + "\"";
    }
}
