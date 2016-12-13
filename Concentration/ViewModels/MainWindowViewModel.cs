using Concentration.Models;
using Concentration.Utilities;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Media;
using System.Linq;
using System.Windows.Threading;
using System.Windows;

namespace Concentration.ViewModels
{
    public class MainWindowViewModel : PropertyChangedNotifier, IDisposable
    {
        private const int SQUARE_COUNT = 25;
        private const int STAGE_COUNT = 3;
        private const int FIRST_STAGE = 0;
        private const int TIME_TO_GUESS = 10; // seconds

        // I have 25 spaces, -1 = 24 / 2 = 12 prizes
        // Same prizes each time just randomize their placement and the reebus
        private List<string> _puzzles;
        private List<string> _prizeList;
        private ObservableCollection<Square> _squares;
        private string _puzzleImageSource;
        private List<string> _puzzleImageLocations;
        private int _stage;
        private MediaPlayer _squareOpenPlayer;
        private MediaPlayer _wildPlayer;
        private MediaPlayer _matchPlayer;
        private MediaPlayer _guessTimeExpiredPlayer;
        private bool _timerVisible;
        private int _timeLeft;
        private DispatcherTimer _timer;
        private bool _matchFound;
        private bool _noMatchFound;
        private bool _guessTimerRunning;
        private Random _seed;

        #region Properties

        public ObservableCollection<Square> Squares
        {
            get { return _squares; }
            set
            {
                if (_squares != value)
                {
                    _squares = value;
                    OnPropertyChanged(nameof(Squares));
                }
            }
        }

        public string PuzzleImageSource
        {
            get { return _puzzleImageSource; }
            set
            {
                if (string.Compare(_puzzleImageSource, value, StringComparison.InvariantCulture) != 0)
                {
                    _puzzleImageSource = value;
                    OnPropertyChanged(nameof(PuzzleImageSource));
                }
            }
        }

        public int Stage
        {
            get { return _stage; }
            set
            {
                if (_stage != value)
                {
                    if (value >= STAGE_COUNT)
                    {
                        _stage = FIRST_STAGE;
                    }
                    else
                    {
                        _stage = value;
                    }

                    OnPropertyChanged(nameof(Stage));
                }
            }
        }

        public int TimeLeft
        {
            get { return _timeLeft; }
            set
            {
                if (_timeLeft != value)
                {
                    _timeLeft = value;
                    OnPropertyChanged(nameof(TimeLeft));
                }
            }
        }

        public bool TimerVisible
        {
            get { return _timerVisible; }
            set
            {
                if (_timerVisible != value)
                {
                    _timerVisible = value;
                    OnPropertyChanged(nameof(TimerVisible));
                }
            }
        }

        #endregion

        #region Events

        private event EventHandler TimerFinished;

        #endregion

        #region Commands

        #region Winner Command

        private RelayCommand _winnerCommand;

        public RelayCommand WinnerCommand
        {
            get { return _winnerCommand ?? (_winnerCommand = new RelayCommand(winnerCommand)); }
        }

        /// <summary>
        /// Shows all of the remaining squares and play a winning sound while doing it.
        /// </summary>
        /// <param name="obj">The object.</param>
        private void winnerCommand(object obj)
        {
            _timer.Stop();
            TimerVisible = false;
            TimeLeft = 0;

            // TODO: play winning sound
            foreach (Square square in _squares)
            {
                square.PrizeVisible = false;
                square.NumberVisible = false;
            }
        }

        #endregion

        #region Reset Command

        private RelayCommand _resetCommand;

        public RelayCommand ResetCommand
        {
            get { return _resetCommand ?? (_resetCommand = new RelayCommand(resetCommand)); }
        }

        /// <summary>
        /// Closes all of the squares, changes the round so the image changes, gets the new prizes,
        /// randomizes their locations and assigns them to the squares.
        /// </summary>
        /// <param name="obj">The object.</param>
        private void resetCommand(object obj)
        {
            // change the stage
            Stage++;
            resetForCurrentStage();   
        }

        #endregion

        #region Toggle Number Visible Command

        private RelayCommand<Square> _numberClickedCommand;

        public RelayCommand<Square> NumberClickedCommand
        {
            get { return _numberClickedCommand ?? (_numberClickedCommand = new RelayCommand<Square>(numberClickedCommand)); }
        }

        /// <summary>
        /// Toggles the number visible command.
        /// </summary>
        /// <param name="obj">The object.</param>
        private void numberClickedCommand(Square square)
        {
            processSquareClick(square);
        }

        #endregion

        #region Exit Command

        private RelayCommand _exitCommand;

        public RelayCommand ExitCommand
        {
            get { return _exitCommand ?? (_exitCommand = new RelayCommand(exitCommand)); }
        }

        private void exitCommand(object obj)
        {
            var result = MessageBox.Show("Are you sure you want to exit Concentration?", "Exit?", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                Application.Current.Shutdown();
            }
        }

        #endregion

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindowViewModel"/> class.
        /// </summary>
        public MainWindowViewModel()
        {
            initializeViewModel();
        }

        #region Initialization

        /// <summary>
        /// Initializes the view model.
        /// </summary>
        private void initializeViewModel()
        {
            Stage = FIRST_STAGE;

            _timer = new DispatcherTimer(new TimeSpan(0, 0, 1), DispatcherPriority.Background, Timer_Tick, Application.Current.Dispatcher);
            
            _timerVisible = false;
            _timeLeft = 0;
            _seed = new Random();

            TimerFinished += Timer_TimerFinished;

            createPrizeList();
            setupAudio();
            createNumberSquares();
            createPuzzleImageLocations();
            resetForCurrentStage();
        }

        /// <summary>
        /// Creates the prize list.
        /// </summary>
        private void createPrizeList()
        {
            _prizeList = new List<string>()
            {
                @"/Concentration;component/Resources/Prize 1.png",
                @"/Concentration;component/Resources/Prize 2.png",
                @"/Concentration;component/Resources/Prize 3.png",
                @"/Concentration;component/Resources/Prize 4.png",
                @"/Concentration;component/Resources/Prize 5.png",
                @"/Concentration;component/Resources/Prize 6.png",
                @"/Concentration;component/Resources/Prize 7.png",
                @"/Concentration;component/Resources/Prize 8.png",
                @"/Concentration;component/Resources/Prize 9.png",
                @"/Concentration;component/Resources/Prize 10.png",
                @"/Concentration;component/Resources/Prize 11.png",
                @"/Concentration;component/Resources/Prize 12.png"
            };
        }

        /// <summary>
        /// Setups the audio.
        /// </summary>
        private void setupAudio()
        {
            _squareOpenPlayer = new MediaPlayer();
            _squareOpenPlayer.Open(new Uri("../../Resources/SquareOpen.mp3", UriKind.Relative));
            _wildPlayer = new MediaPlayer();
            _wildPlayer.Open(new Uri("../../Resources/Wild.mp3", UriKind.Relative));
            _matchPlayer = new MediaPlayer();
            _matchPlayer.Open(new Uri("../../Resources/Match.mp3", UriKind.Relative));
            _guessTimeExpiredPlayer = new MediaPlayer();
            _guessTimeExpiredPlayer.Open(new Uri("../../Resources/GuessTimeExpired.mp3", UriKind.Relative));
        }

        /// <summary>
        /// Creates the number squares.
        /// </summary>
        private void createNumberSquares()
        {
            _squares = new ObservableCollection<Square>();

            for (int index = 0; index < SQUARE_COUNT; index++)
            {
                _squares.Add(new Square()
                {
                    Number = index + 1
                });
            }

            _squares[10].IsWild = true;
        }

        /// <summary>
        /// Creates the puzzle image locations.
        /// </summary>
        private void createPuzzleImageLocations()
        {
            _puzzleImageLocations = new List<string>();

            _puzzleImageLocations.Add(@"/Concentration;component/Resources/Round 1.png");
            _puzzleImageLocations.Add(@"/Concentration;component/Resources/Round 2.png");
            _puzzleImageLocations.Add(@"/Concentration;component/Resources/Round 3.png");
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Handles the Tick event of the Timer control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs" /> instance containing the event data.</param>
        private void Timer_Tick(object sender, EventArgs e)
        {
            if (_timeLeft > 0)
            {
                TimeLeft--;
            }
            else
            {
                TimerVisible = false;
                _timeLeft = 0;
                _timer.Stop();

                if (TimerFinished != null)
                {
                    TimerFinished(this, null);
                }
            }
        }

        /// <summary>
        /// Handles the TimerFinished event of the Timer control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void Timer_TimerFinished(object sender, EventArgs e)
        {
            processTimeUp();
        }

        #endregion

        #region Game Operations

        /// <summary>
        /// Processes the square click.
        /// </summary>
        /// <param name="square">The square.</param>
        private void processSquareClick(Square square)
        { 
            if ((TimeLeft > 0) || !square.NumberVisible)
            {
                return;
            }

            // show the prize if it isn't visible
            if (square.NumberVisible)
            {
                square.NumberVisible = false;

                if (square.IsWild)
                {
                    _wildPlayer.Stop();
                    _wildPlayer.Play();
                }
                else
                {
                    _squareOpenPlayer.Stop();
                    _squareOpenPlayer.Play();
                }
            }

            // examine the board for what to do
            List<Square> visiblePrizeSquares = _squares.Where(s => !s.NumberVisible && s.PrizeVisible).ToList();

            if (visiblePrizeSquares.Count > 1)
            {
                Square visibleWild = visiblePrizeSquares.FirstOrDefault(s => s.IsWild);

                if (visibleWild != null)
                {
                    // Automatically pick the match of the other square.
                    Square otherSquare = visiblePrizeSquares.FirstOrDefault(s => !s.IsWild);

                    foreach (Square item in _squares)
                    {
                        if (string.Compare(item.Prize, otherSquare.Prize, StringComparison.InvariantCulture) == 0)
                        {
                            item.NumberVisible = false;
                        }
                    }

                    // start the timer stuff to allow the user to see the prize and be happy and then try to guess the puzzle.
                    _matchFound = true;
                    _timer.Stop();
                    TimeLeft = 5;
                    TimerVisible = true;
                    _timer.Start();
                }
                else
                {
                    // are these two a match?
                    if (string.Compare(visiblePrizeSquares[0].Prize, visiblePrizeSquares[1].Prize, StringComparison.InvariantCulture) == 0)
                    {
                        // play the match sound
                        _matchPlayer.Stop();
                        _matchPlayer.Play();

                        // start the timer stuff to allow the user to see the prize and be happy and then try to guess the puzzle.
                        _timer.Stop();
                        TimeLeft = 5;
                        TimerVisible = true;
                        _matchFound = true;
                        _timer.Start();
                    }
                    else
                    {
                        // wait for a few seconds and then toggle the squares back.
                        _noMatchFound = true;
                        _timer.Stop();
                        TimeLeft = 5;
                        TimerVisible = true;
                        _timer.Start();
                    }
                }
            }
        }

        /// <summary>
        /// Processes the time up.
        /// </summary>
        private void processTimeUp()
        {
            // if the time is up and a match was found...
            if (_matchFound)
            {
                _matchFound = false;

                // show the puzzle behind the prize pieces
                List<Square> visiblePrizeSquares = _squares.Where(s => !s.NumberVisible && s.PrizeVisible).ToList();

                foreach (var square in visiblePrizeSquares)
                {
                    square.PrizeVisible = false;
                }

                _guessTimerRunning = true;

                // start the guess timer
                _timer.Stop();
                TimeLeft = 10;
                TimerVisible = true;
                _timer.Start();
            }
            else if (_noMatchFound)
            {
                List<Square> visiblePrizeSquares = _squares.Where(s => !s.NumberVisible && s.PrizeVisible).ToList();

                foreach (var square in visiblePrizeSquares)
                {
                    square.NumberVisible = true;
                }
            }
            else if (_guessTimerRunning)
            {
                _guessTimerRunning = false;
                // play the bad guess sound
                _guessTimeExpiredPlayer.Stop();
                _guessTimeExpiredPlayer.Play();
            }
        }

        /// <summary>
        /// Resets for current stage.
        /// </summary>
        private void resetForCurrentStage()
        {
            TimerVisible = false;
            TimeLeft = 0;
            _matchFound = false;
            _guessTimerRunning = false;
            closeAllSquares();
            assignSquarePrizes();
            loadPuzzleImageForStage(Stage);
        }

        /// <summary>
        /// Loads the puzzle image for stage.
        /// </summary>
        /// <param name="stage">The stage.</param>
        /// <exception cref="ArgumentOutOfRangeException">The stage must be between 0 and 2</exception>
        private void loadPuzzleImageForStage(int stage)
        {
            if (stage < 0 || stage >= STAGE_COUNT)
            {
                throw new ArgumentOutOfRangeException(nameof(stage), "The stage must be between 0 and 2");
            }

            PuzzleImageSource = _puzzleImageLocations[stage];
        }

        /// <summary>
        /// Closes all squares.
        /// </summary>
        private void closeAllSquares()
        {
            foreach (Square square in Squares)
            {
                square.NumberVisible = true;
            }

            // Do these separately so they players don't think they're seeing the prize locations for the next round.
            foreach (Square square in Squares)
            {
                square.PrizeVisible = true;
            }
        }

        /// <summary>
        /// Assigns the square prizes and the wild.
        /// </summary>
        private void assignSquarePrizes()
        {
            List<int> remainingSquareIndices = new List<int>();
            List<string> remainingPrizes = new List<string>();

            // populate the remaining square indices
            for (int index = 0; index < _squares.Count; index++)
            {
                remainingSquareIndices.Add(index);
            }

            // populate and shuffle the prize list
            remainingPrizes.AddRange(_prizeList);
            remainingPrizes.Shuffle();

            // assign the wild
            int wildIndex = getRandomValue(0, _squares.Count - 1);

            // remove the wild square so it doesn't get assigned.
            remainingSquareIndices.RemoveAt(wildIndex);

            for (int index = 0; index < _squares.Count; index++)
            {
                _squares[index].IsWild = false;
            }

            _squares[wildIndex].IsWild = true;

            while (remainingPrizes.Count > 0)
            {
                var prize = remainingPrizes[0];
                remainingPrizes.RemoveAt(0);

                // get 2 square indices out of the remaining squares
                int random = getRandomValue(0, remainingSquareIndices.Count - 1);
                int square1 = remainingSquareIndices[random];
                remainingSquareIndices.RemoveAt(random);

                int random2 = getRandomValue(0, remainingSquareIndices.Count - 1);
                int square2 = remainingSquareIndices[random2];
                remainingSquareIndices.RemoveAt(random2);

                _squares[square1].Prize = prize;
                _squares[square2].Prize = prize;
            }
        }

        /// <summary>
        /// Gets the random value.
        /// </summary>
        /// <param name="min">The minimum.</param>
        /// <param name="max">The maximum.</param>
        /// <returns></returns>
        private int getRandomValue(int min, int max)
        {
            return _seed.Next(min, max); //for ints
        }

        #endregion

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _squareOpenPlayer.Close();
                    _wildPlayer.Close();
                    _matchPlayer.Close();
                    _guessTimeExpiredPlayer.Close();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~MainWindowViewModel() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
