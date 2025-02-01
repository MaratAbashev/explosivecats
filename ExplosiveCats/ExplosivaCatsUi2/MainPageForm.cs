using ExplosiveCatsClient;
using ExplosiveCatsEnums;

namespace ExplosivaCatsUi2;

public partial class MainPageForm : Form
{
    private readonly Client _client = new();
    private byte _playerId;
    private byte _playersCount;
    private Card? _selectedCard;
    private byte _currentPlayerId => _game.CurrentPlayer.PlayerId;
    private Game _game;
    private MainPlayer? _mainPlayer;
    private int insertionId;

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
    
    private async void buttonReady_Click(object sender, EventArgs e)
    {
        try
        {
            await _client.GetReady(_playerId);
            buttonReady.Enabled = false;
            buttonReady.Visible = false;
            
            // Запускаем прослушивание в фоновом режиме
            _ = Task.Run(StartListeningAsync);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка: {ex.Message}");
        }
    }
    
    private async void StartListeningAsync()
    {
        while (_client.Connected)
        {
            var result = await _client.GetResponse();
            Invoke(() => ProcessServerResponse(result));
        }
        MessageBox.Show("Connection failed!");
    }
    
    private void ProcessServerResponse(Result result)
    {
        switch (result.Action)
        {
            case ServerActionType.StartGame:
                InitializeGame(result);
                UpdateGameUI("Игра создана");
                break;
            case ServerActionType.Explode:
                HandleExplosion(result);
                break;
            case ServerActionType.PlayCard when result.Cards![0].CardType == CardType.SeeTheFuture:
                string message =
                    $"Игрок номер {result.PlayerId + 1} сходил картой {result.Cards[0].CardType.ToString()}";
                if (_game.IsYourTurn)
                {
                    message += $" Ваши подсмотренные карты: {string.Join(' ', result.Cards.Select(c => c.CardType.ToString()).Skip(1))}";
                }
                _game.ProcessCardPlay(result.Cards![0], result.PlayerId);
                UpdateGameUI(message);
                UpdateOtherPlayers();
                break;
            case ServerActionType.PlayCard when result.Cards[0].CardType != CardType.SeeTheFuture:
                _game.ProcessCardPlay(result.Cards![0], result.PlayerId);
                UpdateTurnState(_mainPlayer!.Cards);
                UpdateGameUI($"Игрок номер {result.PlayerId + 1} сходил картой {result.Cards[0].CardType.ToString()}");
                UpdateOtherPlayers();
                break;
            case ServerActionType.PlayDefuse:
                Console.WriteLine(result.PlayerId);
                Console.WriteLine(result.Cards![0].CardType.ToString());
                Console.WriteLine(_game.CurrentPlayer.PlayerId);
                Console.WriteLine(_game.IsYourTurn);
                Console.WriteLine();
                if (!_game.IsYourTurn)
                    _game.PlayDefuse(result.Cards![0]);
                Console.WriteLine(result.PlayerId);
                Console.WriteLine(result.Cards![0].CardType.ToString());
                Console.WriteLine(_game.CurrentPlayer.PlayerId);
                Console.WriteLine(_game.IsYourTurn);
                Console.WriteLine();
                UpdateGameUI($"Игрок номер {result.PlayerId + 1} обезвредил взрывкота");
                UpdateOtherPlayers();
                break;
            case ServerActionType.GiveCard:
                _game.GiveCard(result.PlayerId, result.Cards![0], result.AnotherPlayerId);
                UpdateGameUI($"Игрок номер {result.PlayerId + 1} дал карту игроку номер {result.AnotherPlayerId + 1}");
                UpdateOtherPlayers();
                break;
            case ServerActionType.TakeCard when result.Cards![0].CardType == CardType.ExplosiveCat && result.PlayerId == _playerId:
                HandleDefuse();
                Console.WriteLine(result.PlayerId);
                Console.WriteLine(result.Cards![0].CardType.ToString());
                Console.WriteLine(_game.CurrentPlayer.PlayerId);
                Console.WriteLine();
                break;
            case ServerActionType.TakeCard when result.Cards![0].CardType == CardType.ExplosiveCat && result.PlayerId != _playerId:
                UpdateGameUI($"Игрок номер {result.PlayerId + 1} выбирает куда засунуть взрывного кота");
                UpdateOtherPlayers();
                break;
            case ServerActionType.TakeCard when result.Cards![0].CardType != CardType.ExplosiveCat:
                if (_game.IsYourTurn)
                {
                    var cardList = _mainPlayer!.Cards;
                    cardList.Add(result.Cards![0]);
                    UpdateTurnState(cardList);
                }
                _game.TakeCard(result.Cards![0]);
                UpdateGameUI($"Игрок номер {result.PlayerId + 1} получил карту {result.Cards![0].CardType.ToString()}");
                UpdateOtherPlayers();
                break;
        }
    }

    private void HandleDefuse()
    {
        btnTake.Enabled = false;
        btnPlay.Enabled = false;
        insertion.Visible = true;
        buttonDefuse.Visible = true;
        indexLabel.Visible = true;
        insertion.Maximum = _game.DeckCount - 1;
    }
    
    
    private void UpdateGameUI(string message)
    {
        labelMove.Visible = true;
        btnPlay.Visible = true;
        btnPlay.Enabled = true;
        btnTake.Visible = true;
        btnTake.Enabled = true;
        labelMove.Text = message;
        // Обновляем статус текущего хода
        labelStatus.Text = _game.IsYourTurn 
            ? "Ваш ход!" 
            : $"Ход игрока {_currentPlayerId + 1}";
    }
    
    private void InitializeGame(Result result)
    {
        var playerList = new List<Player>();
        _mainPlayer = new MainPlayer(_playerId, result.Cards!);;
        for (byte i = 0; i < result.PlayerCount; i++)
        {
            if (_playerId == i)
            { 
                playerList.Add(_mainPlayer);
                continue;
            }
            playerList.Add(new Player(i));
        }
        _game = new Game(playerList, _playerId);
        otherPlayerInfo.Visible = true;
        UpdateTurnState(result.Cards!);
        UpdateOtherPlayers();
    }

    public void UpdateOtherPlayers()
    {
        var otherPlayers = _game.Players.Where(p => p.PlayerId != _playerId).ToList();
        var text = "";
        foreach (var otherPlayer in otherPlayers)
        {
            text += $"Номер: {otherPlayer.PlayerId + 1}, карт: {otherPlayer.CardsCount}";
        }
        otherPlayerInfo.Text = text;
    }
    
    private void UpdateTurnState(List<Card> cards)
    {
        cardPanel.Controls.Clear();
        AddRadioButtons(cards);
        ArrangeRadioButtons();
    }
    
    private void AddRadioButtons(List<Card> cards)
    {
        try
        {
            foreach (var card in cards)
            {
                RadioButton rb = new RadioButton();
                rb.Text = card.CardType.ToString();
                rb.Width = 120; // Ширина радиокнопки
                rb.Height = 25; // Высота радиокнопки
                rb.Margin = new Padding(0);
                rb.Tag = card;
                rb.CheckedChanged += RadioButton_CheckedChanged;

                cardPanel.Controls.Add(rb);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    private void ArrangeRadioButtons()
    {
        int startX = 10;
        int startY = 10; 
        int spacingX = 10; 
        int spacingY = 10; 
        int maxWidth = cardPanel.Width - startX; 
        int buttonWidth = 120; 
        int buttonHeight = 25; 
        int columns = 2; 

        int x = startX;
        int y = startY;
        int currentColumn = 0;

        foreach (Control control in cardPanel.Controls)
        {
            if (x + buttonWidth > maxWidth)
            {
                x = startX;
                y += buttonHeight + spacingY;
                currentColumn = 0;
            }
            
            control.Location = new Point(x, y);

            x += buttonWidth + spacingX;
            currentColumn++;
        }

    }
    
    private void RadioButton_CheckedChanged(object sender, EventArgs e)
    {
        RadioButton rb = sender as RadioButton;
        if (rb != null && rb.Checked)
        {
            _selectedCard = rb.Tag as Card;
        }
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
    
    private async void buttonPlay_Click(object sender, EventArgs e)
    {
        await _client.PlayCard(_playerId, _selectedCard);
    }

    private async void btnTake_Click(object sender, EventArgs e)
    {
        await _client.TakeCard(_playerId);
    }

    private void insertion_ValueChanged(object sender, EventArgs e)
    {
        insertionId = insertion.Value;
        indexLabel.Text = $"Индекс: {insertionId}";
    }

    private async void buttonDefuse_Click(object sender, EventArgs e)
    {
        await _client.PlayDefuse(_playerId, _selectedCard, (byte)insertionId);
        _game.PlayDefuse(_selectedCard);
        insertion.Visible = false;
        buttonDefuse.Visible = false;
        indexLabel.Visible = false;
        btnPlay.Enabled = true;
        btnTake.Enabled = true;
        UpdateTurnState(_mainPlayer!.Cards);
    }
    
}