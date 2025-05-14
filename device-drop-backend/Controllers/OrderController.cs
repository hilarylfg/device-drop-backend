using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using device_drop_backend.Data;
using device_drop_backend.Dtos;
using device_drop_backend.Models;
using device_drop_backend.Services;

[Route("api/[controller]")]
[ApiController]
public class OrderController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IEmailService _emailService;
    private readonly IPaymentService _paymentService;

    public OrderController(AppDbContext context, IEmailService emailService, IPaymentService paymentService)
    {
        _context = context;
        _emailService = emailService;
        _paymentService = paymentService;
    }

    [HttpPost("create")]
    public async Task<IActionResult> CreateOrder([FromBody] CheckoutFormValuesDto dto)
    {
        try
        {
            var cartToken = Request.Cookies["cartToken"];
            if (string.IsNullOrEmpty(cartToken))
            {
                return BadRequest(new { error = "Cart token not found" });
            }

            var userCart = await _context.Carts
                .Include(c => c.Items)
                .ThenInclude(i => i.ProductVariant)
                .ThenInclude(pv => pv.Product)
                .FirstOrDefaultAsync(c => c.Token == cartToken);

            if (userCart == null)
            {
                return NotFound(new { error = "Cart not found" });
            }

            if (userCart.TotalAmount == 0)
            {
                return BadRequest(new { error = "Cart is empty" });
            }

            var order = new Order
            {
                Token = cartToken,
                FirstName = dto.FirstName,
                Email = dto.Email,
                Phone = dto.Phone,
                Address = dto.Address,
                Comment = dto.Comment,
                TotalAmount = userCart.TotalAmount,
                Status = OrderStatus.PENDING,
                Items = JsonConvert.SerializeObject(userCart.Items.Select(i => new
                {
                    i.Id,
                    i.Quantity,
                    i.ProductVariantId,
                    Product = new
                    {
                        i.ProductVariant.Product.Id,
                        i.ProductVariant.Product.Name,
                        i.ProductVariant.Product.Description,
                        i.ProductVariant.Product.Brand
                    },
                    Variant = new
                    {
                        i.ProductVariant.Id,
                        i.ProductVariant.Price,
                        i.ProductVariant.SalePrice,
                        i.ProductVariant.Stock,
                        i.ProductVariant.ImageUrl,
                        i.ProductVariant.ColorId
                    }
                }))
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            userCart.TotalAmount = 0;
            _context.CartItems.RemoveRange(userCart.Items);
            await _context.SaveChangesAsync();

            var paymentData = await _paymentService.CreatePayment(new PaymentRequest
            {
                Amount = order.TotalAmount,
                OrderId = order.Id,
                Description = $"Оплата заказа #{order.Id}"
            });

            if (paymentData == null)
            {
                return StatusCode(500, new { error = "Payment data not found" });
            }

            order.PaymentId = paymentData.Id;
            await _context.SaveChangesAsync();

            var paymentUrl = paymentData.Confirmation.ConfirmationUrl;

            try
            {
                var pendingEmailBody = GeneratePendingEmail(order.Id, order.TotalAmount, paymentUrl);
                await _emailService.SendEmailAsync(
                    dto.Email,
                    $"DeviceDrop / Оплатите заказ #{order.Id}",
                    pendingEmailBody,
                    isHtml: true
                );
            }
            catch
            {
                // Игнорируем ошибку email, чтобы не прерывать процесс
            }

            return Ok(new { paymentUrl });
        }
        catch
        {
            return StatusCode(500, new { error = "Не удалось создать заказ" });
        }
    }

    [HttpPost("checkout/callback")]
    public async Task<IActionResult> Callback()
    {
        using var reader = new StreamReader(Request.Body);
        var body = await reader.ReadToEndAsync();

        PaymentCallbackData? data;

        try
        {
            data = JsonConvert.DeserializeObject<PaymentCallbackData>(body);
            if (data == null || data.Object == null)
                return BadRequest("Invalid data");
        }
        catch (Exception ex)
        {
            return BadRequest("Ошибка JSON");
        }
        
        var orderId = data.Object.Metadata?.OrderId;

        if (string.IsNullOrEmpty(orderId))
            return BadRequest("order_id не найден");

        var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id.ToString() == orderId);
        if (order == null)
        {
            return NotFound(new { error = "Order not found" });
        }

        order.Status = OrderStatus.SUCCEEDED;
        var isSucceeded = order.Status == OrderStatus.SUCCEEDED;
        var items = JsonConvert.DeserializeObject<OrderCartItemDTO[]>(order.Items);
        await _context.SaveChangesAsync();
        
        try
        {
            if (isSucceeded)
            {
                var successEmailBody = GenerateSuccessEmail(order.Id, items);
                await _emailService.SendEmailAsync(
                    order.Email,
                    $"DeviceDrop / Ваш заказ #{order.Id} успешно оформлен 🎉",
                    successEmailBody,
                    isHtml: true
                );
            }
            else
            {
                var cancelledEmailBody = GenerateCancelledEmail(order.Id);
                await _emailService.SendEmailAsync(
                    order.Email,
                    $"DeviceDrop / Оплата заказа #{order.Id} не удалась",
                    cancelledEmailBody,
                    isHtml: true
                );
            }
        }
        catch
        {
            // Игнорируем ошибку email
        }

        return Ok();
    }

    private string GeneratePendingEmail(int orderId, decimal totalAmount, string paymentUrl)
    {
        return $@"
            <html>
            <body style='font-family: Arial, sans-serif; line-height: 1.6;'>
                <h2>Ваш заказ #{orderId} ждёт оплаты</h2>
                <p>Спасибо за заказ в DeviceDrop!</p>
                <p>Сумма: {totalAmount:F2} руб.</p>
                <p>Для завершения оплаты перейдите по ссылке:</p>
                <a href='{paymentUrl}' style='background-color: #28a745; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px;'>Оплатить заказ</a>
                <p>Если у вас есть вопросы, свяжитесь с нами: support@devicedrop.ru</p>
            </body>
            </html>";
    }

    private string GenerateSuccessEmail(int orderId, OrderCartItemDTO[] items)
    {
        var itemsHtml = string.Join("", items.Select(i =>
            $"<li>{i.Product.Name} (x{i.Quantity}) - {(i.Variant.SalePrice ?? i.Variant.Price):F2} руб.</li>"));

        return $@"
            <html>
            <body style='font-family: Arial, sans-serif; line-height: 1.6;'>
                <h2>Ваш заказ #{orderId} успешно оплачен! 🎉</h2>
                <p>Спасибо за покупку в DeviceDrop!</p>
                <h3>Детали заказа:</h3>
                <ul>{itemsHtml}</ul>
                <p>Если у вас есть вопросы, свяжитесь с нами: support@devicedrop.ru</p>
            </body>
            </html>";
    }

    private string GenerateCancelledEmail(int orderId)
    {
        return $@"
            <html>
            <body style='font-family: Arial, sans-serif; line-height: 1.6;'>
                <h2>Оплата заказа #{orderId} не удалась</h2>
                <p>К сожалению, оплата вашего заказа в DeviceDrop была отменена.</p>
                <p>Пожалуйста, попробуйте снова или свяжитесь с нами для помощи.</p>
                <p>Контакты: support@devicedrop.ru</p>
            </body>
            </html>";
    }
}