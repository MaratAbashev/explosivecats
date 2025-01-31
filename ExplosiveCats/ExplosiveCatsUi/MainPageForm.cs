using System.Security.Permissions;
using ExplosiveCatsClient;
using ExplosiveCatsEnums;

namespace ExplosiveCatsUi;

public partial class MainPageForm : Form
{
    private readonly Client _client = new();
    private byte _playerId;
    private byte _playersCount;
    private Card? _selectedCard;
    private byte _currentPlayerId;
    private Game _game;
    public MainPageForm()
    {
        InitializeComponent();
        ConnectToServer();
    }
    
    private async void ConnectToServer()
    {
        _playerId = await _client.StartClient("localhost", 5000);
            
        if (_playerId == byte.MaxValue)
        {
            MessageBox.Show("Connection failed!");
            Close();
            return;
        }

        labelStatus.Text = $"Вы игрок {_playerId + 1}";
        buttonReady.Visible = true;
    }
    
    private async void btnReady_Click(object sender, EventArgs e)
    {
        await _client.GetReady(_playerId);
        buttonReady.Enabled = false;
        StartListening();
    }
    
    private async void StartListening()
    {
        while (_client.Connected)
        {
            var result = await _client.GetResponse();
            ProcessServerResponse(result);
        }
        MessageBox.Show("Connection failed!");
    }
    
    private void ProcessServerResponse(Result result)
    {
        switch (result.Action)
        {
            case ServerActionType.StartGame:
                InitializeGame(result);
                break;
            case ServerActionType.Explode:
                HandleExplosion(result);
                break;
            case ServerActionType.PlayCard:
                _game.ProcessCardPlay(result.Cards![0], result.PlayerId);
                break;
            case ServerActionType.PlayDefuse:
                _game.PlayDefuse(result.Cards![0]);
                break;
            case ServerActionType.GiveCard:
                _game.GiveCard(result.PlayerId, result.Cards![0], result.AnotherPlayerId);
                break;
            case ServerActionType.TakeCard when result.Cards![0].CardType == CardType.ExplosiveCat:
                HandleDefuse();
                //_game.PlayDefuse();
                break;
        }

        if (result.Action == ServerActionType.StartGame)
        {
            InitializeGame(result);
        }
        
    }

    private void HandleDefuse()
    {
        
    }
    
    private void InitializeGame(Result result)
    {
        var playerList = new List<Player>();
        for (byte i = 0; i < result.PlayerCount; i++)
        {
            if (_playerId == i)
            {
                playerList.Add(new MainPlayer(i, result.Cards!));
                continue;
            }
            playerList.Add(new Player(i));
        }
        _game = new Game(playerList);
        flpHand.AutoScroll = true;
        flpHand.WrapContents = false;
        flpHand.FlowDirection = FlowDirection.LeftToRight;
        buttonReady.Visible = false;
        UpdateTurnState();
    }
    
    private void UpdateTurnState()
    {
        
        // foreach (Control c in pnlCards.Controls)
        //     c.Enabled = (_currentPlayerId == _playerId);
    }
    
    private void UpdateHandLayout()
    {
        // Автоматическое обновление прокрутки
        flpHand.PerformLayout();
        flpHand.AutoScrollMinSize = new Size(
            flpHand.Controls.Count * 110, 
            flpHand.Height
        );
    }
    
    private void HandleExplosion(Result result)
    {
        MessageBox.Show($"Взорвался игрок номер {result.PlayerId + 1}");
        _game.Players[result.PlayerId].Exploded = true;
    }

    private void MainPageForm_Resize(object sender, EventArgs e)
    {
        labelStatus.Location = new Point((this.ClientSize.Width - labelStatus.Width) / 2, 20);
    }

    private void buttonReady_MouseHover(object sender, EventArgs e)
    {
        buttonReady.BackColor = Color.Firebrick;
    }

    private async void buttonReady_Click(object sender, EventArgs e)
    {
        await _client.GetReady(_playerId);
    }
}