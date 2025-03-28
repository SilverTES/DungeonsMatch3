using Microsoft.Xna.Framework;
using System;

namespace DungeonsMatch3
{
    public class TimerAI
    {
        // Propriétés principales
        public float Duration { get; private set; } // Durée totale en secondes
        public float RemainingTime { get; private set; } // Temps restant
        public bool IsRunning { get; private set; } // État du timer
        public bool IsPaused { get; private set; } // État de pause
        public bool Loops { get; set; } // Si true, le timer boucle

        // Améliorations ajoutées
        public float Progress => RemainingTime > 0 ? 1 - (RemainingTime / Duration) : 1; // Progression de 0 à 1

        // Événements
        public event Action OnTimerCompleted; // Quand le timer finit
        public event Action<float> OnTimerTick; // À chaque mise à jour (temps restant)
        public event Action OnTimerStarted; // Quand le timer démarre (nouvelle amélioration)

        // Constructeur
        public TimerAI(float duration, bool loops = false)
        {
            if (duration <= 0) throw new ArgumentException("Duration must be greater than 0");
            Duration = duration;
            RemainingTime = duration;
            IsRunning = false;
            IsPaused = false;
            Loops = loops;
        }

        // Démarrer le timer
        public void Start()
        {
            if (!IsRunning)
            {
                RemainingTime = Duration;
                IsRunning = true;
                IsPaused = false;
                OnTimerStarted?.Invoke(); // Déclenche l'événement de démarrage
            }
        }

        // Arrêter le timer
        public void Stop()
        {
            IsRunning = false;
            IsPaused = false;
            RemainingTime = Duration;
        }

        // Mettre en pause
        public void Pause()
        {
            if (IsRunning && !IsPaused)
                IsPaused = true;
        }

        // Reprendre
        public void Resume()
        {
            if (IsRunning && IsPaused)
                IsPaused = false;
        }

        // Mettre à jour le timer
        public void Update(GameTime gameTime)
        {
            if (!IsRunning || IsPaused)
                return;

            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            RemainingTime -= deltaTime;

            OnTimerTick?.Invoke(RemainingTime); // Événement à chaque tick

            if (RemainingTime <= 0)
            {
                OnTimerCompleted?.Invoke(); // Événement de fin
                if (Loops)
                {
                    RemainingTime = Duration; // Redémarre si boucle
                }
                else
                {
                    IsRunning = false;
                    RemainingTime = 0;
                }
            }
        }

        // Réinitialiser le timer
        public void Reset()
        {
            RemainingTime = Duration;
            IsRunning = false;
            IsPaused = false;
        }

        // Changer la durée (nouvelle amélioration)
        public void SetDuration(float newDuration)
        {
            if (newDuration <= 0) throw new ArgumentException("New duration must be greater than 0");
            Duration = newDuration;
            if (RemainingTime > Duration)
                RemainingTime = Duration; // Ajuste le temps restant si nécessaire
        }
    }
}
