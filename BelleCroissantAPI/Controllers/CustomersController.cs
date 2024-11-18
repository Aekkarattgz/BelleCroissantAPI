using Microsoft.AspNetCore.Mvc;
using BelleCroissantAPI.Model;
using System.Data.SqlClient;

namespace BelleCroissantAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CustomersController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public CustomersController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // GET: api/Customers
        // ดึงข้อมูลทั้งหมดของลูกค้า
        [HttpGet]
        public IActionResult GetAllCustomers()
        {
            try
            {
                List<Customer> customers = new List<Customer>();
                string connectionString = _configuration.GetConnectionString("DefaultConnection");

                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();
                    string query = "SELECT id, name, email FROM Customers";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var customer = new Customer
                                {
                                    id = reader.GetInt32(reader.GetOrdinal("id")),
                                    name = reader.GetString(reader.GetOrdinal("name")),
                                    email = reader.GetString(reader.GetOrdinal("email"))
                                };
                                customers.Add(customer);
                            }
                        }
                    }
                }

                return Ok(customers);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // GET: api/Customers/{id}
        // ดึงข้อมูลลูกค้าตาม id
        [HttpGet("{id}")]
        public IActionResult GetCustomerById(int id)
        {
            try
            {
                Customer customer = null;
                string connectionString = _configuration.GetConnectionString("DefaultConnection");

                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();
                    string query = "SELECT id, name, email FROM Customers WHERE id = @id";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@id", id);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                customer = new Customer
                                {
                                    id = reader.GetInt32(reader.GetOrdinal("id")),
                                    name = reader.GetString(reader.GetOrdinal("name")),
                                    email = reader.GetString(reader.GetOrdinal("email"))
                                };
                            }
                        }
                    }
                }

                if (customer == null)
                {
                    return NotFound(); // หากไม่พบข้อมูลลูกค้า
                }

                return Ok(customer);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // POST: api/Customers
        // เพิ่มลูกค้าใหม่
        [HttpPost]
        public IActionResult AddCustomer([FromBody] Customer newCustomer)
        {
            try
            {
                string connectionString = _configuration.GetConnectionString("DefaultConnection");

                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();
                    string query = "INSERT INTO Customers (name, email) VALUES (@name, @email)";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@name", newCustomer.name);
                        cmd.Parameters.AddWithValue("@email", newCustomer.email);

                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            return CreatedAtAction(nameof(GetCustomerById), new { id = newCustomer.id }, newCustomer);
                        }
                        else
                        {
                            return StatusCode(500, "Error inserting new customer.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // PUT: api/Customers/{id}
        // อัปเดตข้อมูลลูกค้าตาม id
        [HttpPut("{id}")]
        public IActionResult UpdateCustomer(int id, [FromBody] Customer updatedCustomer)
        {
            try
            {
                string connectionString = _configuration.GetConnectionString("DefaultConnection");

                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();
                    string query = "UPDATE Customers SET name = @name, email = @email WHERE id = @id";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@name", updatedCustomer.name);
                        cmd.Parameters.AddWithValue("@email", updatedCustomer.email);
                        cmd.Parameters.AddWithValue("@id", id);

                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            return NoContent(); // แสดงว่าอัปเดตสำเร็จ
                        }
                        else
                        {
                            return NotFound(); // หากไม่พบลูกค้าตาม id ที่ระบุ
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
