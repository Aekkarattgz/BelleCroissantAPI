using Microsoft.AspNetCore.Mvc;
using BelleCroissantAPI.Model;
using System.Data.SqlClient;

namespace BelleCroissantAPI.Controllers
{
    // การตั้งค่า Route สำหรับ controller
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        // ตัวแปร configuration สำหรับการเข้าถึงการตั้งค่าภายในไฟล์ appsettings.json (เช่น connection string)
        public OrdersController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // GET: api/orders
        // ดึงข้อมูลคำสั่งซื้อทั้งหมดจากฐานข้อมูล
        [HttpGet]
        public IActionResult GetAllOrders()
        {
            try
            {
                // สร้างรายการของคำสั่งซื้อ
                List<Order> orders = new List<Order>();
                // ดึง connection string จากการตั้งค่า
                string connectionString = _configuration.GetConnectionString("DefaultConnection");

                // สร้างการเชื่อมต่อกับฐานข้อมูล
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open(); // เปิดการเชื่อมต่อ
                    // สร้างคำสั่ง SQL สำหรับดึงข้อมูลคำสั่งซื้อทั้งหมด
                    string query = "SELECT OrderId, CustomerId, OrderDate, IsCompleted, Status FROM Orders";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        // ดำเนินการคำสั่ง SQL และอ่านผลลัพธ์
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read()) // อ่านข้อมูลแต่ละแถวจากผลลัพธ์
                            {
                                var order = new Order
                                {
                                    OrderId = reader.GetInt32(reader.GetOrdinal("OrderId")), // ดึง OrderId
                                    CustomerId = reader.GetInt32(reader.GetOrdinal("CustomerId")), // ดึง CustomerId
                                    OrderDate = reader.GetDateTime(reader.GetOrdinal("OrderDate")), // ดึง DateTime ของการสั่งซื้อ
                                    IsCompleted = reader.GetBoolean(reader.GetOrdinal("IsCompleted")), // ดึงสถานะการเสร็จสมบูรณ์
                                    Status = reader.GetString(reader.GetOrdinal("Status")) // ดึงสถานะคำสั่งซื้อ
                                };
                                orders.Add(order); // เพิ่มคำสั่งซื้อในรายการ
                            }
                        }
                    }
                }

                return Ok(orders); // ส่งคืนรายการคำสั่งซื้อทั้งหมดในรูปแบบ JSON
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}"); // หากเกิดข้อผิดพลาด
            }
        }

        // GET: api/orders/{id}
        // ดึงข้อมูลคำสั่งซื้อตาม ID
        [HttpGet("{id}")]
        public IActionResult GetOrderById(int id)
        {
            try
            {
                Order order = null;
                string connectionString = _configuration.GetConnectionString("DefaultConnection");

                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();
                    // สร้างคำสั่ง SQL สำหรับดึงข้อมูลคำสั่งซื้อที่ตรงกับ OrderId
                    string query = "SELECT OrderId, CustomerId, OrderDate, IsCompleted, Status FROM Orders WHERE OrderId = @id";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@id", id); // เพิ่มพารามิเตอร์สำหรับ OrderId
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read()) // หากพบคำสั่งซื้อที่ตรงกับ ID
                            {
                                order = new Order
                                {
                                    OrderId = reader.GetInt32(reader.GetOrdinal("OrderId")),
                                    CustomerId = reader.GetInt32(reader.GetOrdinal("CustomerId")),
                                    OrderDate = reader.GetDateTime(reader.GetOrdinal("OrderDate")),
                                    IsCompleted = reader.GetBoolean(reader.GetOrdinal("IsCompleted")),
                                    Status = reader.GetString(reader.GetOrdinal("Status"))
                                };
                            }
                        }
                    }
                }

                if (order == null)
                {
                    return NotFound(); // หากไม่พบคำสั่งซื้อ
                }

                return Ok(order); // ส่งคืนคำสั่งซื้อที่พบ
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}"); // หากเกิดข้อผิดพลาด
            }
        }

        // POST: api/orders
        // เพิ่มคำสั่งซื้อใหม่
        [HttpPost]
        public IActionResult AddOrder([FromBody] Order newOrder)
        {
            try
            {
                string connectionString = _configuration.GetConnectionString("DefaultConnection");

                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();
                    // สร้างคำสั่ง SQL สำหรับการเพิ่มคำสั่งซื้อใหม่
                    string query = "INSERT INTO Orders (CustomerId, OrderDate, IsCompleted, Status) VALUES (@customerId, @orderDate, @isCompleted, @status)";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        // เพิ่มพารามิเตอร์ที่รับค่าจากคำสั่งซื้อใหม่
                        cmd.Parameters.AddWithValue("@customerId", newOrder.CustomerId);
                        cmd.Parameters.AddWithValue("@orderDate", newOrder.OrderDate);
                        cmd.Parameters.AddWithValue("@isCompleted", newOrder.IsCompleted);
                        cmd.Parameters.AddWithValue("@status", newOrder.Status);

                        int rowsAffected = cmd.ExecuteNonQuery(); // รันคำสั่ง SQL
                        if (rowsAffected > 0)
                        {
                            return CreatedAtAction(nameof(GetOrderById), new { id = newOrder.OrderId }, newOrder); // คืนค่าคำสั่งซื้อใหม่ที่ถูกสร้าง
                        }
                        else
                        {
                            return StatusCode(500, "Error inserting new order."); // หากไม่สามารถเพิ่มคำสั่งซื้อได้
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}"); // หากเกิดข้อผิดพลาด
            }
        }

        // PUT: api/orders/{id}/complete
        // ทำเครื่องหมายว่าเสร็จสมบูรณ์
        [HttpPut("{id}/complete")]
        public IActionResult CompleteOrder(int id)
        {
            try
            {
                string connectionString = _configuration.GetConnectionString("DefaultConnection");

                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();
                    // สร้างคำสั่ง SQL เพื่ออัปเดตคำสั่งซื้อว่าเสร็จสมบูรณ์
                    string query = "UPDATE Orders SET IsCompleted = 1, Status = 'Completed' WHERE OrderId = @id";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@id", id); // เพิ่มพารามิเตอร์สำหรับ OrderId

                        int rowsAffected = cmd.ExecuteNonQuery(); // รันคำสั่ง SQL
                        if (rowsAffected > 0)
                        {
                            return NoContent(); // อัปเดตคำสั่งซื้อสำเร็จ
                        }
                        else
                        {
                            return NotFound(); // หากไม่พบคำสั่งซื้อ
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}"); // หากเกิดข้อผิดพลาด
            }
        }

        // PUT: api/orders/{id}/cancel
        // ยกเลิกคำสั่งซื้อ
        [HttpPut("{id}/cancel")]
        public IActionResult CancelOrder(int id)
        {
            try
            {
                string connectionString = _configuration.GetConnectionString("DefaultConnection");

                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();
                    // สร้างคำสั่ง SQL เพื่ออัปเดตคำสั่งซื้อว่าเป็นสถานะ "Cancelled"
                    string query = "UPDATE Orders SET IsCompleted = 0, Status = 'Cancelled' WHERE OrderId = @id";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@id", id); // เพิ่มพารามิเตอร์สำหรับ OrderId

                        int rowsAffected = cmd.ExecuteNonQuery(); // รันคำสั่ง SQL
                        if (rowsAffected > 0)
                        {
                            return NoContent(); // การยกเลิกคำสั่งซื้อสำเร็จ
                        }
                        else
                        {
                            return NotFound(); // หากไม่พบคำสั่งซื้อ
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
