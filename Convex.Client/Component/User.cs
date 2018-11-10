#region USINGS

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Convex.Event;
using Convex.Util;

#endregion

namespace Convex.Client.Component {
    public sealed class User : INotifyPropertyChanged {
        public User(string nickname, string realname, int access) {
            Modes = new List<IrcMode>();

            Nickname = nickname;
            Realname = realname;
            Access = access;
            Seen = DateTime.Now;
        }

        public User(string rawUser) {
            Modes = new List<IrcMode>();

            StringBuilder userName = new StringBuilder();
            foreach (char character in rawUser) {
                if (_modeIdentifiers.Contains(character)) {
                    Modes.Add(new IrcMode(character));
                    continue;
                }

                userName.Append(character);
            }

            Nickname = userName.ToString();
            Realname = userName.ToString();
            Access = 9;
            Seen = DateTime.Now;
        }

        #region INTERFACE IMPLEMENTATION

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        /// <summary>
        ///     Discern whether a user has exceeded command-querying limit
        /// </summary>
        /// <returns>true: user timeout</returns>
        public bool GetTimeout() {
            bool doTimeout = false;

            if (Attempts.Equals(4)) {
                if (Seen.AddMinutes(1) < DateTime.UtcNow) {
                    Attempts = 0; // if so, reset their attempts to 0
                } else {
                    doTimeout = true; // if not, timeout is true
                }
            } else if (Access > 1) {
                // if user isn't admin/op, increment their attempts
                Attempts++;
            }

            return doTimeout;
        }

        private void NotifyPropertyChanged(object newValue, [CallerMemberName] string memberName = "") {
            OnPropertyChanged(this, new UserPropertyChangedEventArgs(memberName, Realname, newValue));
        }

        public void OnPropertyChanged(object sender, PropertyChangedEventArgs e) {
            PropertyChanged?.Invoke(sender, e);
        }

        #region MEMBERS

        private char[] _modeIdentifiers = { '~', '@', '+' };

        public int Attempts {
            get => _attempts;
            set {
                _attempts = value;
                NotifyPropertyChanged(value);
            }
        }

        public string Nickname {
            get => _nickname;
            set {
                _nickname = value;
                NotifyPropertyChanged(value);
            }
        }

        public string Realname {
            get => _realname;
            set {
                _realname = value;
                NotifyPropertyChanged(value);
            }
        }

        public int Access {
            get => _access;
            set {
                _access = value;
                NotifyPropertyChanged(value);
            }
        }

        public DateTime Seen {
            get => _seen;
            set {
                _seen = value;
                NotifyPropertyChanged(value);
            }
        }

        public List<IrcMode> Modes {
            get => _modes;
            private set {
                _modes = value;
                NotifyPropertyChanged(value);
            }
        }

        private int _access;
        private int _attempts;
        private string _nickname;
        private string _realname;
        private DateTime _seen;
        private List<IrcMode> _modes;

        #endregion
    }
}