using System;
using System.Text;
using System.Threading;
using PlayerIOClient;

namespace BitSend
{
    public class BitSendClient
    {
        private readonly UserManager _userManager = new UserManager();
        private readonly ReceiveManager _receiveManager = new ReceiveManager();
        private readonly SendManager _sendManager;
        private readonly Connection _connection;
        private readonly int _myUserId;

        public BitSendClient(Connection connection, int myUserId)
        {
            this._sendManager = new SendManager(this);
            this._connection = connection;
            this._myUserId = myUserId;
            this._connection.OnMessage += _connection_OnMessage;
            this.SendPacket((int)Packet.Hai);
        }

        public void Send(byte[] bytes)
        {
            this._sendManager.Send(bytes);
        }

        internal void SendPacket(int packet)
        {
            this._connection.Send("c", packet, 0, 0);
        }

        private void _connection_OnMessage(object sender, Message e)
        {
            if (e.Type == "c")
            {
                Console.WriteLine("c = {}" + e.GetInt(1));
                this.OnCoin(e.GetInt(0), e.GetInt(1));
            }
            else if (e.Type == "left")
            {
                this.OnLeft(e.GetInt(0));
            }
        }

        private void OnCoin(int userId, int coins)
        {
            var packet = (Packet)coins;
            switch (packet)
            {
                case Packet.Hai:
                    if (userId == _myUserId) break;
                    this.SendPacket((int)Packet.Hey);
                    goto case Packet.Hey;

                case Packet.Hey:
                    if (userId == _myUserId) break;
                    this._userManager.AddUser(userId);
                    break;

                case Packet.Bai:
                    this._userManager.RemoveUser(userId);
                    break;

                case Packet.BreakChunk:
                    if (!this._userManager.Contains(userId)) break;
                    var bytes = this._receiveManager.BreakChunk(userId);
                    var str = Encoding.Unicode.GetString(bytes);
                    Console.WriteLine(str);
                    break;

                default:
                    if (this._userManager.Contains(userId))
                    {
                        this._receiveManager.HandlePacket(userId, (ChunkPacket)packet);
                    }
                    else if (userId == _myUserId)
                    {
                        this._sendManager.HandlePacket((ChunkPacket)packet);
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