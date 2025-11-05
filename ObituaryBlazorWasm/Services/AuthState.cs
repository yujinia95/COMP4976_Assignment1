using System;

namespace ObituaryBlazorWasm.Services
{
    public class AuthState
    {
        public event Action? OnChange;

        private bool _isLoggedIn;

        public bool IsLoggedIn
        {
            get => _isLoggedIn;
            set
            {
                if (_isLoggedIn != value)
                {
                    _isLoggedIn = value;
                    NotifyStateChanged();
                }
            }
        }

        private void NotifyStateChanged()
        {
            Console.WriteLine($"AuthState changed: IsLoggedIn = {_isLoggedIn}");
            OnChange?.Invoke();
        }

    }
}

