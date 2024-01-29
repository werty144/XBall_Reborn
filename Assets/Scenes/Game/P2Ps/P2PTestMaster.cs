using Steamworks;

public class P2PTestMaster : P2PMaster
{
    public void DummyReady()
    {
        Server.PeerReady(new CSteamID(1));
    }
}
