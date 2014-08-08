using System.Collections.Generic;

namespace BitSend
{
    internal class UserManager
    {
        public event UserEventHandler Add;

        protected virtual void OnAdd(int userid)
        {
            UserEventHandler handler = this.Add;
            if (handler != null) handler(userid);
        }

        public event UserEventHandler Remove;

        protected virtual void OnRemove(int userid)
        {
            UserEventHandler handler = this.Remove;
            if (handler != null) handler(userid);
        }

        private readonly HashSet<int> _users = new HashSet<int>();

        public void AddUser(int userId)
        {
            if (this._users.Add(userId))
                OnAdd(userId);
        }

        public void RemoveUser(int userId)
        {
            if (this._users.Remove(userId))
                this.OnRemove(userId);
        }

        public bool Contains(int userId)
        {
            return this._users.Contains(userId);
        }
    }
}