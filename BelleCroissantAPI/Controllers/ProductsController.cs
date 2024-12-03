using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BelleCroissantAPI.Models;
using BelleCroissantAPI.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace BelleCroissantAPI.Controllers
{
    [Route("api/[controller]")] // ระบุเส้นทางของ API (/api/products)
    [ApiController] // กำหนดให้ Controller นี้เป็น API Controller
    public class ProductsController : ControllerBase
    {
        private readonly ApplicationDbContext _context; // ตัวแปรสำหรับจัดการฐานข้อมูลผ่าน EF Core
        private readonly ILogger<ProductsController> _logger;

        // Constructor เพื่อ Inject ApplicationDbContext
        public ProductsController(ApplicationDbContext context, ILogger<ProductsController> logger)
        {
            _context = context; // กำหนดค่าตัวแปร _context จาก context ที่ Inject เข้ามา
            _logger = logger;
        }

        // GET: api/products
        [HttpGet] // ระบุว่าเป็น HTTP GET
        public async Task<IActionResult> GetProducts()
        {
            try
            {
                var products = await _context.Products.ToListAsync(); // เรียกข้อมูลทั้งหมดจาก Products

                if (products == null || !products.Any())
                {
                    _logger.LogInformation("No products found.");
                    return NotFound(new { message = "No products found." }); // ถ้าไม่มีข้อมูลให้ส่งสถานะ 404
                }

                return Ok(products); // ส่งข้อมูลกลับในรูปแบบ JSON พร้อมสถานะ 200
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving products.");
                return StatusCode(500, new { message = "An error occurred while retrieving products.", error = ex.Message });
            }
        }

        // GET: api/products/{id}
        [HttpGet("{id}")] // ระบุว่าเป็น HTTP GET พร้อมรับค่า {id} จาก URL
        public async Task<IActionResult> GetProduct(int id)
        {
            try
            {
                var product = await _context.Products.FirstOrDefaultAsync(p => p.ProductId == id); // ค้นหาข้อมูล Product ตาม ID

                if (product == null)
                {
                    return NotFound(new { message = $"Product with ID {id} not found." }); // ส่งสถานะ 404 ถ้าไม่พบ
                }

                return Ok(product); // ส่งข้อมูลกลับในรูปแบบ JSON พร้อมสถานะ 200
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving the product.");
                return StatusCode(500, new { message = "An error occurred while retrieving the product.", error = ex.Message });
            }
        }

        // POST: api/products
        [HttpPost] // ระบุว่าเป็น HTTP POST
        public async Task<IActionResult> CreateProduct([FromBody] Product product) // รับข้อมูล Product จาก Body ของ Request
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState); // ส่งสถานะ 400 ถ้า ModelState ไม่ถูกต้อง
            }

            try
            {
                _context.Products.Add(product); // เพิ่ม Product ใหม่ลงในฐานข้อมูล
                await _context.SaveChangesAsync(); // บันทึกการเปลี่ยนแปลงในฐานข้อมูลแบบ Asynchronous

                return CreatedAtAction(nameof(GetProduct), new { id = product.ProductId }, product); // ส่งสถานะ 201 พร้อมข้อมูล Product และลิงก์ไปยัง GetProduct
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating the product.");
                return StatusCode(500, new { message = "An error occurred while creating the product.", error = ex.Message });
            }
        }

        // PUT: api/products/{id}
        [HttpPut("{id}")] // ระบุว่าเป็น HTTP PUT พร้อมรับค่า {id} จาก URL
        public async Task<IActionResult> UpdateProduct(int id, [FromBody] Product product) // รับข้อมูล Product ที่ต้องการอัปเดตจาก Body ของ Request
        {
            if (id != product.ProductId)
            {
                return BadRequest(new { message = "Product ID mismatch." }); // ส่งสถานะ 400 ถ้าไม่ตรงกัน
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState); // ส่งสถานะ 400 ถ้า ModelState ไม่ถูกต้อง
            }

            try
            {
                _context.Entry(product).State = EntityState.Modified; // ตั้งค่า State ของ Product เป็น Modified
                await _context.SaveChangesAsync(); // บันทึกการเปลี่ยนแปลงในฐานข้อมูลแบบ Asynchronous

                return NoContent(); // ส่งสถานะ 204 เมื่ออัปเดตสำเร็จ
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductExists(id))
                {
                    return NotFound(new { message = $"Product with ID {id} not found." }); // ส่งสถานะ 404 ถ้าไม่พบ
                }

                return StatusCode(500, new { message = "An error occurred while updating the product." });
            }
        }

        // DELETE: api/products/{id}
        [HttpDelete("{id}")] // ระบุว่าเป็น HTTP DELETE พร้อมรับค่า {id} จาก URL
        public async Task<IActionResult> DeleteProduct(int id)
        {
            try
            {
                var product = await _context.Products.FindAsync(id); // ค้นหาข้อมูล Product ตาม ID
                if (product == null)
                {
                    return NotFound(new { message = $"Product with ID {id} not found." }); // ส่งสถานะ 404 ถ้าไม่พบ
                }

                _context.Products.Remove(product); // ลบ Product จากฐานข้อมูล
                await _context.SaveChangesAsync(); // บันทึกการเปลี่ยนแปลงในฐานข้อมูลแบบ Asynchronous

                return NoContent(); // ส่งสถานะ 204 เมื่อการลบสำเร็จ
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting the product.");
                return StatusCode(500, new { message = "An error occurred while deleting the product.", error = ex.Message });
            }
        }

        // ตรวจสอบว่า Product มีอยู่ในฐานข้อมูลหรือไม่
        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.ProductId == id); // คืนค่า true ถ้าพบ Product ที่มี ID ตรงกัน
        }
    }
}
