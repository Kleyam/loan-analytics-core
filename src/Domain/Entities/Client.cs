namespace PrepaymentIntelliBank.Domain.Entities
{
    public class Client
    {
        public Guid Id { get; private set; }

        public string Name { get; private set; }
        public string Cpf { get; private set; }
        public decimal MonthlyIncome { get; private set; }
        public string? Profession { get; private set; }
        public string EmploymentRelationship { get; private set; }

        // Representa o tempo de relacionamento do cliente com a instituição
        public DateTime AdmissionDate { get; private set; }

        public int Score { get; private set; }
        public decimal HistoricalDefaultRate { get; private set; }

        public Client(
            string name,
            string cpf,
            string employmentRelationship,
            DateTime admissionDate,
            decimal historicalDefaultRate,
            decimal monthlyIncome,
            string? profession)
        {
            Id = Guid.NewGuid();

            Validate(name, monthlyIncome, cpf, employmentRelationship);

            Name = name;
            Cpf = cpf;
            MonthlyIncome = monthlyIncome;
            Profession = profession;
            EmploymentRelationship = employmentRelationship;
            AdmissionDate = admissionDate;
            Score = 0;

            UpdateDefaultRate(historicalDefaultRate);
        }

        private void Validate(
            string name,
            decimal monthlyIncome,
            string cpf,
            string employmentRelationship)
        {

            ArgumentException.ThrowIfNullOrWhiteSpace(name, "Erro: O nome não pode ser vazio ou nulo.");
            ArgumentException.ThrowIfNullOrWhiteSpace(cpf, "Erro: O cpf não pode ser vazio ou nulo.");
            ArgumentException.ThrowIfNullOrWhiteSpace(employmentRelationship, "Erro: O Cliente deve ter pelo menos um tipo de vinculo empregatício.");

            if (monthlyIncome < 0)
            {
                throw new ArgumentException("Erro: A renda mensal do cliente não pode ser negativa.");
            }

        }

        public void UpdateScore(int newScore)
        {
            if (newScore < 0) throw new ArgumentOutOfRangeException(nameof(newScore), "Erro: O Score não pode ser menor que zero.");
            if (newScore > 1000) throw new ArgumentOutOfRangeException(nameof(newScore), "Erro: O Score máximo permitido é 1000.");

            Score = newScore;
        }

        public void UpdateDefaultRate(decimal newDefaultRate)
        {
            if (newDefaultRate < 0m) throw new ArgumentOutOfRangeException(nameof(newDefaultRate), "Erro: A taxa de inadimplência não pode ser menor que zero.");
            if (newDefaultRate > 100m) throw new ArgumentOutOfRangeException(nameof(newDefaultRate), "Erro: A taxa de inadimplência não pode ser maior que 100%.");

            HistoricalDefaultRate = newDefaultRate;
        }

        public int YearsAsClient => CustomerRelationshipTime(AdmissionDate, DateTime.Today);

        private static int CustomerRelationshipTime(DateTime start, DateTime end)
        {
            int years = end.Year - start.Year;

            if (end < start.AddYears(years))
            {
                years--;
            }

            return years;
        }
    }
}