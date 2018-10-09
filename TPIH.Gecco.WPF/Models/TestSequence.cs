using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using TPIH.Gecco.WPF.Annotations;
using TPIH.Gecco.WPF.Drivers;

namespace TPIH.Gecco.WPF.Models
{
    public class TestSequence : INotifyPropertyChanged
    {
        private uint _sequence;
        private uint _duration = 200;
        private double _delay = 20;
        private uint _power = 50;
        private SideEnum _side;
        //private uint _repeatSequence;

        public uint Sequence
        {
            get { return _sequence; }
            set
            {
                _sequence = value;
                OnPropertyChanged("Sequence");
            }
        }

        public const uint MaxDuration = 2 * 60 * 1000;
        public uint Duration
        {
            get { return _duration; }
            set
            {
                _duration = value < 20 ? 20 : value;
                _duration = _duration > MaxDuration ? MaxDuration : _duration;
                OnPropertyChanged("Duration");
            }
        }

        public double Delay
        {
            get { return _delay; }
            set
            {
                _delay = value < 20 ? 20 : value;
                OnPropertyChanged("Delay");
            }
        }

        public uint Power
        {
            get { return _power; }
            set
            {
                _power = value > 100 ? 100 : value;
                OnPropertyChanged("Power");
            }
        }

        public SideEnum Side
        {
            get { return _side; }
            set
            {
                _side = value;
                OnPropertyChanged("Side");
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        public TestSequence Copy()
        {
            return new TestSequence
            {
                Delay = _delay,
                Duration = _duration,
                Power = _power,
                Side = _side,
                Sequence = _sequence,
            };
        }
    }
}
