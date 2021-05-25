using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using W65C02S.Bus.EventArgs;
using W65C02S.Engine;

namespace W65C02S.Simulator
{
    public partial class frmEmulator : BusForm
    {
        public frmEmulator(Bus.Bus bus, Emulator emulator) : base(bus, emulator)
        {
            InitializeComponent();

            bus.Subscribe<AddressBusEventArgs>(OnAddressChanged);
            bus.Subscribe<OnInstructionExecutingEventArg>(OnInstructionExecuting);
            bus.Subscribe<OnInstructionExecutedEventArg>(OnInstructionExecuted);
            bus.Subscribe<ExceptionEventArg>(OnError);

        }


        #region Bus Events
        private void OnError(ExceptionEventArg obj)
        {
            throw new NotImplementedException();
        }

        private void OnInstructionExecuted(OnInstructionExecutedEventArg obj)
        {
            throw new NotImplementedException();
        }

        private void OnInstructionExecuting(OnInstructionExecutingEventArg obj)
        {
            throw new NotImplementedException();
        }

        private void OnAddressChanged(AddressBusEventArgs obj)
        {
            throw new NotImplementedException();
        }
        #endregion

        private void frmEmulator_FormClosing(object sender, FormClosingEventArgs e)
        {
            // unregister devices from bus
            bus.UnSubscribe<AddressBusEventArgs>(OnAddressChanged);
            bus.UnSubscribe<OnInstructionExecutingEventArg>(OnInstructionExecuting);
            bus.UnSubscribe<OnInstructionExecutedEventArg>(OnInstructionExecuted);
            bus.UnSubscribe<ExceptionEventArg>(OnError);

            // discard local references

        }
    }
}
