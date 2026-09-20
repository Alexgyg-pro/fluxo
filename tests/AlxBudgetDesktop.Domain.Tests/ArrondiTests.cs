using AlxBudgetDesktop.Domain;
using Xunit;

namespace AlxBudgetDesktop.Domain.Tests;

public class ArrondiTests
{
    [Theory]
    [InlineData(0.1, 0.2, 0.3)]
    [InlineData(12.345, 0, 12.35)]
    [InlineData(12.344, 0, 12.34)]
    [InlineData(-1.005, 0, -1.01)]
    public void Arrondi2_arrondit_correctement(decimal a, decimal b, decimal attendu)
    {
        Assert.Equal(attendu, Arrondi.Arrondi2(a + b));
    }
}
