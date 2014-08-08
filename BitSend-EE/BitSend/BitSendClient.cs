using System;
using System.Text;
using PlayerIOClient;

namespace BitSend
{
    public class BitSendClient
    {
        private readonly Connection _connection;
        private readonly int _myUserId;
        private readonly ReceiveManager _receiveManager = new ReceiveManager();
        private readonly SendManager _sendManager;
        private readonly UserManager _userManager = new UserManager();

        public BitSendClient(Connection connection, int myUserId)
        {
            this._sendManager = new SendManager(connection);
            this._connection = connection;
            this._myUserId = myUserId;
            this._connection.OnMessage += this._connection_OnMessage;
            this._sendManager.Send(Packet.Hai);
        }

        public void Send(byte[] bytes)
        {
            this._sendManager.Send(bytes);
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
                    if (userId == this._myUserId) break;
                    this._sendManager.Send(Packet.Hey);
                    goto case Packet.Hey;

                case Packet.Hey:
                    if (userId == this._myUserId) break;
                    this._userManager.AddUser(userId);
                    break;

                case Packet.Bai:
                    this._userManager.RemoveUser(userId);
                    break;

                case Packet.BreakChunk:
                    if (!this._userManager.Contains(userId)) break;
                    byte[] bytes = this._receiveManager.BreakChunk(userId);
                    string str = Encoding.Unicode.GetString(bytes);
                    Console.WriteLine(str);
                    break;

                default:
                    if (this._userManager.Contains(userId))
                    {
                        this._receiveManager.HandlePacket(userId, (ChunkPacket)packet);
                    }
                    else if (userId == this._myUserId)
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