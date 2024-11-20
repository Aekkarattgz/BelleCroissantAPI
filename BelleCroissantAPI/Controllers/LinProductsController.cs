using BelleCroissantAPI.Model;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;

namespace BelleCroissantAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LinProductsController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public LinProductsController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // ฟังก์ชันช่วย: ดึงข้อมูลผลิตภัณฑ์ทั้งหมดจากฐานข้อมูล
        private List<Product> FetchAllProducts()
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                string query = "SELECT ProductId, Name, Price, Description FROM Products";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        var products = new List<Product>();
                        while (reader.Read())
                        {
                            products.Add(new Product
                            {
                                ProductId = reader.GetInt32(reader.GetOrdinal("ProductId")),
                                Name = reader.GetString(reader.GetOrdinal("Name")),
                                Price = reader.GetDecimal(reader.GetOrdinal("Price")),
                                Description = reader.GetString(reader.GetOrdinal("Description"))
                            });
                        }
                        return products;
                    }
                }
            }
        }

        // GET: api/Products
        // ดึงข้อมูลผลิตภัณฑ์ทั้งหมดและจัดเรียงด้วย LINQ
        [HttpGet]
        public IActionResult GetAllProducts()
        {
            try
            {
                var products = FetchAllProducts();

                // ใช้ LINQ เพื่อจัดเรียงผลิตภัณฑ์ตามชื่อ
                var sortedProducts = products.OrderBy(p => p.Name).ToList();

                return Ok(sortedProducts); // คืนค่าข้อมูลที่จัดเรียงแล้ว
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // GET: api/Products/{id}
        // ค้นหาผลิตภัณฑ์ตาม ID ด้วย LINQ
        [HttpGet("{id}")]
        public IActionResult GetProductById(int id)
        {
            try
            {
                // ใช้ LINQ เพื่อค้นหาผลิตภัณฑ์ตาม ID
                var product = FetchAllProducts().FirstOrDefault(p => p.ProductId == id);

                if (product == null)
                    return NotFound();

                return Ok(product); // คืนค่าข้อมูลที่พบ
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // POST: api/Products
        // เพิ่มผลิตภัณฑ์ใหม่ในฐานข้อมูล
        [HttpPost]
        public IActionResult AddProduct([FromBody] Product newProduct)
        {
            try
            {
                string connectionString = _configuration.GetConnectionString("DefaultConnection");
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();
                    string query = "INSERT INTO Products (Name, Price, Description) OUTPUT INSERTED.ProductId VALUES (@name, @price, @description)";
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@name", newProduct.Name);
                        cmd.Parameters.AddWithValue("@price", newProduct.Price);
                        cmd.Parameters.AddWithValue("@description", newProduct.Description);

                        // รับ ID ที่สร้างใหม่
                        newProduct.ProductId = (int)cmd.ExecuteScalar();
                    }
                }

                return CreatedAtAction(nameof(GetProductById), new { id = newProduct.ProductId }, newProduct);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // PUT: api/Products/{id}
        // อัปเดตผลิตภัณฑ์ในฐานข้อมูล
        [HttpPut("{id}")]
        public IActionResult UpdateProduct(int id, [FromBody] Product updatedProduct)
        {
            try
            {
                string connectionString = _configuration.GetConnectionString("DefaultConnection");
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();
                    string query = "UPDATE Products SET Name = @name, Price = @price, Description = @description WHERE ProductId = @id";
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@name", updatedProduct.Name);
                        cmd.Parameters.AddWithValue("@price", updatedProduct.Price);
                        cmd.Parameters.AddWithValue("@description", updatedProduct.Description);
                        cmd.Parameters.AddWithValue("@id", id);

                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected == 0)
                            return NotFound();
                    }
                }

                // ใช้ LINQ เพื่อคืนค่าผลิตภัณฑ์ที่อัปเดต
                var product = FetchAllProducts().FirstOrDefault(p => p.ProductId == id);
                return Ok(product);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // DELETE: api/Products/{id}
        // ลบผลิตภัณฑ์ในฐานข้อมูล
        [HttpDelete("{id}")]
        public IActionResult DeleteProduct(int id)
        {
            try
            {
                string connectionString = _configuration.GetConnectionString("DefaultConnection");
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();
                    string query = "DELETE FROM Products WHERE ProductId = @id";
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@id", id);
                        int rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected == 0)
                            return NotFound();
                    }
                }

                return NoContent(); // การลบสำเร็จ
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
