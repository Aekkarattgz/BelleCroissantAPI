using Microsoft.AspNetCore.Mvc; // ใช้สำหรับการสร้าง Controller และ Action
using Microsoft.EntityFrameworkCore; // ใช้สำหรับการจัดการ Entity Framework Core
using BelleCroissantAPI.Models; // อ้างอิงไปยังโมเดล Order, Customer, Product, OrderItems
using BelleCroissantAPI.Data; // อ้างอิงไปยังคลาส ApplicationDbContext
using System.Linq; // ใช้สำหรับ LINQ
using System.Threading.Tasks; // ใช้สำหรับการทำงานแบบ Asynchronous

namespace BelleCroissantAPI.Controllers
{
    [Route("api/[controller]")] // ระบุเส้นทางของ API (/api/Orders)
    [ApiController] // กำหนดให้ Controller นี้เป็น API Controller
    public class OrdersController : ControllerBase
    {
        private readonly ApplicationDbContext _context; // ตัวแปรสำหรับจัดการฐานข้อมูลผ่าน EF Core

        // Constructor เพื่อ Inject ApplicationDbContext
        public OrdersController(ApplicationDbContext context)
        {
            _context = context; // กำหนดค่าตัวแปร _context จาก context ที่ Inject เข้ามา
        }

        // GET: api/Orders
        [HttpGet] // ระบุว่าเป็น HTTP GET
        public async Task<IActionResult> GetOrders()
        {
            try
            {
                // Query ข้อมูล Order พร้อม Include ข้อมูลที่สัมพันธ์กัน
                var orders = await _context.Orders
                    .Include(o => o.Customer) // Include ข้อมูล Customer
                    .Include(o => o.OrderItems) // Include ข้อมูล OrderItems
                    .ThenInclude(oi => oi.Product) // Include Product ภายใน OrderItems
                    .AsNoTracking() // ใช้ AsNoTracking เพื่อปรับปรุงประสิทธิภาพสำหรับการอ่านข้อมูล
                    .ToListAsync(); // เรียกข้อมูลในรูปแบบ List แบบ Asynchronous

                // เช็คว่ามีข้อมูลหรือไม่
                if (orders == null || !orders.Any())
                {
                    return NotFound(new { message = "No orders found." }); // ถ้าไม่มีข้อมูลให้ส่งสถานะ 404
                }

                return Ok(orders); // ส่งข้อมูลกลับในรูปแบบ JSON พร้อมสถานะ 200
            }
            catch (System.Exception ex)
            {
                // ส่งสถานะ 500 และข้อความเมื่อมีข้อผิดพลาด
                return StatusCode(500, new { message = "An error occurred while retrieving orders.", error = ex.Message });
            }
        }

        // GET: api/Orders/{id}
        [HttpGet("{id}")] // ระบุว่าเป็น HTTP GET พร้อมรับค่า {id} จาก URL
        public async Task<IActionResult> GetOrder(int id)
        {
            try
            {
                // Query ข้อมูล Order ตาม ID พร้อม Include ข้อมูลที่สัมพันธ์กัน
                var order = await _context.Orders
                    .Include(o => o.Customer) // Include ข้อมูล Customer
                    .Include(o => o.OrderItems) // Include ข้อมูล OrderItems
                    .ThenInclude(oi => oi.Product) // Include Product ภายใน OrderItems
                    .AsNoTracking() // ใช้ AsNoTracking เพื่อปรับปรุงประสิทธิภาพสำหรับการอ่านข้อมูล
                    .FirstOrDefaultAsync(o => o.TransactionId == id); // หา Order ที่ตรงกับ ID

                // เช็คว่าพบ Order หรือไม่
                if (order == null)
                {
                    return NotFound(new { message = $"Order with ID {id} not found." }); // ส่งสถานะ 404 ถ้าไม่พบ
                }

                return Ok(order); // ส่งข้อมูลกลับในรูปแบบ JSON พร้อมสถานะ 200
            }
            catch (System.Exception ex)
            {
                // ส่งสถานะ 500 และข้อความเมื่อมีข้อผิดพลาด
                return StatusCode(500, new { message = "An error occurred while retrieving the order.", error = ex.Message });
            }
        }

        // POST: api/Orders
        [HttpPost] // ระบุว่าเป็น HTTP POST
        public async Task<IActionResult> CreateOrder([FromBody] Order order) // รับข้อมูล Order จาก Body ของ Request
        {
            // ตรวจสอบความถูกต้องของ ModelState
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState); // ส่งสถานะ 400 ถ้า ModelState ไม่ถูกต้อง
            }

            try
            {
                _context.Orders.Add(order); // เพิ่ม Order ใหม่ลงในฐานข้อมูล
                await _context.SaveChangesAsync(); // บันทึกการเปลี่ยนแปลงในฐานข้อมูลแบบ Asynchronous

                // ส่งสถานะ 201 พร้อมข้อมูล Order และลิงก์ไปยัง GetOrder
                return CreatedAtAction(nameof(GetOrder), new { id = order.TransactionId }, order);
            }
            catch (System.Exception ex)
            {
                // ส่งสถานะ 500 และข้อความเมื่อมีข้อผิดพลาด
                return StatusCode(500, new { message = "An error occurred while creating the order.", error = ex.Message });
            }
        }

        // PUT: api/Orders/{id}
        [HttpPut("{id}")] // ระบุว่าเป็น HTTP PUT พร้อมรับค่า {id} จาก URL
        public async Task<IActionResult> UpdateOrder(int id, [FromBody] Order order) // รับ Order ที่ต้องการอัปเดตจาก Body ของ Request
        {
            // ตรวจสอบว่า ID ใน URL และ Order ตรงกันหรือไม่
            if (id != order.TransactionId)
            {
                return BadRequest(new { message = "Order ID mismatch." }); // ส่งสถานะ 400 ถ้าไม่ตรงกัน
            }

            // ตรวจสอบความถูกต้องของ ModelState
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState); // ส่งสถานะ 400 ถ้า ModelState ไม่ถูกต้อง
            }

            try
            {
                _context.Entry(order).State = EntityState.Modified; // ตั้งค่า State ของ Order เป็น Modified
                await _context.SaveChangesAsync(); // บันทึกการเปลี่ยนแปลงในฐานข้อมูลแบบ Asynchronous

                return NoContent(); // ส่งสถานะ 204 เมื่ออัปเดตสำเร็จ
            }
            catch (DbUpdateConcurrencyException)
            {
                // ตรวจสอบว่า Order มีอยู่จริงหรือไม่
                if (!OrderExists(id))
                {
                    return NotFound(new { message = $"Order with ID {id} not found." }); // ส่งสถานะ 404 ถ้าไม่พบ
                }

                // ส่งสถานะ 500 หากเกิดข้อผิดพลาดอื่น
                return StatusCode(500, new { message = "An error occurred while updating the order." });
            }
        }

        // DELETE: api/Orders/{id}
        [HttpDelete("{id}")] // ระบุว่าเป็น HTTP DELETE พร้อมรับค่า {id} จาก URL
        public async Task<IActionResult> DeleteOrder(int id)
        {
            try
            {
                // ค้นหา Order ตาม ID
                var order = await _context.Orders.FindAsync(id);
                if (order == null)
                {
                    return NotFound(new { message = $"Order with ID {id} not found." }); // ส่งสถานะ 404 ถ้าไม่พบ
                }

                _context.Orders.Remove(order); // ลบ Order จากฐานข้อมูล
                await _context.SaveChangesAsync(); // บันทึกการเปลี่ยนแปลงในฐานข้อมูลแบบ Asynchronous

                return NoContent(); // ส่งสถานะ 204 เมื่อการลบสำเร็จ
            }
            catch (System.Exception ex)
            {
                // ส่งสถานะ 500 และข้อความเมื่อมีข้อผิดพลาด
                return StatusCode(500, new { message = "An error occurred while deleting the order.", error = ex.Message });
            }
        }

        // ตรวจสอบว่า Order มีอยู่ในฐานข้อมูลหรือไม่
        private bool OrderExists(int id)
        {
            return _context.Orders.Any(e => e.TransactionId == id); // คืนค่า true ถ้าพบ Order ที่มี ID ตรงกัน
        }
    }
}
