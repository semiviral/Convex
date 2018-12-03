#region USINGS

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Convex.Event;
using Convex.Core.Component;

#endregion

namespace Convex.IRC.Component {
    public sealed class User : INotifyPropertyChanged {
        public User(int id, string nickname, string realname, int access) {
            Id = id;
            Nickname = nickname;
            Realname = realname;
            Access = access;
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

            if (Attempts.Equals(4))
                if (Seen.AddMinutes(1) < DateTime.UtcNow)
                    Attempts = 0; // if so, reset their attempts to 0
                else
                    doTimeout = true; // if not, timeout is true
            else if (Access > 1)
                // if user isn't admin/op, increment their attempts
                Attempts++;

            return doTimeout;
        }

        private void NotifyPropertyChanged(object newValue, [CallerMemberName] string memberName = "") {
            OnPropertyChanged(this, new UserPropertyChangedEventArgs(memberName, Realname, newValue));
        }

        public void OnPropertyChanged(object sender, PropertyChangedEventArgs e) {
            PropertyChanged?.Invoke(sender, e);
        }

        /// <summary>
        ///     Adds a Args object to list
        /// </summary>
        /// <param name="user">user object</param>
        /// <param name="message"><see cref="Message" /> to be added</param>
        public void AddMessage(User user, IMessage message) {
            user.Messages.Add(message);
        }

        #region MEMBERS

        public int Id {
            get => _id;
            set {
                _id = value;
                NotifyPropertyChanged(value);
            }
        }

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

        public ObservableCollection<IMessage> Messages { get; } = new ObservableCollection<IMessage>();
        public List<string> Channels { get; } = new List<string>();
        private int _access;
        private int _attempts;
        private int _id;
        private string _nickname;
        private string _realname;
        private DateTime _seen;

        #endregion
    }
}