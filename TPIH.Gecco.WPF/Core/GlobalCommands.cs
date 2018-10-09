using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TPIH.Gecco.WPF.ViewModels;

namespace TPIH.Gecco.WPF.Core
{
    public static class GlobalCommands
    {
        /// <summary>
        /// Execute with an exception as a parameter to have the exception displayed for the user.
        /// </summary>
        ///
        //Todo implement a register/unregister way instead. Look at PRISM composite command.
        public static DelegateCommand ShowError;
    }
}
