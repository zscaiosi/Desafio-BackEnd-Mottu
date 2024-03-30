namespace MottuRentalApp.Interface.Repositories
{
  public struct PatchRentalDto
  {
    public PatchRentalDto(string userId, DateTime endTerm, decimal totalFare)
    {
      UserId = userId;
      EndTerm = endTerm;
      TotalFare = totalFare;
    }

    public string UserId { get; }
    public DateTime EndTerm { get; }
    public decimal TotalFare { get; set; }
  }
}