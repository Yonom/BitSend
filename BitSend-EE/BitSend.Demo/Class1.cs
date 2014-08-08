using System.Text;
using CupCake;
using CupCake.Messages.Receive;
using CupCake.Players;

namespace BitSend.Demo
{
    public class Class1 : CupCakeMuffin
    {
        protected override void Enable()
        {
            this.Events.Bind<InitReceiveEvent>(this.OnInit);
        }

        private void OnInit(object sender, InitReceiveEvent e)
        {
            var client = new BitSendClient(this.ConnectionPlatform.Connection, this.PlayerService.OwnPlayer.UserId);
            client.Send(Encoding.Unicode.GetBytes("Hello world!"));

            client.Message += this.client_Message;
        }

        private void client_Message(int userId, byte[] data)
        {
            Player p;
            if (this.PlayerService.TryGetPlayer(userId, out p))
            {
                this.Logger.Log(p.Username + ": " + Encoding.Unicode.GetString(data));
            }
        }
    }
}