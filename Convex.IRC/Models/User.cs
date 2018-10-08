#region usings

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Convex.Event;

#endregion

namespace Convex.IRC.Models {
    public sealed class User : INotifyPropertyChanged {
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

        public ObservableCollection<Message> Messages { get; } = new ObservableCollection<Message>();
        public List<string> Channels { get; } = new List<string>();
        private int _access;
        private int _attempts;
        private int _id;
        private string _nickname;
        private string _realname;
        private DateTime _seen;

        #endregion

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
        public void AddMessage(User user, Message message) {
            user.Messages.Add(message);
        }
    }

    public class Message {
        #region MEMBERS

        public int Id { get; }
        public string Sender { get; }
        public string Contents { get; }
        public DateTime Date { get; }

        #endregion

        public Message(int id, string sender, string contents, DateTime timestamp) {
            Id = id;
            Sender = sender;
            Contents = contents;
            Date = timestamp;
        }
    }
}
