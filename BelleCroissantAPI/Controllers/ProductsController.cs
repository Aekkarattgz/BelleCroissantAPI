using BelleCroissantAPI.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;

namespace BelleCroissantAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public ProductsController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // GET: api/Products
        // ดึงข้อมูลทั้งหมดของผลิตภัณฑ์
        [HttpGet]
        public IActionResult GetAllProducts()
        {
            try
            {
                List<Product> products = new List<Product>();
                string connectionString = _configuration.GetConnectionString("DefaultConnection");

                // เชื่อมต่อกับฐานข้อมูล
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();
                    // คำสั่ง SQL เพื่อดึงข้อมูลผลิตภัณฑ์
                    string query = "SELECT ProductId, Name, Price, Description FROM Products";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            // อ่านข้อมูลจากฐานข้อมูล
                            while (reader.Read())
                            {
                                var product = new Product
                                {
                                    ProductId = reader.GetInt32(reader.GetOrdinal("ProductId")),
                                    Name = reader.GetString(reader.GetOrdinal("Name")),
                                    Price = reader.GetDecimal(reader.GetOrdinal("Price")),
                                    Description = reader.GetString(reader.GetOrdinal("Description"))
                                };
                                products.Add(product); // เพิ่มผลิตภัณฑ์ในรายการ
                            }
                        }
                    }
                }

                return Ok(products); // คืนค่ารายการผลิตภัณฑ์ทั้งหมด
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}"); // หากเกิดข้อผิดพลาด
            }
        }

        // GET: api/Products/{id}
        // ดึงข้อมูลผลิตภัณฑ์ตาม id
        [HttpGet("{id}")]
        public IActionResult GetProductById(int id)
        {
            try
            {
                Product product = null;
                string connectionString = _configuration.GetConnectionString("DefaultConnection");

                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();
                    // คำสั่ง SQL เพื่อดึงข้อมูลผลิตภัณฑ์ตาม id
                    string query = "SELECT ProductId, Name, Price, Description FROM Products WHERE ProductId = @id";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@id", id); // เพิ่ม parameter สำหรับ id
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            // หากพบข้อมูล
                            if (reader.Read())
                            {
                                product = new Product
                                {
                                    ProductId = reader.GetInt32(reader.GetOrdinal("ProductId")),
                                    Name = reader.GetString(reader.GetOrdinal("Name")),
                                    Price = reader.GetDecimal(reader.GetOrdinal("Price")),
                                    Description = reader.GetString(reader.GetOrdinal("Description"))
                                };
                            }
                        }
                    }
                }

                if (product == null)
                {
                    return NotFound(); // หากไม่พบผลิตภัณฑ์ตาม id
                }

                return Ok(product); // คืนค่าผลิตภัณฑ์ที่พบ
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}"); // หากเกิดข้อผิดพลาด
            }
        }

        // POST: api/Products
        // เพิ่มผลิตภัณฑ์ใหม่
        [HttpPost]
        public IActionResult AddProduct([FromBody] Product newProduct)
        {
            try
            {
                string connectionString = _configuration.GetConnectionString("DefaultConnection");

                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();
                    // คำสั่ง SQL เพื่อเพิ่มผลิตภัณฑ์ใหม่
                    string query = "INSERT INTO Products (Name, Price, Description) VALUES (@name, @price, @description)";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        // เพิ่มค่า parameters สำหรับคำสั่ง SQL
                        cmd.Parameters.AddWithValue("@name", newProduct.Name);
                        cmd.Parameters.AddWithValue("@price", newProduct.Price);
                        cmd.Parameters.AddWithValue("@description", newProduct.Description);

                        int rowsAffected = cmd.ExecuteNonQuery(); // การดำเนินการคำสั่ง SQL
                        if (rowsAffected > 0)
                        {
                            return CreatedAtAction(nameof(GetProductById), new { id = newProduct.ProductId }, newProduct); // สร้างผลิตภัณฑ์ใหม่
                        }
                        else
                        {
                            return StatusCode(500, "Error inserting new product.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}"); // หากเกิดข้อผิดพลาด
            }
        }

        // PUT: api/Products/{id}
        // อัปเดตข้อมูลผลิตภัณฑ์ตาม id
        [HttpPut("{id}")]
        public IActionResult UpdateProduct(int id, [FromBody] Product updatedProduct)
        {
            try
            {
                string connectionString = _configuration.GetConnectionString("DefaultConnection");

                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();
                    // คำสั่ง SQL เพื่ออัปเดตผลิตภัณฑ์ตาม id
                    string query = "UPDATE Products SET Name = @name, Price = @price, Description = @description WHERE ProductId = @id";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        // เพิ่มค่า parameters สำหรับคำสั่ง SQL
                        cmd.Parameters.AddWithValue("@name", updatedProduct.Name);
                        cmd.Parameters.AddWithValue("@price", updatedProduct.Price);
                        cmd.Parameters.AddWithValue("@description", updatedProduct.Description);
                        cmd.Parameters.AddWithValue("@id", id); // เพิ่ม parameter สำหรับ id

                        int rowsAffected = cmd.ExecuteNonQuery(); // การดำเนินการคำสั่ง SQL
                        if (rowsAffected > 0)
                        {
                            return NoContent(); // การอัปเดตสำเร็จ
                        }
                        else
                        {
                            return NotFound(); // หากไม่พบผลิตภัณฑ์ตาม id ที่ระบุ
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}"); // หากเกิดข้อผิดพลาด
            }
        }

        // DELETE: api/Products/{id}
        // ลบผลิตภัณฑ์ตาม id
        [HttpDelete("{id}")]
        public IActionResult DeleteProduct(int id)
        {
            try
            {
                string connectionString = _configuration.GetConnectionString("DefaultConnection");

                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();
                    // คำสั่ง SQL เพื่อลบผลิตภัณฑ์ตาม id
                    string query = "DELETE FROM Products WHERE ProductId = @id";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@id", id); // เพิ่ม parameter สำหรับ id

                        int rowsAffected = cmd.ExecuteNonQuery(); // การดำเนินการคำสั่ง SQL
                        if (rowsAffected > 0)
                        {
                            return NoContent(); // การลบสำเร็จ
                        }
                        else
                        {
                            return NotFound(); // หากไม่พบผลิตภัณฑ์ตาม id ที่ระบุ
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}"); // หากเกิดข้อผิดพลาด
            }
        }
    }
}

