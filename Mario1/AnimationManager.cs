using System;
using System.Windows.Forms;

namespace Mario1
{
    public static class AnimationManager
    {
        private static System.Windows.Forms.Timer _animTimer = new System.Windows.Forms.Timer();
        private static Action<int> _frameAction;
        private static Action _completeAction;
        public static int _currentFrame;
        public static int _maxFrames { get; private set; }

        // Публичный флаг. Проверяем его в GameTimer_Tick
        public static bool IsAnimating { get; set; } = false;

        static AnimationManager()
        {
            _animTimer.Tick += (s, e) =>
            {
                _frameAction?.Invoke(_currentFrame);
                _currentFrame++;
                if (_currentFrame >= _maxFrames) Finish();
            };
        }

        // Очистка
        public static void Clean()
        {
            _animTimer?.Stop();
            _animTimer?.Dispose();
        }

        /// <param name="durationMs">Общая длительность в мс</param>
        /// <param name="intervalMs">Частота обновления кадров (20-30 мс для плавности)</param>
        /// <param name="onFrame">Логика кадра (принимает номер текущего кадра)</param>
        /// <param name="onComplete">Что выполнить после анимации</param>
        public static void PlayAnimation(int durationMs, int intervalMs, Action<int> onFrame, Action onComplete)
        {
            if (IsAnimating) return; // Защита от повторного запуска

            IsAnimating = true;
            _maxFrames = durationMs / intervalMs;
            _currentFrame = 0;
            _frameAction = onFrame;
            _completeAction = onComplete;

            _animTimer.Interval = intervalMs;
            _animTimer.Start();
        }

        private static void Finish()
        {
            _animTimer.Stop();
            IsAnimating = false;
            _completeAction?.Invoke();
        }
    }
}