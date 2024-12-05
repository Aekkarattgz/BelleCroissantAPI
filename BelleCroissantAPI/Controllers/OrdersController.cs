using Microsoft.AspNetCore.Mvc; // ใช้สำหรับการสร้าง Controller และ Action
using Microsoft.EntityFrameworkCore; // ใช้สำหรับการจัดการ Entity Framework Core
using BelleCroissantAPI.Models; // อ้างอิงไปยังโมเดล Order, Customer, Product, OrderItems
using BelleCroissantAPI.Data; // อ้างอิงไปยังคลาส ApplicationDbContext
using Microsoft.Extensions.Logging; // ใช้สำหรับการ log ข้อมูล
using System.Linq; // ใช้สำหรับ LINQ
using System.Threading.Tasks; // ใช้สำหรับการทำงานแบบ Asynchronous

namespace BelleCroissantAPI.Controllers
{
    [Route("api/[controller]")] // ระบุเส้นทางของ API (/api/Orders)
    [ApiController] // กำหนดให้ Controller นี้เป็น API Controller
    public class OrdersController : ControllerBase
    {
        private readonly ApplicationDbContext _context; // ตัวแปรสำหรับจัดการฐานข้อมูลผ่าน EF Core
        private readonly ILogger<OrdersController> _logger; // ตัวแปรสำหรับการ log ข้อมูล

        // Constructor เพื่อ Inject ApplicationDbContext และ ILogger
        public OrdersController(ApplicationDbContext context, ILogger<OrdersController> logger)
        {
            _context = context; // กำหนดค่าตัวแปร _context จาก context ที่ Inject เข้ามา
            _logger = logger; // กำหนดค่าตัวแปร _logger
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
                    .Take(100) // จำกัดผลลัพธ์ที่ดึงออกมาเป็น 50 แถว
                    .ToListAsync(); // เรียกข้อมูลในรูปแบบ List แบบ Asynchronous

                // เช็คว่ามีข้อมูลหรือไม่
                if (orders == null || !orders.Any())
                {
                    _logger.LogInformation("No orders found.");
                    return NotFound(new { message = "No orders found." }); // ถ้าไม่มีข้อมูลให้ส่งสถานะ 404
                }

                // ปริ้นข้อมูลที่ได้ออกมาดู
                foreach (var order in orders)
                {
                    _logger.LogInformation($"Order ID: {order.TransactionId}, Customer: {order.Customer?.FirstName} {order.Customer?.LastName}");
                    foreach (var orderItem in order.OrderItems)
                    {
                        _logger.LogInformation($"  Order Item ID: {orderItem.OrderItemId}, Product: {orderItem.Product?.ProductName}, Quantity: {orderItem.Quantity}");
                    }
                }

                return Ok(orders); // ส่งข้อมูลกลับในรูปแบบ JSON พร้อมสถานะ 200
            }
            catch (System.Exception ex)
            {
                // ส่งสถานะ 500 และข้อความเมื่อมีข้อผิดพลาด
                _logger.LogError(ex, "An error occurred while retrieving orders.");
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
                    _logger.LogInformation($"Order with ID {id} not found.");
                    return NotFound(new { message = $"Order with ID {id} not found." }); // ส่งสถานะ 404 ถ้าไม่พบ
                }

                // ปริ้นข้อมูลที่ได้ออกมาดู
                _logger.LogInformation($"Order ID: {order.TransactionId}, Customer: {order.Customer?.FirstName} {order.Customer?.LastName}");
                foreach (var orderItem in order.OrderItems)
                {
                    _logger.LogInformation($"  Order Item ID: {orderItem.OrderItemId}, Product: {orderItem.Product?.ProductName}, Quantity: {orderItem.Quantity}");
                }

                return Ok(order); // ส่งข้อมูลกลับในรูปแบบ JSON พร้อมสถานะ 200
            }
            catch (System.Exception ex)
            {
                // ส่งสถานะ 500 และข้อความเมื่อมีข้อผิดพลาด
                _logger.LogError(ex, "An error occurred while retrieving the order.");
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
                _logger.LogError(ex, "An error occurred while creating the order.");
                return StatusCode(500, new { message = "An error occurred while creating the order.", error = ex.Message });
            }
        }

        // PUT: api/Orders/{id}/complete
        [HttpPut("{id}/complete")] // ระบุว่าเป็น HTTP PUT สำหรับการทำเครื่องหมายคำสั่งซื้อให้เสร็จสมบูรณ์
        public async Task<IActionResult> CompleteOrder(int id)
        {
            try
            {
                var order = await _context.Orders.FindAsync(id);
                if (order == null)
                {
                    return NotFound(new { message = $"Order with ID {id} not found." });
                }

                order.Status = "Completed"; // ตั้งค่า Status เป็น "Completed"
                await _context.SaveChangesAsync(); // บันทึกการเปลี่ยนแปลง

                _logger.LogInformation($"Order {id} marked as complete.");
                return NoContent(); // ส่งสถานะ 204 เมื่อการอัปเดตเสร็จสมบูรณ์
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "An error occurred while completing the order.");
                return StatusCode(500, new { message = "An error occurred while completing the order.", error = ex.Message });
            }
        }

        // PUT: api/Orders/{id}/cancel
        [HttpPut("{id}/cancel")] // ระบุว่าเป็น HTTP PUT สำหรับการยกเลิกคำสั่งซื้อ
        public async Task<IActionResult> CancelOrder(int id)
        {
            try
            {
                var order = await _context.Orders.FindAsync(id);
                if (order == null)
                {
                    return NotFound(new { message = $"Order with ID {id} not found." });
                }

                order.Status = "Cancelled"; // ตั้งค่า Status เป็น "Cancelled"
                await _context.SaveChangesAsync(); // บันทึกการเปลี่ยนแปลง

                _logger.LogInformation($"Order {id} marked as cancelled.");
                return NoContent(); // ส่งสถานะ 204 เมื่อการยกเลิกเสร็จสมบูรณ์
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "An error occurred while canceling the order.");
                return StatusCode(500, new { message = "An error occurred while canceling the order.", error = ex.Message });
            }
        }
    }
}
