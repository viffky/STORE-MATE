using Newtonsoft.Json;
using System.Data;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;


namespace StoreMate_WinForms {
    public partial class MainForm : Form {

        private TableManager pricesManager;
        private TableManager storesManager;
        private TableManager customersManager;
        private TableManager suppliersManager;
        private TableManager employeesManager;
        private TableManager stockManager;
        private TableManager salesManager;
        private TableManager saleItemsManager;
        private TableManager transferManager;
        private TableManager transferItemsManager;
        private TableManager restockManager;
        private TableManager restockItemsManager;
        private TableManager discountManager;



        public MainForm() {
            InitializeComponent();

            pricesManager = new TableManager("product_prices", dataGridView_prices);
            pricesManager.LoadAsync();
            storesManager = new TableManager("stores", dataGridView_stores);
            storesManager.LoadAsync();
            customersManager = new TableManager("customers", dataGridView_customers);
            customersManager.LoadAsync();
            suppliersManager = new TableManager("suppliers", dataGridView_suppliers);
            suppliersManager.LoadAsync();
            employeesManager = new TableManager("employees", dataGridView_employees);
            employeesManager.LoadAsync();
            stockManager = new TableManager("stock", dataGridView_stock);
            stockManager.LoadAsync();
            salesManager = new TableManager("sales", dataGridView_sales);
            salesManager.LoadAsync();
            saleItemsManager = new TableManager("sale_items", dataGridView_sale_items);
            saleItemsManager.LoadAsync();
            transferManager = new TableManager("transfers", dataGridView_transfer);
            transferManager.LoadAsync();
            transferItemsManager = new TableManager("transfer_items", dataGridView_transfer_items);
            transferItemsManager.LoadAsync();
            restockManager = new TableManager("restocks", dataGridView_restock);
            restockManager.LoadAsync();
            restockItemsManager = new TableManager("restock_items", dataGridView_restock_items);
            restockItemsManager.LoadAsync();
            discountManager = new TableManager("discounts", dataGridView_discount);
            discountManager.LoadAsync();
        }

        private async void MainForm_Load(object sender, EventArgs e) {
            ProductsUpdate();
        }


        public class Product {
            public int id { get; set; }
            public string name { get; set; }
            public string brand { get; set; }
            public string category { get; set; }
            public string image { get; set; }
            public string price { get; set; }
        }


        private async Task<List<Product>> GetProductsAsync() {
            try {
                using (HttpClient client = new HttpClient()) {
                    client.BaseAddress = new Uri("http://localhost:5000");
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    HttpResponseMessage response = await client.GetAsync("/api/products");
                    if (response.IsSuccessStatusCode) {
                        var stream = await response.Content.ReadAsStreamAsync();
                        var products = await System.Text.Json.JsonSerializer.DeserializeAsync<List<Product>>(stream);
                        return products;
                    }
                    else {
                        MessageBox.Show("Ошибка при получении данных: " + response.StatusCode);
                        return new List<Product>();
                    }
                }
            }
            catch (Exception ex) {
                MessageBox.Show(ex.ToString());
                return new List<Product>();
            }
        }


        private async void ProductsUpdate() {
            var products = await GetProductsAsync();

            var table = new DataTable();
            table.Columns.Add("ID", typeof(int));
            table.Columns.Add("Name", typeof(string));
            table.Columns.Add("Brand", typeof(string));
            table.Columns.Add("Category", typeof(string));
            table.Columns.Add("Image", typeof(Image));
            table.Columns.Add("Price", typeof(string));

            foreach (var p in products) {
                Image img = null;
                if (!string.IsNullOrEmpty(p.image)) {
                    byte[] imageBytes = Convert.FromBase64String(p.image);
                    using (var ms = new MemoryStream(imageBytes)) {
                        img = Image.FromStream(ms);
                    }
                }

                table.Rows.Add(p.id, p.name, p.brand, p.category, img, p.price);
            }

            dataGridView_prod.RowTemplate.Height = 120;
            dataGridView_prod.DataSource = table;

            var imageColumn = (DataGridViewImageColumn)dataGridView_prod.Columns["Image"];
            imageColumn.ImageLayout = DataGridViewImageCellLayout.Zoom;
        }

        private async Task AddProductAsync(int id, string name, string brand, string category, string base64Image) {
            var product = new Dictionary<string, object> {
                ["id"] = id,
                ["name"] = name,
                ["brand"] = brand,
                ["category"] = category,
                ["image"] = base64Image
            };

            using (HttpClient client = new HttpClient()) {
                client.BaseAddress = new Uri("http://localhost:5000");
                var json = System.Text.Json.JsonSerializer.Serialize(product);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync("/api/products", content);
                if (response.IsSuccessStatusCode) {
                    MessageBox.Show("Товар добавлен!");
                }
                else {
                    MessageBox.Show("Ошибка при добавлении: " + response.StatusCode);
                }
            }
        }

        private async Task UpdateProductAsync(int id, string name, string brand, string category, string base64Image) {
            var product = new Dictionary<string, object> {
                ["id"] = id,
                ["name"] = name,
                ["brand"] = brand,
                ["category"] = category,
                ["image"] = base64Image
            };

            using (HttpClient client = new HttpClient()) {
                client.BaseAddress = new Uri("http://localhost:5000");
                var json = System.Text.Json.JsonSerializer.Serialize(product);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PutAsync("/api/products", content);
                if (response.IsSuccessStatusCode) {
                    MessageBox.Show("Товар обновлён!");
                }
                else {
                    MessageBox.Show("Ошибка при обновлении: " + response.StatusCode);
                }
            }
        }

        private async Task DeleteProductAsync(int id) {
            using (var client = new HttpClient()) {
                var response = await client.DeleteAsync($"http://localhost:5000/api/products/{id}");
                if (!response.IsSuccessStatusCode) {
                    MessageBox.Show("Ошибка при удалении товара");
                }
            }
        }

        private string ImageToBase64(Image image) {
            using (MemoryStream ms = new MemoryStream()) {
                image.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                byte[] imageBytes = ms.ToArray();
                return Convert.ToBase64String(imageBytes);
            }
        }


        private void button_prod_add_ChooseImage_Click(object sender, EventArgs e) {
            using (OpenFileDialog openFileDialog = new OpenFileDialog()) {
                openFileDialog.Filter = "Image files (*.jpg;*.jpeg;*.png)|*.jpg;*.jpeg;*.png|All files (*.*)|*.*";

                if (openFileDialog.ShowDialog() == DialogResult.OK) {
                    pictureBox_prod_add.Image = Image.FromFile(openFileDialog.FileName);
                }
            }
        }

        private void button_prod_add_DeleteImage_Click(object sender, EventArgs e) {
            pictureBox_prod_add.Image = null;
        }

        private void button_prod_edit_ChooseImage_Click(object sender, EventArgs e) {
            using (OpenFileDialog openFileDialog = new OpenFileDialog()) {
                openFileDialog.Filter = "Image files (*.jpg;*.jpeg;*.png)|*.jpg;*.jpeg;*.png|All files (*.*)|*.*";

                if (openFileDialog.ShowDialog() == DialogResult.OK) {
                    pictureBox_prod_edit.Image = Image.FromFile(openFileDialog.FileName);
                }
            }
        }

        private void button_prod_edit_DeleteImage_Click(object sender, EventArgs e) {
            pictureBox_prod_edit.Image = null;
        }


        private bool ValidateProductInput(
            TextBox textBoxId,
            TextBox textBoxName,
            TextBox textBoxBrand,
            TextBox textBoxCategory,
            PictureBox pictureBox,
            out int id,
            out string name,
            out string brand,
            out string category,
            out string? base64Image) {
            id = 0;
            name = textBoxName.Text.Trim();
            brand = textBoxBrand.Text.Trim();
            category = textBoxCategory.Text.Trim();
            base64Image = pictureBox.Image != null ? ImageToBase64(pictureBox.Image) : null;

            if (!int.TryParse(textBoxId.Text, out id)) {
                MessageBox.Show("Некорректный ID");
                return false;
            }

            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(brand) || string.IsNullOrWhiteSpace(category)) {
                MessageBox.Show("Все поля, кроме изображения, обязательны");
                return false;
            }

            return true;
        }

        private async void button_prod_add_Click(object sender, EventArgs e) {
            if (!ValidateProductInput(
                textBox_prod_add_ID,
                textBox_prod_add_Name,
                textBox_prod_add_Brand,
                textBox_prod_add_Category,
                pictureBox_prod_add,
                out int id, out string name, out string brand, out string category, out string? base64Image))
                return;

            await AddProductAsync(id, name, brand, category, base64Image);
            ProductsUpdate();
        }

        private async void button_prod_edit_Click(object sender, EventArgs e) {
            if (!ValidateProductInput(
                textBox_prod_edit_ID,
                textBox_prod_edit_Name,
                textBox_prod_edit_Brand,
                textBox_prod_edit_Category,
                pictureBox_prod_edit,
                out int id, out string name, out string brand, out string category, out string? base64Image))
                return;

            await UpdateProductAsync(id, name, brand, category, base64Image);
            ProductsUpdate();
        }

        private void button_prod_update_Click(object sender, EventArgs e) {
            ProductsUpdate();
        }


        private void dataGridView_prod_SelectionChanged(object sender, EventArgs e) {
            if (dataGridView_prod.SelectedRows.Count > 0) {
                var selected_row = dataGridView_prod.SelectedRows[0];
                textBox_prod_edit_ID.Text = selected_row.Cells["ID"].Value.ToString();
                textBox_prod_edit_Name.Text = selected_row.Cells["Name"].Value.ToString();
                textBox_prod_edit_Brand.Text = selected_row.Cells["Brand"].Value.ToString();
                textBox_prod_edit_Category.Text = selected_row.Cells["Category"].Value.ToString();
                pictureBox_prod_edit.Image = selected_row.Cells["Image"].Value != DBNull.Value && selected_row.Cells["Image"].Value != null
                    ? (Image)selected_row.Cells["Image"].Value
                    : null;
            }
        }

        private async void button_prod_delete_Click(object sender, EventArgs e) {
            var id = 0;
            if (int.TryParse(textBox_prod_edit_ID.Text, out id)) {
                await DeleteProductAsync(id);
                ProductsUpdate();
            }
        }

        private async void button_prices_update_Click(object sender, EventArgs e) {
            await pricesManager.LoadAsync();
        }

        private async void button_prices_save_Click(object sender, EventArgs e) {
            await pricesManager.SaveAsync();

        }

        private async void button_stores_update_Click(object sender, EventArgs e) {
            await storesManager.LoadAsync();
        }

        private async void button_stores_save_Click(object sender, EventArgs e) {
            await storesManager.SaveAsync();
        }

        private async void button_customers_update_Click(object sender, EventArgs e) {
            await customersManager.LoadAsync();
        }

        private async void button_customers_save_Click(object sender, EventArgs e) {
            await customersManager.SaveAsync();
        }


        private async void button_suppliers_update_Click(object sender, EventArgs e) {
            await suppliersManager.LoadAsync();
        }

        private async void button_suppliers_save_Click(object sender, EventArgs e) {
            await suppliersManager.SaveAsync();
        }

        private async void button_employees_update_Click(object sender, EventArgs e) {
            await employeesManager.LoadAsync();
        }

        private async void button_employees_save_Click(object sender, EventArgs e) {
            await employeesManager.SaveAsync();
        }

        private async void button_stock_update_Click(object sender, EventArgs e) {
            await stockManager.LoadAsync();
        }

        private async void button_stock_save_Click(object sender, EventArgs e) {
            await stockManager.SaveAsync();
        }

        private async void button_sales_update_Click(object sender, EventArgs e) {
            await salesManager.LoadAsync();
        }

        private async void button_sales_save_Click(object sender, EventArgs e) {
            await salesManager.SaveAsync();
        }

        private async void button_sale_items_update_Click(object sender, EventArgs e) {
            await saleItemsManager.LoadAsync();
        }

        private async void button_sale_items_save_Click(object sender, EventArgs e) {
            await saleItemsManager.SaveAsync();
        }

        private async void button_transfer_update_Click(object sender, EventArgs e) {
            await transferManager.LoadAsync();
        }

        private async void button_transfer_save_Click(object sender, EventArgs e) {
            await transferManager.SaveAsync();
        }

        private async void button_transfer_items_update_Click(object sender, EventArgs e) {
            await transferItemsManager.LoadAsync();
        }

        private async void button_transfer_items_save_Click(object sender, EventArgs e) {
            await transferItemsManager.SaveAsync();
        }

        private async void button_restock_update_Click(object sender, EventArgs e) {
            await restockManager.LoadAsync();
        }

        private async void button_restock_save_Click(object sender, EventArgs e) {
            await restockManager.SaveAsync();
        }

        private async void button_restock_items_update_Click(object sender, EventArgs e) {
            await restockItemsManager.LoadAsync();
        }

        private async void button_restock_items_save_Click(object sender, EventArgs e) {
            await restockItemsManager.SaveAsync();
        }

        private async void button_discount_update_Click(object sender, EventArgs e) {
            await discountManager.LoadAsync();
        }

        private async void button_discount_save_Click(object sender, EventArgs e) {
            await discountManager.SaveAsync();
        }
    }


    public class TableManager {
        private readonly string tableName;
        private readonly DataGridView gridView;
        private readonly BindingSource bindingSource = new BindingSource();
        private DataTable tableData;

        public TableManager(string tableName, DataGridView gridView) {
            this.tableName = tableName;
            this.gridView = gridView;
        }

        public async Task LoadAsync() {
            using var client = new HttpClient();
            var json = await client.GetStringAsync($"http://localhost:5000/api/{tableName}");
            tableData = JsonConvert.DeserializeObject<DataTable>(json);

            bindingSource.DataSource = tableData;
            gridView.DataSource = bindingSource;
            tableData.AcceptChanges();
        }

        public async Task SaveAsync() {
            var changes = tableData.GetChanges();
            if (changes == null) return;

            foreach (DataRow row in changes.Rows) {
                using var client = new HttpClient();

                if (row.RowState == DataRowState.Added) {
                    var dict = row.Table.Columns.Cast<DataColumn>()
                        .ToDictionary(col => col.ColumnName, col => row[col]);

                    var content = new StringContent(JsonConvert.SerializeObject(dict), Encoding.UTF8, "application/json");
                    await client.PostAsync($"http://localhost:5000/api/{tableName}", content);
                }
                else if (row.RowState == DataRowState.Modified) {
                    var id = row["id"];
                    var dict = row.Table.Columns.Cast<DataColumn>()
                        .ToDictionary(col => col.ColumnName, col => row[col]);

                    var content = new StringContent(JsonConvert.SerializeObject(dict), Encoding.UTF8, "application/json");
                    await client.PutAsync($"http://localhost:5000/api/{tableName}/{id}", content);
                }
                else if (row.RowState == DataRowState.Deleted) {
                    var id = row["id", DataRowVersion.Original];
                    await client.DeleteAsync($"http://localhost:5000/api/{tableName}/{id}");
                }
            }

            tableData.AcceptChanges();
        }
    }
}
