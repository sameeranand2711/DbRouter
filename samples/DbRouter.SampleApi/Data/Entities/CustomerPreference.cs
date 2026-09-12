namespace DbRouter.SampleApi.Data.Entities;

public sealed class CustomerPreference
{
    public int Id { get; set; }

    public int CustomerId { get; set; }

    public required string Preference { get; set; }
}
