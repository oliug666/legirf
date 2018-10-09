using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using TPIH.Gecco.WPF.Drivers;
using TPIH.Gecco.WPF.Models;

namespace TPIH.Gecco.WPF.ViewModels
{
    public class TestSequenceViewModel : ViewModelBase
    {
        private TestSequence _selectedSequence;
        public TestSequence SelectedSequence
        {
            get { return _selectedSequence; }
            set
            {
                _selectedSequence = value;
                OnPropertyChanged(() => SelectedSequence);
            }
        }

        private ObservableCollection<TestSequence> _sequences;
        public ObservableCollection<TestSequence> Sequences
        {
            get { return _sequences; }
            set
            {
                _sequences = value;
                OnPropertyChanged(() => _sequences);
            }
        }


        private bool _isRunningSequence;
        public bool IsRunningSequence
        {
            get { return _isRunningSequence; }
            set
            {
                _isRunningSequence = value;
                OnPropertyChanged("IsRunningSequence");
            }
        }

        private ObservableCollection<SideEnum> _availableSides;
        public ObservableCollection<SideEnum> AvailableSides
        {
            get { return _availableSides; }
            set
            {
                _availableSides = value;
                OnPropertyChanged("AvailableSides");
            }
        }

        #region Commands
        public ICommand AddNewSequenceCommand { get; private set; }
        public ICommand IncrementSequenceCommand { get; private set; }
        public ICommand DecrementSequenceCommand { get; private set; }

        public ICommand RemoveCommand { get; private set; }
        public ICommand CopyCommand { get; private set; }

        public ICommand ToggleExecutionStateCommand { get; private set; }

        #endregion

        #region Privates

        private readonly AutoResetEvent _pulseDoneEvent = new AutoResetEvent(false);
        private static CancellationTokenSource TokenSource2 = new CancellationTokenSource();
        CancellationToken _cancellationToken = TokenSource2.Token;
        #endregion

        public TestSequenceViewModel()
        {
            AvailableSides = new ObservableCollection<SideEnum>();
            foreach (var value in Enum.GetValues(typeof(SideEnum)).Cast<SideEnum>())
            {
                if(value != SideEnum.Both)
                AvailableSides.Add(value);
            }

            Sequences = new ObservableCollection<TestSequence>
            {
                new TestSequence{Side = SideEnum.Out1, Duration = 2000},
                new TestSequence{Side = SideEnum.Out2, Duration = 2000},
                new TestSequence{Side = SideEnum.Out1, Duration = 2000}
            };
            ReorderSequenceNumbers();

            AddNewSequenceCommand = new DelegateCommand(obj =>
            {
                var seq = new TestSequence();
                Sequences.Add(seq);
                ReorderSequenceNumbers();
                SelectedSequence = seq;
            });
            IncrementSequenceCommand = new DelegateCommand(obj =>
            {
                var index = Sequences.IndexOf(SelectedSequence);
                var seq = Sequences[index];
                Sequences.Remove(SelectedSequence);
                Sequences.Insert(index - 1, seq);
                SelectedSequence = seq;
                ReorderSequenceNumbers();
            }, obj => SelectedSequence != null && Sequences.IndexOf(SelectedSequence) > 0);
            DecrementSequenceCommand = new DelegateCommand(obj =>
            {
                var index = Sequences.IndexOf(SelectedSequence);
                var seq = Sequences[index];
                Sequences.Remove(SelectedSequence);
                Sequences.Insert(index + 1, seq);
                SelectedSequence = seq;
                ReorderSequenceNumbers();
            }, obj => SelectedSequence != null && Sequences.IndexOf(SelectedSequence) < Sequences.Count - 1);

            CopyCommand = new DelegateCommand(Copy, obj => SelectedSequence != null);
            RemoveCommand = new DelegateCommand(Remove, obj => SelectedSequence != null);
            ToggleExecutionStateCommand = new DelegateCommand(ToggleExecutionState, obj => Sequences.All(s => s.Power > 0));

        }

        void Driver_OnCommandReceived(object sender, GeccoDriverArgs e)
        {
            if (e.Info == InfoEnum.PulseDisabled)
            {
                _pulseDoneEvent.Set();
            }
        }

        private void ToggleExecutionState(object obj)
        {
            if (!IsRunningSequence)
            {
                TokenSource2.Cancel();
                return;
            }
            else
            {
                if (DriverContainer.Driver.IsConnected)
                    ExecuteSequences();
                else
                {
                    OnError(new Exception("Not connected to any device."));
                    IsRunningSequence = false;
                }
            }

        }

        private void ExecuteSequences()
        {
            var sequences = Sequences.ToList();

            TokenSource2 = new CancellationTokenSource();
            _cancellationToken = TokenSource2.Token;
            Task.Factory.StartNew(() =>
            {
                _cancellationToken.ThrowIfCancellationRequested();
                foreach (var testSequence in sequences)
                {
                    DriverContainer.Driver.SetPower((int)testSequence.Power);
                    Thread.Sleep(200);
                    DriverContainer.Driver.SetContinousDelay((int)testSequence.Delay);
                    Thread.Sleep(200);
                    DriverContainer.Driver.SetPulseDuration(testSequence.Duration);
                    Thread.Sleep(200);
                    DriverContainer.Driver.SetSide(testSequence.Side);
                    Thread.Sleep(200);
                    DriverContainer.Driver.PulseSingle();
                    Thread.Sleep((int)testSequence.Duration + (int)testSequence.Delay + 1000);
                    _cancellationToken.ThrowIfCancellationRequested();
                }
                IsRunningSequence = false;
            }, _cancellationToken);
        }

        private void Remove(object obj)
        {
            Sequences.Remove(SelectedSequence);
        }

        private void Copy(object obj)
        {
            if (SelectedSequence != null)
            {
                var copy = SelectedSequence.Copy();
                var index = Sequences.IndexOf(SelectedSequence);
                Sequences.Insert(index + 1, copy);
                ReorderSequenceNumbers();
                SelectedSequence = copy;
            }
        }

        private void ReorderSequenceNumbers()
        {
            for (int i = 0; i < Sequences.Count; i++)
            {
                Sequences[i].Sequence = (uint)(i + 1);
            }
        }
    }
}
