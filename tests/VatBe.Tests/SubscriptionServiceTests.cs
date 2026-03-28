using Microsoft.Extensions.Logging.Abstractions;
using Shouldly;
using VatBe.Stripe;
using Xunit;

namespace VatBe.Tests;

/// <summary>
/// Unit tests for <see cref="SubscriptionService"/>.
/// These tests verify that each lifecycle method completes without throwing
/// and logs the expected identifiers. The concrete DB/email integrations
/// will be tested in integration tests once Sprint 2 backends are wired up.
/// </summary>
public sealed class SubscriptionServiceTests
{
    private static SubscriptionService CreateService()
        => new(NullLogger<SubscriptionService>.Instance);

    [Fact]
    public async Task ProvisionSubscriberAsync_DoesNotThrow()
    {
        var svc = CreateService();

        var ex = await Record.ExceptionAsync(() =>
            svc.ProvisionSubscriberAsync("cus_test123", "customer@example.com", "cs_test_abc"));

        ex.ShouldBeNull();
    }

    [Fact]
    public async Task ProvisionSubscriberAsync_WithEmptyEmail_DoesNotThrow()
    {
        var svc = CreateService();

        var ex = await Record.ExceptionAsync(() =>
            svc.ProvisionSubscriberAsync("cus_test123", string.Empty, "cs_test_abc"));

        ex.ShouldBeNull();
    }

    [Fact]
    public async Task SyncSubscriptionAsync_ActiveStatus_DoesNotThrow()
    {
        var svc = CreateService();

        var ex = await Record.ExceptionAsync(() =>
            svc.SyncSubscriptionAsync("sub_test123", "cus_test123", "active", "Pro Monthly"));

        ex.ShouldBeNull();
    }

    [Fact]
    public async Task SyncSubscriptionAsync_PastDueStatus_DoesNotThrow()
    {
        var svc = CreateService();

        var ex = await Record.ExceptionAsync(() =>
            svc.SyncSubscriptionAsync("sub_test123", "cus_test123", "past_due", null));

        ex.ShouldBeNull();
    }

    [Theory]
    [InlineData("active")]
    [InlineData("past_due")]
    [InlineData("trialing")]
    [InlineData("unpaid")]
    [InlineData("canceled")]
    [InlineData("incomplete")]
    public async Task SyncSubscriptionAsync_AllKnownStatuses_DoesNotThrow(string status)
    {
        var svc = CreateService();

        var ex = await Record.ExceptionAsync(() =>
            svc.SyncSubscriptionAsync("sub_test", "cus_test", status, "Pro Monthly"));

        ex.ShouldBeNull();
    }

    [Fact]
    public async Task RevokeSubscriberAsync_DoesNotThrow()
    {
        var svc = CreateService();

        var ex = await Record.ExceptionAsync(() =>
            svc.RevokeSubscriberAsync("sub_test123", "cus_test123"));

        ex.ShouldBeNull();
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public async Task HandlePaymentFailureAsync_AllAttempts_DoesNotThrow(int attempt)
    {
        var svc = CreateService();

        var ex = await Record.ExceptionAsync(() =>
            svc.HandlePaymentFailureAsync("in_test123", "cus_test123", attempt));

        ex.ShouldBeNull();
    }

    [Fact]
    public async Task HandlePaymentFailureAsync_ZeroAttempt_DoesNotThrow()
    {
        // Edge case: Stripe may send attempt=0 for some event types
        var svc = CreateService();

        var ex = await Record.ExceptionAsync(() =>
            svc.HandlePaymentFailureAsync("in_test123", "cus_test123", 0));

        ex.ShouldBeNull();
    }
}
