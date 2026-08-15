namespace PrepaymentIntelliBank.Domain.Entities;

public class Loan
{
    public Guid Id { get; private set; }
    public Guid ClientId { get; private set; }

    public decimal LoanAmount { get; private set; }
    public decimal MonthlyLoanInterest { get; private set; }
    public decimal BalanceOwed { get; private set; }

    // Cronograma de Originação: Metadados do contrato, data de emissão e prazo total de amortização.
    public DateTime LoanCreationDate { get; private set; }
    public string AgreementNumber { get; private set; }
    public int TotalInstallments { get; private set; }

    public Loan(
        Guid clientId,
        decimal loanAmount,
        decimal monthlyLoanInterest,
        string agreementNumber,
        DateTime loanCreationDate,
        int totalInstallments,
        DateTime referenceDate)
    {
        Id = Guid.NewGuid();
        ClientId = clientId;

        Validate(loanAmount, monthlyLoanInterest, agreementNumber, totalInstallments, loanCreationDate, referenceDate);

        LoanAmount = loanAmount;
        MonthlyLoanInterest = monthlyLoanInterest;
        BalanceOwed = loanAmount;
        AgreementNumber = agreementNumber;
        LoanCreationDate = loanCreationDate;
        TotalInstallments = totalInstallments;
    }

    private static void Validate(
        decimal loanAmount,
        decimal monthlyLoanInterest,
        string agreementNumber,
        int totalInstallments,
        DateTime loanCreationDate,
        DateTime referenceDate)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(loanAmount, 0m, "O valor do empréstimo deve ser maior que zero.");
        ArgumentOutOfRangeException.ThrowIfLessThan(monthlyLoanInterest, 0m, "A Taxa de Juros não pode ser menor que zero.");
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(totalInstallments);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(monthlyLoanInterest, 5m, "A Taxa de Juros mensal não pode ser maior que 5%.");

        ArgumentException.ThrowIfNullOrWhiteSpace(agreementNumber, "O número do contrato de empréstimo é obrigatório.");

        ArgumentOutOfRangeException.ThrowIfGreaterThan(loanCreationDate, referenceDate, "A data de criação não pode ser futura.");
    }
}
