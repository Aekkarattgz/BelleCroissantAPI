using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BelleCroissantAPI.Models;
using BelleCroissantAPI.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace BelleCroissantAPI.Controllers
{
    [Route("api/[controller]")] // ระบุว่า API นี้จะใช้เส้นทางฐานเป็น /api/products
    [ApiController] // กำหนดให้ Controller นี้เป็น API Controller เพื่อเพิ่มความสะดวกในการตรวจสอบข้อมูลที่ส่งมา
    public class ProductsController : ControllerBase
    {
        private readonly ApplicationDbContext _context; // ใช้สำหรับจัดการฐานข้อมูลผ่าน Entity Framework Core
        private readonly ILogger<ProductsController> _logger; // ใช้สำหรับบันทึกข้อความหรือข้อผิดพลาด

        // Constructor เพื่อ Inject ApplicationDbContext และ ILogger
        public ProductsController(ApplicationDbContext context, ILogger<ProductsController> logger)
        {
            _context = context; // รับค่าคอนเท็กซ์ฐานข้อมูลผ่าน Dependency Injection
            _logger = logger; // รับ Logger ผ่าน Dependency Injection
        }

        // GET: api/products
        [HttpGet] // กำหนดว่าเมธอดนี้จะทำงานเมื่อมีการเรียก HTTP GET ไปที่ /api/products
        public async Task<IActionResult> GetProducts()
        {
            try
            {
                // ดึงข้อมูลสินค้าทั้งหมดจากฐานข้อมูลแบบ Asynchronous
                var products = await _context.Products.ToListAsync();

                // ตรวจสอบว่ามีข้อมูลหรือไม่
                if (products == null || !products.Any())
                {
                    _logger.LogInformation("No products found."); // บันทึกข้อความว่าไม่มีข้อมูลสินค้า
                    return NotFound(new { message = "No products found." }); // ส่งสถานะ 404 พร้อมข้อความ
                }

                return Ok(products); // ส่งข้อมูลสินค้าในรูปแบบ JSON พร้อมสถานะ 200
            }
            catch (System.Exception ex)
            {
                // บันทึกข้อผิดพลาดในกรณีที่เกิด Exception
                _logger.LogError(ex, "An error occurred while retrieving products.");
                return StatusCode(500, new { message = "An error occurred while retrieving products.", error = ex.Message });
            }
        }

        // GET: api/products/{id}
        [HttpGet("{id}")] // กำหนดว่าเมธอดนี้จะทำงานเมื่อมีการเรียก HTTP GET พร้อมระบุ ID
        public async Task<IActionResult> GetProduct(int id)
        {
            try
            {
                // ค้นหาสินค้าตาม ID
                var product = await _context.Products.FirstOrDefaultAsync(p => p.ProductId == id);

                // หากไม่พบสินค้า
                if (product == null)
                {
                    return NotFound(new { message = $"Product with ID {id} not found." }); // ส่งสถานะ 404
                }

                return Ok(product); // ส่งข้อมูลสินค้าในรูปแบบ JSON พร้อมสถานะ 200
            }
            catch (System.Exception ex)
            {
                // บันทึกข้อผิดพลาดในกรณีที่เกิด Exception
                _logger.LogError(ex, "An error occurred while retrieving the product.");
                return StatusCode(500, new { message = "An error occurred while retrieving the product.", error = ex.Message });
            }
        }

        // POST: api/products
        [HttpPost] // กำหนดว่าเมธอดนี้จะทำงานเมื่อมีการเรียก HTTP POST ไปที่ /api/products
        public async Task<IActionResult> CreateProduct([FromBody] Product product) // รับข้อมูลสินค้าในรูปแบบ JSON จาก Body ของคำร้องขอ
        {
            // ตรวจสอบว่าโมเดลที่ส่งมาถูกต้องหรือไม่
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState); // ส่งสถานะ 400 พร้อมข้อมูลข้อผิดพลาดใน ModelState
            }

            try
            {
                _context.Products.Add(product); // เพิ่มสินค้าใหม่ลงในฐานข้อมูล
                await _context.SaveChangesAsync(); // บันทึกการเปลี่ยนแปลงในฐานข้อมูลแบบ Asynchronous

                // ส่งสถานะ 201 พร้อมข้อมูลสินค้าใหม่และลิงก์ไปยังการดึงข้อมูลสินค้าตาม ID
                return CreatedAtAction(nameof(GetProduct), new { id = product.ProductId }, product);
            }
            catch (System.Exception ex)
            {
                // บันทึกข้อผิดพลาดในกรณีที่เกิด Exception
                _logger.LogError(ex, "An error occurred while creating the product.");
                return StatusCode(500, new { message = "An error occurred while creating the product.", error = ex.Message });
            }
        }

        // PUT: api/products/{id}
        [HttpPut("{id}")] // กำหนดว่าเมธอดนี้จะทำงานเมื่อมีการเรียก HTTP PUT พร้อมระบุ ID
        public async Task<IActionResult> UpdateProduct(int id, [FromBody] Product product) // รับข้อมูลสินค้าใหม่จาก Body ของคำร้องขอ
        {
            // ตรวจสอบว่า ID ใน URL ตรงกับ ID ใน Body หรือไม่
            if (id != product.ProductId)
            {
                return BadRequest(new { message = "Product ID mismatch." }); // ส่งสถานะ 400 หากไม่ตรงกัน
            }

            // ตรวจสอบว่าโมเดลที่ส่งมาถูกต้องหรือไม่
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState); // ส่งสถานะ 400 พร้อมข้อมูลข้อผิดพลาดใน ModelState
            }

            try
            {
                _context.Entry(product).State = EntityState.Modified; // ตั้งค่า State ของสินค้าเป็น Modified
                await _context.SaveChangesAsync(); // บันทึกการเปลี่ยนแปลงในฐานข้อมูลแบบ Asynchronous

                return NoContent(); // ส่งสถานะ 204 เมื่ออัปเดตสำเร็จ
            }
            catch (DbUpdateConcurrencyException)
            {
                // หากไม่พบสินค้าระหว่างการอัปเดต
                if (!ProductExists(id))
                {
                    return NotFound(new { message = $"Product with ID {id} not found." }); // ส่งสถานะ 404
                }

                return StatusCode(500, new { message = "An error occurred while updating the product." });
            }
        }

        // DELETE: api/products/{id}
        [HttpDelete("{id}")] // กำหนดว่าเมธอดนี้จะทำงานเมื่อมีการเรียก HTTP DELETE พร้อมระบุ ID
        public async Task<IActionResult> DeleteProduct(int id)
        {
            try
            {
                // ค้นหาสินค้าตาม ID
                var product = await _context.Products.FindAsync(id);
                if (product == null)
                {
                    return NotFound(new { message = $"Product with ID {id} not found." }); // ส่งสถานะ 404 หากไม่พบสินค้า
                }

                _context.Products.Remove(product); // ลบสินค้าจากฐานข้อมูล
                await _context.SaveChangesAsync(); // บันทึกการเปลี่ยนแปลงในฐานข้อมูลแบบ Asynchronous

                return NoContent(); // ส่งสถานะ 204 เมื่อการลบสำเร็จ
            }
            catch (System.Exception ex)
            {
                // บันทึกข้อผิดพลาดในกรณีที่เกิด Exception
                _logger.LogError(ex, "An error occurred while deleting the product.");
                return StatusCode(500, new { message = "An error occurred while deleting the product.", error = ex.Message });
            }
        }

        // ตรวจสอบว่าสินค้ามีอยู่ในฐานข้อมูลหรือไม่
        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.ProductId == id); // คืนค่า true หากพบสินค้าในฐานข้อมูล
        }
    }
}
