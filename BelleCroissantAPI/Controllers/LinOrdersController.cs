using Microsoft.AspNetCore.Mvc;
using BelleCroissantAPI.Model;
using System.Data.SqlClient;
using System.Linq;

namespace BelleCroissantAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LinOrdersController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public LinOrdersController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // ฟังก์ชันช่วย: ดึงข้อมูลคำสั่งซื้อทั้งหมดจากฐานข้อมูล
        private List<Order> FetchAllOrders()
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                string query = "SELECT OrderId, CustomerId, OrderDate, IsCompleted, Status FROM Orders";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        var orders = new List<Order>();
                        while (reader.Read())
                        {
                            orders.Add(new Order
                            {
                                OrderId = reader.GetInt32(reader.GetOrdinal("OrderId")),
                                CustomerId = reader.GetInt32(reader.GetOrdinal("CustomerId")),
                                OrderDate = reader.GetDateTime(reader.GetOrdinal("OrderDate")),
                                IsCompleted = reader.GetBoolean(reader.GetOrdinal("IsCompleted")),
                                Status = reader.GetString(reader.GetOrdinal("Status"))
                            });
                        }
                        return orders;
                    }
                }
            }
        }

        // GET: api/orders
        [HttpGet]
        public IActionResult GetAllOrders()
        {
            try
            {
                // ดึงข้อมูลคำสั่งซื้อทั้งหมดจากฐานข้อมูล
                var orders = FetchAllOrders();

                // ใช้ LINQ เพื่อจัดเรียงคำสั่งซื้อโดย OrderDate
                var sortedOrders = orders.OrderBy(o => o.OrderDate).ToList();

                return Ok(sortedOrders); // ส่งคำสั่งซื้อที่จัดเรียงแล้วในรูปแบบ JSON
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // GET: api/orders/{id}
        [HttpGet("{id}")]
        public IActionResult GetOrderById(int id)
        {
            try
            {
                // ใช้ LINQ เพื่อค้นหาคำสั่งซื้อที่มี ID ตรงกัน
                var order = FetchAllOrders().FirstOrDefault(o => o.OrderId == id);

                if (order == null)
                    return NotFound();

                return Ok(order); // ส่งคำสั่งซื้อที่พบกลับในรูปแบบ JSON
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // POST: api/orders
        [HttpPost]
        public IActionResult AddOrder([FromBody] Order newOrder)
        {
            try
            {
                string connectionString = _configuration.GetConnectionString("DefaultConnection");
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();
                    string query = "INSERT INTO Orders (CustomerId, OrderDate, IsCompleted, Status) OUTPUT INSERTED.OrderId VALUES (@customerId, @orderDate, @isCompleted, @status)";
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@customerId", newOrder.CustomerId);
                        cmd.Parameters.AddWithValue("@orderDate", newOrder.OrderDate);
                        cmd.Parameters.AddWithValue("@isCompleted", newOrder.IsCompleted);
                        cmd.Parameters.AddWithValue("@status", newOrder.Status);

                        // รับ ID ที่สร้างใหม่
                        newOrder.OrderId = (int)cmd.ExecuteScalar();
                    }
                }

                return CreatedAtAction(nameof(GetOrderById), new { id = newOrder.OrderId }, newOrder);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // PUT: api/orders/{id}/complete
        [HttpPut("{id}/complete")]
        public IActionResult CompleteOrder(int id)
        {
            try
            {
                string connectionString = _configuration.GetConnectionString("DefaultConnection");
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();
                    string query = "UPDATE Orders SET IsCompleted = 1, Status = 'Completed' WHERE OrderId = @id";
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@id", id);
                        int rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected == 0)
                            return NotFound(); // ไม่พบคำสั่งซื้อ
                    }
                }

                // ใช้ LINQ เพื่อดึงคำสั่งซื้อที่อัปเดต
                var updatedOrder = FetchAllOrders().FirstOrDefault(o => o.OrderId == id);
                return Ok(updatedOrder);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // PUT: api/orders/{id}/cancel
        [HttpPut("{id}/cancel")]
        public IActionResult CancelOrder(int id)
        {
            try
            {
                string connectionString = _configuration.GetConnectionString("DefaultConnection");
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();
                    string query = "UPDATE Orders SET IsCompleted = 0, Status = 'Cancelled' WHERE OrderId = @id";
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@id", id);
                        int rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected == 0)
                            return NotFound(); // ไม่พบคำสั่งซื้อ
                    }
                }

                // ใช้ LINQ เพื่อดึงคำสั่งซื้อที่อัปเดต
                var cancelledOrder = FetchAllOrders().FirstOrDefault(o => o.OrderId == id);
                return Ok(cancelledOrder);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
