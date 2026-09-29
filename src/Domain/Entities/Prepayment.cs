namespace PrepaymentIntelliBank.Domain.Entities;

public class Prepayment
{
    public Guid Id { get; private set; }
    public Guid LoanId { get; private set; }

    public decimal PrepaymentAmount { get; private set; }

    public DateTime PrepaymentDate { get; private set; }

    public decimal InterestSavings { get; private set; }

    public SourceOfFunds FundSource { get; private set; }
    public PrepaymentStrategy AppliedStrategy { get; private set; }

    public enum SourceOfFunds : byte
    {
        OwnCash = 1,
        Refinancing = 2,
        AssetSale = 3,
        Other = 4
    }

    public enum PrepaymentStrategy : byte
    {
        /// <summary>
        /// Mantém o prazo original do contrato e recalcula o saldo devedor para reduzir o valor das parcelas mensais consecutivas.
        /// </summary>
        ReductionOfInstallment = 1,

        /// <summary>
        /// Mantém o valor atual das parcelas e amortiza o saldo devedor diretamente no prazo, reduzindo o número de meses para a quitação do contrato.
        /// </summary>
        ReductionOfTerm = 2
    }

    public Prepayment(
        Guid loanId,
        decimal prepaymentAmount,
        DateTime prepaymentDate,
        decimal interestSavings,
        SourceOfFunds sourceOfFunds,
        PrepaymentStrategy prepaymentStrategy,
        DateTime? referenceDate = null)
    {
        Id = Guid.NewGuid();
        LoanId = loanId;

        DateTime dateValidation = referenceDate ?? DateTime.Today;

        Validate(prepaymentAmount, prepaymentDate, dateValidation, loanId);

        (FundSource, AppliedStrategy) = ValidateAndParseEnums(sourceOfFunds, prepaymentStrategy);

        PrepaymentAmount = prepaymentAmount;
        InterestSavings = interestSavings;
        PrepaymentDate = prepaymentDate;
    }

    private static void Validate(decimal prepaymentAmount, DateTime prepaymentDate, DateTime dateValidation, Guid loanId)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(prepaymentAmount, 0m);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(prepaymentDate, dateValidation);
        ArgumentOutOfRangeException.ThrowIfEqual(loanId, Guid.Empty);
    }

    private static (SourceOfFunds Source, PrepaymentStrategy Strategy) ValidateAndParseEnums(SourceOfFunds sourceOfFunds, PrepaymentStrategy prepaymentStrategy)
    {
        if (!Enum.IsDefined(sourceOfFunds))
            throw new ArgumentOutOfRangeException(nameof(sourceOfFunds), "Origem dos fundos inválida.");

        if (!Enum.IsDefined(prepaymentStrategy))
            throw new ArgumentOutOfRangeException(nameof(prepaymentStrategy), "Estratégia de amortização inválida.");

        return (sourceOfFunds, prepaymentStrategy);
    }
}