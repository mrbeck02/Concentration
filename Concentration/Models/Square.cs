using Concentration.Utilities;

using System;

namespace Concentration.Models
{
    public class Square : PropertyChangedNotifier
    {
        private int _number;
        private bool _numberVisible;
        private string _prize;
        private bool _prizeVisible;
        private bool _isWild;

        #region Properties

        public int Number
        {
            get { return _number; }
            set
            {
                if (_number != value)
                {
                    _number = value;
                    OnPropertyChanged(nameof(Number));
                }
            }
        }

        public bool NumberVisible
        {
            get { return _numberVisible; }
            set
            {
                if (_numberVisible != value)
                {
                    _numberVisible = value;
                    OnPropertyChanged(nameof(NumberVisible));
                }
            }
        }

        public string Prize
        {
            get { return _prize; }
            set
            {
                if (string.Compare(_prize, value, StringComparison.InvariantCulture) != 0)
                {
                    _prize = value;
                    OnPropertyChanged(nameof(Prize));
                }
            }
        }

        public bool PrizeVisible
        {
            get { return _prizeVisible; }
            set
            {
                if (_prizeVisible != value)
                {
                    _prizeVisible = value;
                    OnPropertyChanged(nameof(PrizeVisible));
                }
            }
        }

        public bool IsWild
        {
            get { return _isWild; }
            set
            {
                if (_isWild != value)
                {
                    _isWild = value;
                    OnPropertyChanged(nameof(IsWild));
                }
            }
        }

        #endregion

        #region Commands

        #region Toggle Prize Visible Command

        private RelayCommand _togglePrizeVisibleCommand;

        public RelayCommand TogglePrizeVisibleCommand
        {
            get { return _togglePrizeVisibleCommand ?? (_togglePrizeVisibleCommand = new RelayCommand(togglePrizeVisibleCommand)); }
        }

        /// <summary>
        /// Toggles the number visible command.
        /// </summary>
        /// <param name="obj">The object.</param>
        private void togglePrizeVisibleCommand(object obj)
        {
            PrizeVisible = !PrizeVisible;
        }

        #endregion

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="Square"/> class.
        /// </summary>
        public Square()
        {
            initializeViewModel();
        }

        #region Initialization

        /// <summary>
        /// Initializes the view model.
        /// </summary>
        private void initializeViewModel()
        {
            _prizeVisible = true;
            _prize = "<Default Prize>";
            _numberVisible = true;
            _number = 1;
        }

        #endregion
    }
}
