using Microsoft.AspNetCore.Mvc; // นำเข้าไลบรารีที่ใช้ในการสร้าง Web API
using Microsoft.EntityFrameworkCore; // นำเข้าไลบรารีที่ใช้ในการติดต่อกับฐานข้อมูล (EF Core)
using BelleCroissantAPI.Models; // ใช้ namespace ของ model สำหรับใช้งานโมเดลต่าง ๆ ที่กำหนดไว้ในโปรเจกต์
using BelleCroissantAPI.Data; // ใช้ namespace สำหรับการเข้าถึงข้อมูลจากฐานข้อมูล

namespace BelleCroissantAPI.Controllers // กำหนด namespace สำหรับ controller ของ API
{
    [Route("api/[controller]")] // กำหนดเส้นทางของ URL ที่เข้าถึง API เช่น /api/customers
    [ApiController] // ระบุว่าเป็น API Controller เพื่อให้ ASP.NET Core จัดการคำขอ HTTP
    public class CustomersController : ControllerBase // กำหนด class ของ API Controller สำหรับการจัดการข้อมูลลูกค้า
    {
        private readonly ApplicationDbContext _context; // สร้างตัวแปรสำหรับเก็บข้อมูล ApplicationDbContext

        // Constructor: Inject ApplicationDbContext เพื่อใช้จัดการฐานข้อมูล
        public CustomersController(ApplicationDbContext context)
        {
            _context = context; // กำหนดค่า _context ให้เป็น instance ของ ApplicationDbContext ที่รับมาจาก constructor
        }

        // GET: api/customers
        // ดึงข้อมูลลูกค้าทั้งหมด
        [HttpGet] // กำหนด HTTP method เป็น GET
        public async Task<ActionResult<IEnumerable<Customer>>> GetCustomers()
        {
            // ใช้ LINQ เพื่อดึงข้อมูลลูกค้าทั้งหมดจากฐานข้อมูล
            var customers = await (from customer in _context.Customers
                                   select customer).ToListAsync(); // ทำการดึงข้อมูลและแปลงเป็น List แบบอะซิงโครนัส

            return Ok(customers); // คืนค่าผลลัพธ์ลูกค้าในรูปแบบ HTTP 200 OK
        }

        // GET: api/customers/5
        // ดึงข้อมูลลูกค้าตาม ID
        [HttpGet("{id}")] // กำหนด URL ที่รับค่า id ของลูกค้า
        public async Task<ActionResult<Customer>> GetCustomer(int id)
        {
            // ใช้ LINQ เพื่อดึงข้อมูลลูกค้าตาม ID
            var customer = await (from c in _context.Customers
                                  where c.CustomerId == id
                                  select c).FirstOrDefaultAsync(); // ค้นหาลูกค้าตาม ID และดึงมาแค่ 1 ตัว

            if (customer == null)
            {
                return NotFound(); // หากไม่พบข้อมูลลูกค้า ให้คืนค่าผลลัพธ์ HTTP 404 Not Found
            }

            return Ok(customer); // คืนค่าลูกค้าในรูปแบบ HTTP 200 OK
        }

        // POST: api/customers
        // เพิ่มข้อมูลลูกค้าใหม่
        [HttpPost] // กำหนด HTTP method เป็น POST สำหรับการเพิ่มข้อมูล
        public async Task<ActionResult<Customer>> PostCustomer(Customer customer)
        {
            // เพิ่มข้อมูลลูกค้าใหม่เข้า DbSet
            _context.Customers.Add(customer);
            await _context.SaveChangesAsync(); // บันทึกการเปลี่ยนแปลงในฐานข้อมูลแบบอะซิงโครนัส

            // Return 201 Created พร้อม URL ของข้อมูลที่สร้างใหม่
            return CreatedAtAction("GetCustomer", new { id = customer.CustomerId }, customer); // คืนค่าผลลัพธ์ HTTP 201 Created
        }

        // PUT: api/customers/5
        // อัปเดตข้อมูลลูกค้า
        [HttpPut("{id}")] // กำหนด URL สำหรับการอัปเดตข้อมูลลูกค้าตาม id
        public async Task<IActionResult> PutCustomer(int id, Customer customer)
        {
            // ตรวจสอบว่า ID ตรงกันหรือไม่
            if (id != customer.CustomerId)
            {
                return BadRequest(); // หาก ID ไม่ตรงกัน คืนค่าผลลัพธ์ HTTP 400 Bad Request
            }

            // ตั้งค่า Entity State เป็น Modified เพื่อเตรียมบันทึกการเปลี่ยนแปลง
            _context.Entry(customer).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync(); // บันทึกการเปลี่ยนแปลงในฐานข้อมูล
            }
            catch (DbUpdateConcurrencyException) // กรณีเกิดข้อผิดพลาดการอัปเดตข้อมูล
            {
                // ตรวจสอบว่าลูกค้าตาม ID มีอยู่หรือไม่
                if (!CustomerExists(id))
                {
                    return NotFound(); // หากไม่พบข้อมูลลูกค้า ให้คืนค่าผลลัพธ์ HTTP 404 Not Found
                }
                else
                {
                    throw; // หากมีข้อผิดพลาดอื่น ๆ ให้โยนข้อผิดพลาดออกไป
                }
            }

            return NoContent(); // คืนค่าผลลัพธ์ HTTP 204 No Content หากการอัปเดตสำเร็จ
        }

        // DELETE: api/customers/5
        // ลบข้อมูลลูกค้า
        [HttpDelete("{id}")] // กำหนด URL สำหรับการลบข้อมูลลูกค้าตาม id
        public async Task<ActionResult<Customer>> DeleteCustomer(int id)
        {
            // ค้นหาลูกค้าตาม ID
            var customer = await (from c in _context.Customers
                                  where c.CustomerId == id
                                  select c).FirstOrDefaultAsync(); // ค้นหาลูกค้าตาม ID และดึงมาแค่ 1 ตัว

            if (customer == null)
            {
                return NotFound(); // หากไม่พบข้อมูลลูกค้า ให้คืนค่าผลลัพธ์ HTTP 404 Not Found
            }

            // ลบข้อมูลลูกค้า
            _context.Customers.Remove(customer);
            await _context.SaveChangesAsync(); // บันทึกการเปลี่ยนแปลงในฐานข้อมูล

            return Ok(customer); // คืนค่าข้อมูลลูกค้าที่ถูกลบในรูปแบบ HTTP 200 OK
        }

        // Method: ตรวจสอบว่าลูกค้าตาม ID มีอยู่ในฐานข้อมูลหรือไม่
        private bool CustomerExists(int id)
        {
            // ใช้ LINQ เพื่อตรวจสอบว่ามีข้อมูลลูกค้าตาม ID หรือไม่
            return _context.Customers.Any(c => c.CustomerId == id);
        }
    }
}
