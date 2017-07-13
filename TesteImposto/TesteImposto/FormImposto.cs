using Imposto.Core.Service;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Imposto.Core.Domain;
using System.Text.RegularExpressions;

namespace TesteImposto
{
    public partial class FormImposto : Form
    {
        #region .:Constructor:.
        public FormImposto()
        {
            InitializeComponent();
            AutoValidate = AutoValidate.EnableAllowFocusChange;
            LoadDataGridView();
            LoadComboBoxes();
        }
        #endregion

        #region .:Properties:.
        private bool IsFormValid
        {
            get
            {
                Regex regexNomeCliente = new Regex(@"^[\p{L}\s'.-]+$");

                if (!regexNomeCliente.IsMatch(textBoxNomeCliente.Text)
                    || comboBoxEstadoDestino.SelectedItem == null
                    || comboBoxEstadoOrigem.SelectedItem == null
                    || dataGridViewPedidos.RowCount < 1)
                {
                    MessageBox.Show("Preencha todos os campos corretamente.");
                    return false;
                }

                DataGridViewRowCollection rows = dataGridViewPedidos.Rows;
                for (int i = 0; i < rows.Count; i++)
                {
                    if (i > 0 && i == rows.Count - 1) continue;

                    double valorNota;
                    var valor = rows[i].Cells["Valor"].EditedFormattedValue.ToString();

                    if (!Double.TryParse(valor, out valorNota) || valorNota < 0 ||
                        string.IsNullOrWhiteSpace(rows[i].Cells["Codigo do produto"].EditedFormattedValue.ToString()) ||
                        string.IsNullOrWhiteSpace(rows[i].Cells["Nome do produto"].EditedFormattedValue.ToString()))
                    {
                        MessageBox.Show("Preencha corretamente as informações dos itens da nota.");
                        return false;
                    }
                }
                return true;
            }
        }
        #endregion

        #region .:Loaders:.
        private void LoadComboBoxes()
        {
            string[] source = { "AC", "AL", "AP", "AM", "BA", "CE", "DF", "ES", "GO", "MA", "MT",
                "MS", "MG", "PA", "PB", "PR", "PE", "PI", "RJ", "RN", "RS", "RO", "RR", "SC", "SP", "SE", "TO" };

            comboBoxEstadoDestino.Items.AddRange(source);
            comboBoxEstadoOrigem.Items.AddRange(source);
        }

        private void LoadDataGridView()
        {
            dataGridViewPedidos.AutoGenerateColumns = true;
            dataGridViewPedidos.CausesValidation = false;
            dataGridViewPedidos.DataSource = GetTablePedidos();
            dataGridViewPedidos.Columns["Valor"].DefaultCellStyle.Format = "N";
            ResizeColumns();
        }

        private void ResizeColumns()
        {
            double mediaWidth = dataGridViewPedidos.Width / dataGridViewPedidos.Columns.GetColumnCount(DataGridViewElementStates.Visible);

            for (int i = dataGridViewPedidos.Columns.Count - 1; i >= 0; i--)
            {
                var coluna = dataGridViewPedidos.Columns[i];
                coluna.Width = Convert.ToInt32(mediaWidth);
            }
        }

        private object GetTablePedidos()
        {
            DataTable table = new DataTable("pedidos");
            table.Columns.Add(new DataColumn("Nome do produto", typeof(string)));
            table.Columns.Add(new DataColumn("Codigo do produto", typeof(string)));
            table.Columns.Add(new DataColumn("Valor", typeof(double)));
            table.Columns.Add(new DataColumn("Brinde", typeof(bool)));

            return table;
        }
        #endregion

        #region .:Event Handlers:.
        private void buttonGerarNotaFiscal_Click(object sender, EventArgs e)
        {
            try
            {
                if (!IsFormValid) return;

                Pedido pedido = new Pedido();                
                NotaFiscalService service = new NotaFiscalService();

                pedido.EstadoOrigem = comboBoxEstadoOrigem.SelectedText;
                pedido.EstadoDestino = comboBoxEstadoDestino.SelectedText;
                pedido.NomeCliente = textBoxNomeCliente.Text;

                DataGridViewRowCollection rows = dataGridViewPedidos.Rows;
                for (int i = 0; i < rows.Count; i++)
                {
                    if (i > 0 && i == rows.Count - 1) continue;

                    var item = new PedidoItem()
                    {
                        Brinde = (bool)rows[i].Cells["Brinde"].EditedFormattedValue,
                        CodigoProduto = rows[i].Cells["Codigo do produto"].EditedFormattedValue.ToString(),
                        NomeProduto = rows[i].Cells["Nome do produto"].EditedFormattedValue.ToString(),
                        ValorItemPedido = Double.Parse(rows[i].Cells["Valor"].EditedFormattedValue.ToString())
                    };
                    pedido.ItensDoPedido.Add(item);
                }

                service.GerarNotaFiscal(pedido);
                MessageBox.Show("Operação efetuada com sucesso");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ocorreu um erro: " + ex.Message);
            }
            finally
            {
                comboBoxEstadoOrigem.SelectedIndex = -1;
                comboBoxEstadoDestino.SelectedIndex = -1;
                textBoxNomeCliente.Text = "";
                ((DataTable)dataGridViewPedidos.DataSource).Clear();
            }
        }
        #endregion
    }
}
