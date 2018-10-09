using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FakeItEasy;
using NUnit.Framework;
using TPIH.Gecco.WPF.Core;
using TPIH.Gecco.WPF.Drivers;
using TPIH.Gecco.WPF.Interfaces;
using TPIH.Gecco.WPF.Models;
using TPIH.Gecco.WPF.ViewModels;

namespace TPIH.Gecco.Test.ViewModels
{
    [TestFixture]
    public class TestSequenceViewModelTests
    {
        private TestSequenceViewModel _sut;
        private IGeccoDriver _fakeDriver;
        [SetUp]
        public void Init()
        {
            _fakeDriver = A.Fake<IGeccoDriver>();
            DriverContainer.Driver = _fakeDriver;
            _sut = new TestSequenceViewModel();
        }

        [Test]
        public void CallingExecute_WithSomeSequences_ShouldWaitForEachPulseToFinish()
        {
            //A.CallTo(() => _fakeDriver.IsConnected).Returns(true);
            //GlobalCommands.ShowError = new DelegateCommand(err =>
            //{
            //    if (err is Exception)
            //    {
            //        var exp = err as Exception;
            //    }
            //});
            //_sut.Sequences.Add(new TestSequence { Delay = 100, Duration = 1000, Power = 50, Sequence = 1 });
            //_sut.Sequences.Add(new TestSequence { Delay = 100, Duration = 2000, Power = 50, Sequence = 2 });
            
            //_sut.ToggleExecutionStateCommand.Execute(null);
            //Task.Factory.StartNew(() =>
            //{
            //    Thread.Sleep(100);
            //    _fakeDriver.OnCommandReceived +=
            //        Raise.With(this, new GeccoDriverArgs {Info = InfoEnum.PulseDisabled}).Now;
            //});
            //_sut.PulseDoneEvent.WaitOne();
            //Task.Factory.StartNew(() =>
            //{
            //    Thread.Sleep(200);
            //    _fakeDriver.OnCommandReceived +=
            //        Raise.With(this, new GeccoDriverArgs { Info = InfoEnum.PulseDisabled }).Now;
            //});
            //_sut.PulseDoneEvent.WaitOne();

        }
    }
}
