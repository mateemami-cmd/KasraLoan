using FluentAssertions;
using KasraLoan.Application.Interfaces.Services;
using KasraLoan.Application.Services;
using Xunit;

namespace KasraLoan.Tests.Payments;

public class MockPaymentGatewayTests
{
    private readonly MockPaymentGateway _sut = new();

    private static GatewayCardInput Card(string secondPassword = "123456", string card = "6037991234567890")
        => new()
        {
            CardNumber = card,
            Cvv2 = "123",
            ExpiryMonth = "08",
            ExpiryYear = "07",
            SecondPassword = secondPassword,
        };

    [Fact]
    public void Six_Digit_Second_Password_Succeeds()
    {
        _sut.Authorize(Card("123456")).IsSuccessful.Should().BeTrue();
    }

    [Theory]
    [InlineData("12345")]      // کمتر از ۶
    [InlineData("1234567")]    // بیشتر از ۶
    [InlineData("")]
    [InlineData("12345a")]     // حرف
    [InlineData("123 56")]     // فاصله
    [InlineData("۱۲۳۴۵")]      // پنج رقم فارسی
    public void Second_Password_Must_Be_Exactly_Six_Digits(string password)
    {
        var result = _sut.Authorize(Card(password));

        result.IsSuccessful.Should().BeFalse();
        result.FailureReason.Should().Contain("رمز دوم");
    }

    [Theory]
    [InlineData("۱۲۳۴۵۶")]  // ارقام فارسی
    [InlineData("١٢٣٤٥٦")]  // ارقام عربی
    public void Persian_And_Arabic_Digits_Are_Normalized(string password)
    {
        // کاربر ایرانی ممکن است با کیبورد فارسی تایپ کند. نکته‌ی مهم: در دات‌نت
        // \d این ارقام را می‌گیرد، پس اگر نرمال‌سازی نمی‌شد بی‌سروصدا رد می‌شدند
        // بدون این‌که واقعاً عدد اسکی باشند.
        _sut.Authorize(Card(password)).IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void Normalized_Persian_Zero_Still_Triggers_The_Failure_Path()
    {
        // «۰۱۲۳۴۵» بعد از نرمال‌سازی «012345» است و باید همان مسیر شکست را برود.
        var result = _sut.Authorize(Card("۰۱۲۳۴۵"));

        result.IsSuccessful.Should().BeFalse();
        result.FailureReason.Should().Contain("تأیید نشد");
    }

    [Fact]
    public void Card_Number_Must_Be_Sixteen_Digits()
    {
        var result = _sut.Authorize(Card(card: "1234"));

        result.IsSuccessful.Should().BeFalse();
        result.FailureReason.Should().Contain("۱۶ رقم");
    }

    [Fact]
    public void Card_Number_May_Contain_Separators()
    {
        // کاربر معمولاً با خط تیره یا فاصله وارد می‌کند.
        _sut.Authorize(Card(card: "6037-9912-3456-7890")).IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void Second_Password_Starting_With_Zero_Fails_On_Purpose()
    {
        // مسیر شکست، تا در دمو بشود حالت ناموفق را هم نشان داد.
        var result = _sut.Authorize(Card("012345"));

        result.IsSuccessful.Should().BeFalse();
        result.FailureReason.Should().Contain("تأیید نشد");
    }

    [Fact]
    public void Reference_Id_Is_Twelve_Digits()
    {
        _sut.GenerateReferenceId().Should().MatchRegex(@"^\d{12}$");
    }

    [Fact]
    public void Missing_Cvv2_Is_Rejected()
    {
        var input = Card();
        input.Cvv2 = "";

        _sut.Authorize(input).IsSuccessful.Should().BeFalse();
    }
}
