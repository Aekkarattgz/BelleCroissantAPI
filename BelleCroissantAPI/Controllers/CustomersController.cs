using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BelleCroissantAPI.Models;
using BelleCroissantAPI.Data;

namespace BelleCroissantAPI.Controllers
{
    [Route("api/[controller]")] // กำหนดเส้นทาง API ให้เริ่มต้นด้วย "api/customers"
    [ApiController] // ระบุว่าเป็น API Controller สำหรับการจัดการคำขอ HTTP
    public class CustomersController : ControllerBase
    {
        private readonly ApplicationDbContext _context; // ตัวแปรสำหรับจัดการฐานข้อมูลผ่าน EF Core

        // Constructor: ใช้ Dependency Injection เพื่อรับ ApplicationDbContext
        public CustomersController(ApplicationDbContext context)
        {
            _context = context; // กำหนดค่าให้ _context เพื่อใช้งานใน Controller
        }

        // ** GET /api/customers **
        // ฟังก์ชันสำหรับดึงข้อมูลลูกค้าทั้งหมด
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Customer>>> GetAllCustomers()
        {
            // ใช้ LINQ เพื่อดึงรายการลูกค้าทั้งหมดจากฐานข้อมูล
            var customers = await _context.Customers.ToListAsync();

            // คืนค่ารายการลูกค้าในรูปแบบ HTTP 200 OK
            return Ok(customers);
        }

        // ** GET /api/customers/{id} **
        // ฟังก์ชันสำหรับดึงข้อมูลลูกค้าตาม ID
        [HttpGet("{id}")]
        public async Task<ActionResult<Customer>> GetCustomerById(int id)
        {
            // ใช้ FindAsync เพื่อตรวจสอบข้อมูลลูกค้าในฐานข้อมูลตาม ID
            var customer = await _context.Customers.FindAsync(id);

            // หากไม่พบลูกค้า คืนค่าผลลัพธ์ HTTP 404 Not Found พร้อมข้อความแจ้ง
            if (customer == null)
            {
                return NotFound(new { Message = $"Customer with ID {id} not found." });
            }

            // หากพบลูกค้า คืนค่าผลลัพธ์ HTTP 200 OK พร้อมข้อมูลลูกค้า
            return Ok(customer);
        }

        // ** POST /api/customers **
        // ฟังก์ชันสำหรับเพิ่มข้อมูลลูกค้าใหม่
        [HttpPost]
        public async Task<ActionResult<Customer>> AddCustomer(Customer customer)
        {
            // ทำให้ Orders เป็น null เพื่อไม่ให้สร้างคำสั่งที่ไม่จำเป็น
            customer.Orders = null;

            // ตรวจสอบว่าข้อมูลที่ส่งมาถูกต้องหรือไม่
            if (!ModelState.IsValid)
            {
                // หากข้อมูลไม่ถูกต้อง คืนค่าผลลัพธ์ HTTP 400 Bad Request พร้อมข้อผิดพลาด
                return BadRequest(ModelState);
            }

            // เพิ่มข้อมูลลูกค้าใหม่เข้าไปในฐานข้อมูล
            _context.Customers.Add(customer);
            await _context.SaveChangesAsync(); // บันทึกการเปลี่ยนแปลง

            // คืนค่าผลลัพธ์ HTTP 201 Created พร้อม URL และข้อมูลลูกค้าใหม่
            return CreatedAtAction(nameof(GetCustomerById), new { id = customer.CustomerId }, customer);
        }

        // ** PUT /api/customers/{id} **
        // ฟังก์ชันสำหรับแก้ไขข้อมูลลูกค้า
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCustomer(int id, Customer customer)
        {
            // ตรวจสอบว่า ID ที่ส่งมาตรงกับข้อมูลลูกค้าหรือไม่
            if (id != customer.CustomerId)
            {
                // หาก ID ไม่ตรงกัน คืนค่าผลลัพธ์ HTTP 400 Bad Request
                return BadRequest(new { Message = "Customer ID does not match." });
            }

            // ระบุสถานะ Entity เป็น Modified เพื่อเตรียมบันทึกการเปลี่ยนแปลง
            _context.Entry(customer).State = EntityState.Modified;

            try
            {
                // บันทึกการเปลี่ยนแปลงในฐานข้อมูล
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                // ตรวจสอบว่ามีลูกค้าตาม ID ในฐานข้อมูลหรือไม่
                if (!CustomerExists(id))
                {
                    // หากไม่พบข้อมูลลูกค้า คืนค่าผลลัพธ์ HTTP 404 Not Found
                    return NotFound(new { Message = $"Customer with ID {id} not found." });
                }
                else
                {
                    // หากเกิดข้อผิดพลาดอื่น ให้โยนข้อผิดพลาดออกไป
                    throw;
                }
            }

            // คืนค่าผลลัพธ์ HTTP 204 No Content เมื่อการแก้ไขสำเร็จ
            return NoContent();
        }

        // ** Method สำหรับตรวจสอบว่ามีข้อมูลลูกค้าตาม ID หรือไม่ **
        private bool CustomerExists(int id)
        {
            // ใช้ LINQ เพื่อตรวจสอบว่ามีลูกค้าในฐานข้อมูลตาม ID หรือไม่
            return _context.Customers.Any(c => c.CustomerId == id);
        }
    }
}
