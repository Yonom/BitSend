using System;
using System.Threading;
using PlayerIOClient;

namespace BitSend
{
    public delegate void UserEventHandler(int userId);

    public delegate void MessageEventHandler(int userId, byte[] data);

    public delegate void StatusEventHandler(int current, int total);

    public class BitSendClient
    {
        private readonly int _myUserId;
        private readonly ReceiveManager _receiveManager = new ReceiveManager();
        private readonly SendManager _sendManager;
        private readonly UserManager _userManager = new UserManager();

        public BitSendClient(Connection connection, int myUserId)
        {
            if (!BitConverter.IsLittleEndian)
                throw new NotSupportedException("BitSendClient only supports little endian machines.");

            if (connection == null)
                throw new ArgumentNullException("connection");

            if (myUserId <= 0)
                throw new ArgumentException("The provided myUserId is invalid.");

            this._sendManager = new SendManager(connection);

            connection.OnMessage += this._connection_OnMessage;
            this._userManager.Add += this.OnAdd;
            this._userManager.Remove += this.OnRemove;
            this._sendManager.Status += this.OnStatus;

            this._myUserId = myUserId;
            this._sendManager.Send(Packet.Hai);
        }

        public bool IsEnabled
        {
            get { return this._sendManager.IsEnabled; }
            set { this._sendManager.IsEnabled = value; }
        }

        public event MessageEventHandler Message;

        protected virtual void OnMessage(int userid, byte[] data)
        {
            MessageEventHandler handler = this.Message;
            if (handler != null) handler(userid, data);
        }

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

        public event StatusEventHandler Status;

        protected virtual void OnStatus(int current, int total)
        {
            StatusEventHandler handler = this.Status;
            if (handler != null) handler(current, total);
        }

        public bool Send(byte[] bytes)
        {
            return this._sendManager.Send(bytes);
        }

        private void _connection_OnMessage(object sender, Message e)
        {
            switch (e.Type)
            {
                case "c":
                    this.OnCoin(e.GetInt(0), e.GetInt(1));
                    break;

                case "left":
                    this.OnLeft(e.GetInt(0));
                    break;
            }
        }

        private void OnCoin(int userId, int coins)
        {
            var packet = (Packet)coins;
            switch (packet)
            {
                case Packet.Hai:
                    if (userId == this._myUserId) break;
                    ThreadPool.QueueUserWorkItem(o => this._sendManager.Send(Packet.Hey));
                    goto case Packet.Hey;

                case Packet.Hey:
                    if (userId == this._myUserId) break;
                    this._userManager.AddUser(userId);
                    break;

                case Packet.Bai:
                    this._userManager.RemoveUser(userId);
                    this._receiveManager.DropQueue(userId);
                    break;

                case Packet.StartChunk:
                    if (!this._userManager.Contains(userId)) break;
                    this._receiveManager.StartChunk(userId);
                    break;

                case Packet.EndChunk:
                    if (!this._userManager.Contains(userId)) break;
                    byte[] bytes = this._receiveManager.EndChunk(userId);
                    this.OnMessage(userId, bytes);
                    break;

                default:
                    if (this._userManager.Contains(userId))
                    {
                        this._receiveManager.HandlePacket(userId, coins);
                    }
                    else if (userId == this._myUserId)
                    {
                        this._sendManager.HandlePacket(coins);
                    }

                    break;
            }
        }

        private void OnLeft(int userId)
        {
            this._userManager.RemoveUser(userId);
        }
    }
}