using System.Collections.Generic;

namespace BitSend
{
    internal class UserManager
    {
        private readonly HashSet<int> _users = new HashSet<int>();

        public void AddUser(int userId)
        {
            this._users.Add(userId);
        }

        public void RemoveUser(int userId)
        {
            this._users.Remove(userId);
        }

        public bool Contains(int userId)
        {
            return this._users.Contains(userId);
        }
    }
}