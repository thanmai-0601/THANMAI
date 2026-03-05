using Application.DTOs.Policy;
using Application.Interfaces.Services;
using API.Controllers;
using Domain.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;
using Xunit;

namespace API.Tests.Controllers;

public class PolicyControllerTests
{
    private readonly Mock<IPolicyService> _policyServiceMock;
    private readonly Mock<IPlanService> _planServiceMock;
    private readonly Mock<IAgentPolicyService> _agentPolicyMock;
    private readonly PolicyController _controller;

    public PolicyControllerTests()
    {
        _policyServiceMock = new Mock<IPolicyService>();
        _planServiceMock = new Mock<IPlanService>();
        _agentPolicyMock = new Mock<IAgentPolicyService>();
        
        _controller = new PolicyController(
            _planServiceMock.Object, 
            _policyServiceMock.Object, 
            _agentPolicyMock.Object);

        // Mock User identity
        var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim(ClaimTypes.NameIdentifier, "1"),
            new Claim(ClaimTypes.Role, "Customer")
        }, "mock"));

        _controller.ControllerContext = new ControllerContext()
        {
            HttpContext = new DefaultHttpContext() { User = user }
        };
    }

    [Fact]
    public async Task GetPlans_ShouldReturnOk()
    {
        // Arrange
        var plans = new List<PlanResponseDto>();
        _planServiceMock.Setup(s => s.GetAllPlansAsync(false)).ReturnsAsync(plans);

        // Act
        var result = await _controller.GetPlans();

        // Assert
        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task RequestPolicy_ShouldReturnOk_ForCustomer()
    {
        // Arrange
        var dto = new RequestPolicyDto { InsurancePlanId = 1 };
        var response = new PolicyResponseDto { PolicyNumber = "POL-123" };
        _policyServiceMock.Setup(s => s.RequestPolicyAsync(1, dto)).ReturnsAsync(response);

        // Act
        var result = await _controller.RequestPolicy(dto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal("POL-123", ((PolicyResponseDto)okResult.Value!).PolicyNumber);
    }
}
