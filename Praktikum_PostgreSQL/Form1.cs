using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.Odbc;
using System.Windows.Forms.VisualStyles;

namespace Praktikum_PostgreSQL
{
    public partial class ProductForm : Form
    {
        string connectionString = "DSN=Bakery;Uid=postgres;Pwd=password;";
        List<ProductItem> productItems = new List<ProductItem>();
        List<CategoryItem> categoryItems = new List<CategoryItem>();
        bool isEdit = true;
        public ProductForm()
        {
            InitializeComponent();
        }

        private void onFormLoaded(object sender, EventArgs e)
        {
            OdbcConnection connection = new OdbcConnection(connectionString);
            connection.Open();
            loadProductComboBox(connection);
            loadCategoryComboBox(connection);
            connection.Close();
        }

        private void loadProductComboBox(OdbcConnection connection)
        {
            productItems.Clear();
            OdbcCommand command = new OdbcCommand("SELECT * FROM public.\"Products Table\" ORDER BY \"ID\"", connection);
            OdbcDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                ProductItem item = new ProductItem(int.Parse(reader["ID"].ToString()), int.Parse(reader["Category ID"].ToString()), reader["Product Name"].ToString(), reader["Description"].ToString());
                productItems.Add(item);
            }
            cmbProduct.DataSource = null;
            cmbProduct.DataSource = productItems;
            cmbProduct.DisplayMember = "ProductName";
            cmbProduct.ValueMember = "ItemId";
        }

        private void loadCategoryComboBox(OdbcConnection connection)
        {
            categoryItems.Clear();
            OdbcCommand command = new OdbcCommand("SELECT * FROM public.\"Categories\"", connection);
            OdbcDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                CategoryItem item = new CategoryItem(int.Parse(reader["ID"].ToString()), reader["Product Types"].ToString());
                categoryItems.Add(item);
            }
            cmbCategory.DataSource = categoryItems;
            cmbCategory.DisplayMember = "ItemName";
            cmbCategory.ValueMember = "ItemId";
        }

        private void onSelectedChange(object sender, EventArgs e)
        {

            if (cmbProduct.SelectedIndex != -1)
            {
                ProductItem product = productItems[cmbProduct.SelectedIndex];
                cmbCategory.SelectedValue = product.CategoryId;
                txtProductName.Text = product.ProductName;
                txtDescription.Text = product.Description;
            }
        }

        private void onBtnSaveClicked(object sender, EventArgs e)
        {
            string name = txtProductName.Text;
            string description = txtDescription.Text;
            if (name.Trim().Length > 0 && description.Trim().Length > 0 && cmbCategory.SelectedIndex != -1) 
            {
                OdbcConnection connection = new OdbcConnection(connectionString);
                connection.Open();
                name = name.Replace("'", "''");
                description = description.Replace("'", "''");
                string text;

                if (cmbProduct.SelectedIndex != -1)
                {
                    isEdit = true;
                }

                if (isEdit)
                {
                    ProductItem product = productItems[cmbProduct.SelectedIndex];
                    text = "UPDATE public.\"Products Table\" SET \"Category ID\"=" + cmbCategory.SelectedValue + ", \"Product Name\"='" + name + "', \"Description\"='" + description + "' WHERE \"ID\"=" + product.ItemId;
                }
                else
                    text = "INSERT INTO public.\"Products Table\" (\"Category ID\", \"Product Name\", \"Description\") VALUES (" + cmbCategory.SelectedValue +", '"+name+"'"+ ", '"+description+"')";
                OdbcCommand command = new OdbcCommand(text, connection);

                command.ExecuteNonQuery();

                MessageBox.Show("Product successfully saved!");
                loadProductComboBox(connection);
                if (!isEdit)
                {
                    string selectText = "SELECT lastval()";
                    OdbcCommand selectCommand = new OdbcCommand(selectText, connection);
                    int insertedId = Convert.ToInt32(selectCommand.ExecuteScalar());
                    cmbProduct.SelectedValue = insertedId;
                    isEdit = true;
                }
                connection.Close();
            }
            else
            {
                MessageBox.Show("Field can't be empty!");
            }
        }

        private void onBtnNewClicked(object sender, EventArgs e)
        {
            isEdit = false;
            cmbProduct.SelectedItem = null;
            cmbProduct.SelectedIndex = -1;
            cmbCategory.SelectedItem = null;
            cmbCategory.SelectedIndex = -1;
            txtProductName.Text = "";
            txtDescription.Text = "";
        }
    }

    public class CategoryItem
    {
        public CategoryItem(int Id, string Name)
        {
            ItemId = Id;
            ItemName = Name;
        }

        public int ItemId { get; set; }
        public string ItemName { get; set; }
    }

    public class ProductItem
    {
        public ProductItem(int Id, int CategoryId, string ProductName, string Description)
        {
            this.ItemId = Id;
            this.CategoryId = CategoryId;
            this.ProductName = ProductName;
            this.Description = Description;
        }

        public int ItemId { get; set; }
        public int CategoryId { get; set; }
        public string ProductName { get; set; }
        public string Description { get; set; }
    }
}
