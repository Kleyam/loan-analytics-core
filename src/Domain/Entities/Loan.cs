namespace PrepaymentIntelliBank.Domain.Entities;

public class Loan
{
    public Guid Id { get; private set; }
    public Guid ClientId { get; private set; }

    public decimal LoanAmount { get; private set; }

    // Taxa de juros mensal, representada na escala 0-100 (ex: 5m = 5% ao mês).
    public decimal MonthlyLoanInterest { get; private set; }
    public decimal BalanceOwed { get; private set; }

    // Cronograma de Originação: Metadados do contrato, data de emissão e prazo total de amortização.
    public DateTime LoanCreationDate { get; private set; }
    public string AgreementNumber { get; private set; } = string.Empty;
    public int TotalInstallments { get; private set; }

    public Loan(
        Guid clientId,
        decimal loanAmount,
        decimal monthlyLoanInterest,
        string agreementNumber,
        DateTime loanCreationDate,
        int totalInstallments,
        DateTime? referenceDate = null)
    {
        Id = Guid.NewGuid();
        ClientId = clientId;

        DateTime dateValidation = referenceDate ?? DateTime.Today;

        Validate(clientId, loanAmount, monthlyLoanInterest, agreementNumber, totalInstallments, loanCreationDate, dateValidation);

        LoanAmount = loanAmount;
        MonthlyLoanInterest = monthlyLoanInterest;
        BalanceOwed = loanAmount;
        AgreementNumber = agreementNumber;
        LoanCreationDate = loanCreationDate;
        TotalInstallments = totalInstallments;
    }

    private static void Validate(
        Guid clientId,
        decimal loanAmount,
        decimal monthlyLoanInterest,
        string agreementNumber,
        int totalInstallments,
        DateTime loanCreationDate,
        DateTime referenceDate)
    {
        ArgumentOutOfRangeException.ThrowIfEqual(clientId, Guid.Empty);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(loanAmount, 0m);
        ArgumentOutOfRangeException.ThrowIfNegative(monthlyLoanInterest);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(totalInstallments);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(monthlyLoanInterest, 5m);

        ArgumentException.ThrowIfNullOrWhiteSpace(agreementNumber);

        ArgumentOutOfRangeException.ThrowIfGreaterThan(loanCreationDate, referenceDate);
    }
}
