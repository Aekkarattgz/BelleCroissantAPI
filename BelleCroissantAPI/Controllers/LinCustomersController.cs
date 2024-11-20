using Microsoft.AspNetCore.Mvc;
using BelleCroissantAPI.Model;
using System.Data.SqlClient;

namespace BelleCroissantAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LinCustomersController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public LinCustomersController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // ฟังก์ชันช่วย: ดึงข้อมูลลูกค้าทั้งหมดจากฐานข้อมูล
        private List<Customer> FetchAllCustomers()
        {
            List<Customer> customers = new List<Customer>();
            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                string query = "SELECT id, name, email FROM Customers"; // Query พื้นฐาน

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            customers.Add(new Customer
                            {
                                id = reader.GetInt32(reader.GetOrdinal("id")),
                                name = reader.GetString(reader.GetOrdinal("name")),
                                email = reader.GetString(reader.GetOrdinal("email"))
                            });
                        }
                    }
                }
            }
            return customers; // คืนค่าลูกค้าทั้งหมด
        }

        // GET: api/Customers
        [HttpGet]
        public IActionResult GetAllCustomers()
        {
            try
            {
                var customers = FetchAllCustomers(); // ดึงข้อมูลทั้งหมดจากฐานข้อมูล

                // ใช้ LINQ เพื่อจัดเรียงข้อมูลลูกค้าตามชื่อแบบ A-Z
                var sortedCustomers = customers.OrderBy(c => c.name).ToList();

                return Ok(sortedCustomers); // ส่งข้อมูลที่จัดเรียงแล้ว
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // GET: api/Customers/{id}
        [HttpGet("{id}")]
        public IActionResult GetCustomerById(int id)
        {
            try
            {
                var customers = FetchAllCustomers(); // ดึงข้อมูลทั้งหมดจากฐานข้อมูล

                // ใช้ LINQ เพื่อค้นหาลูกค้าที่มี id ตรงกับที่ระบุ
                var customer = customers.FirstOrDefault(c => c.id == id);

                if (customer == null)
                    return NotFound(); // ส่งสถานะ 404 หากไม่พบลูกค้า

                return Ok(customer); // ส่งข้อมูลลูกค้าในรูปแบบ JSON
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // POST: api/Customers
        [HttpPost]
        public IActionResult AddCustomer([FromBody] Customer newCustomer)
        {
            try
            {
                string connectionString = _configuration.GetConnectionString("DefaultConnection");

                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();

                    // ใช้คำสั่ง SQL เพื่อเพิ่มข้อมูลลูกค้า
                    string query = "INSERT INTO Customers (name, email) OUTPUT INSERTED.id VALUES (@name, @email)";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@name", newCustomer.name);
                        cmd.Parameters.AddWithValue("@email", newCustomer.email);

                        // ใช้ ExecuteScalar เพื่อดึง id ของลูกค้าใหม่
                        var newId = (int)cmd.ExecuteScalar();
                        newCustomer.id = newId;

                        return CreatedAtAction(nameof(GetCustomerById), new { id = newId }, newCustomer);
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // PUT: api/Customers/{id}
        [HttpPut("{id}")]
        public IActionResult UpdateCustomer(int id, [FromBody] Customer updatedCustomer)
        {
            try
            {
                string connectionString = _configuration.GetConnectionString("DefaultConnection");

                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();

                    // ใช้คำสั่ง SQL สำหรับอัปเดตข้อมูลลูกค้า
                    string query = "UPDATE Customers SET name = @name, email = @email WHERE id = @id";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@name", updatedCustomer.name);
                        cmd.Parameters.AddWithValue("@id", id);
                        cmd.Parameters.AddWithValue("@email", updatedCustomer.email);

                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            // ใช้ LINQ เพื่อตรวจสอบผลลัพธ์ของการอัปเดตในแอพ
                            var updatedCustomers = FetchAllCustomers().Where(c => c.id == id).ToList();

                            return Ok(updatedCustomers.First()); // ส่งข้อมูลที่อัปเดตกลับ
                        }
                        else
                        {
                            return NotFound(); // ส่งสถานะ 404 หากไม่พบข้อมูล
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
