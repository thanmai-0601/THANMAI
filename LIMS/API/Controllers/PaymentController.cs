using System.Security.Claims;
using Application.DTOs.Payment;
using Application.Interfaces.Services;
using Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PaymentController : ControllerBase
{
    private readonly IPaymentService _paymentService;
    private readonly IInvoiceService _invoiceService;

    public PaymentController(
        IPaymentService paymentService,
        IInvoiceService invoiceService)
    {
        _paymentService = paymentService;
        _invoiceService = invoiceService;
    }

    // GET api/payment/policy/{policyId}
    // GET api/payment/policy/{policyId}?type=Schedule
    [HttpGet("policy/{policyId:int}")]
    [Authorize(Roles = "Customer,Agent,Admin")]
    public async Task<IActionResult> GetPaymentInfo(int policyId, [FromQuery] string type = "History")
    {
        switch (type.ToLower())
        {
            case "schedule":
                return Ok(await _invoiceService.GetScheduleAsync(policyId));
            case "invoices":
                return Ok(await _invoiceService.GetPolicyInvoicesAsync(policyId));
            case "history":
            default:
                return Ok(await _paymentService.GetPaymentHistoryAsync(policyId));
        }
    }

    // GET api/payment/invoice/{invoiceId}
    [HttpGet("invoice/{invoiceId:int}")]
    [Authorize(Roles = "Customer,Agent,Admin")]
    public async Task<IActionResult> GetInvoice(int invoiceId)
    {
        var result = await _invoiceService.GetInvoiceAsync(invoiceId);
        return Ok(result);
    }

    [HttpPost("pay")]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> MakePayment(MakePaymentDto dto)
    {
        var result = await _paymentService.MakePaymentAsync(GetCurrentUserId(), dto);
        return Ok(result);
    }

    [HttpPost("policy/{policyId:int}/generate-schedule")]
    [Authorize(Roles = "Agent,Admin")]
    public async Task<IActionResult> GenerateSchedule(int policyId, [FromQuery] PaymentFrequency frequency = PaymentFrequency.Annual)
    {
        await _invoiceService.GenerateScheduleAsync(policyId, frequency);
        return Ok(new { message = "Invoice schedule generated successfully." });
    }

    [HttpGet("my")]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> GetMyPayments()
    {
        var result = await _paymentService.GetCustomerPaymentsAsync(GetCurrentUserId());
        return Ok(result);
    }

    private int GetCurrentUserId()
        => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}