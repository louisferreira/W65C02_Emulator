using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using W65C02.API.Enums;
using W65C02.API.EventArgs;
using W65C02.API.Interfaces;

namespace W65C02.IDE
{
    public partial class FormMemoryMonitor : BusForm
    {
        private bool loading = false;
        public FormMemoryMonitor(IBus bus) : base(bus)
        {
            InitializeComponent();
            this.Shown += new EventHandler(Form_Shown);
        }

        private void Form_Shown(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            SetupDataGridView();
            LoadData();
        }


        private void SetupDataGridView()
        {
            dataGridView.ColumnCount = 17;
            dataGridView.Columns[0].Width = 60;
            dataGridView.Columns[0].Name = "Address";

            for (int index = 1; index < dataGridView.ColumnCount; index++)
            {
                dataGridView.Columns[index].Width = 25;
                dataGridView.Columns[index].Name = $"{(index - 1):X2}";
            }

        }

        private async void LoadData()
        {
            loading = true;
            dataGridView.CellFormatting -= new DataGridViewCellFormattingEventHandler(dataGridView_CellFormatting);
            List<string[]> datasource = new List<string[]>();
            await Task.Run(() => {
                var size = 1024 * 64;
                string[] gridRow;
                
                for (int index = 0; index < size - 1; index += 16)
                {
                    gridRow = new string[17];
                    gridRow[0] = $"${index:X4}";
                    for (int col = 0; col < 16; col++)
                    {
                        ushort address = (ushort)(index + col);
                        var arg = new AddressBusEventArgs
                        {
                            Address = address,
                            Mode = DataBusMode.Read
                        };
                        bus.Publish(arg);
                        var cellData = arg.Data;
                        gridRow[col + 1] = $"{cellData:X2}";
                    }
                    datasource.Add(gridRow);
                    //this.UIThread(() =>
                    //{
                    //    dataGridView.Rows.Add(gridRow);
                    //});
                }
            });
            //var source = new BindingSource();
            //source.DataSource = datasource;
            dataGridView.DataSource = datasource;
            loading = false;
            dataGridView.CellFormatting += new DataGridViewCellFormattingEventHandler(dataGridView_CellFormatting);
            this.Cursor = Cursors.Default;
        }
        private void dataGridView_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            //if (loading)
            //    return;
            //if (e != null)
            //{
            //    if (e.Value != null)
            //    {
            //        try
            //        {
            //            e.Value = DateTime.Parse(e.Value.ToString())
            //                .ToLongDateString();
            //            e.FormattingApplied = true;
            //        }
            //        catch (FormatException)
            //        {
            //            Console.WriteLine("{0} is not a valid hex value.", e.Value.ToString());
            //        }
            //    }
            //}
        }

    }
}
