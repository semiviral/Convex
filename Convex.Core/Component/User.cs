#region

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Convex.Core.Events;

#endregion

namespace Convex.Core.Component
{
    public sealed class User : INotifyPropertyChanged
    {
        public User(int id, string nickname, string realname, int access)
        {
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
        public bool GetTimeout()
        {
            bool doTimeout = false;

            if (Attempts.Equals(4))
            {
                if (Seen.AddMinutes(1) < DateTime.UtcNow)
                {
                    Attempts = 0; // if so, reset their attempts to 0
                }
                else
                {
                    doTimeout = true; // if not, timeout is true
                }
            }
            else if (Access > 1)
                // if user isn't admin/op, increment their attempts
            {
                Attempts++;
            }

            return doTimeout;
        }

        private void NotifyPropertyChanged(object newValue, [CallerMemberName] string memberName = "")
        {
            OnPropertyChanged(this, new UserPropertyChangedEventArgs(memberName, Realname, newValue));
        }

        public void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(sender, e);
        }

        /// <summary>
        ///     Adds a Args object to list
        /// </summary>
        /// <param name="user">user object</param>
        /// <param name="message"><see cref="Message" /> to be added</param>
        public void AddMessage(User user, IMessage message)
        {
            user.Messages.Add(message);
        }

        #region MEMBERS

        public int Id
        {
            get => _Id;
            set
            {
                _Id = value;
                NotifyPropertyChanged(value);
            }
        }

        public int Attempts
        {
            get => _Attempts;
            set
            {
                _Attempts = value;
                NotifyPropertyChanged(value);
            }
        }

        public string Nickname
        {
            get => _Nickname;
            set
            {
                _Nickname = value;
                NotifyPropertyChanged(value);
            }
        }

        public string Realname
        {
            get => _Realname;
            set
            {
                _Realname = value;
                NotifyPropertyChanged(value);
            }
        }

        public int Access
        {
            get => _Access;
            set
            {
                _Access = value;
                NotifyPropertyChanged(value);
            }
        }

        public DateTime Seen
        {
            get => _Seen;
            set
            {
                _Seen = value;
                NotifyPropertyChanged(value);
            }
        }

        public ObservableCollection<IMessage> Messages { get; } = new ObservableCollection<IMessage>();
        public List<string> Channels { get; } = new List<string>();
        private int _Access;
        private int _Attempts;
        private int _Id;
        private string _Nickname;
        private string _Realname;
        private DateTime _Seen;

        #endregion
    }
}