namespace qplix_portfolio_management.Application.Services.Cities.Dtos;

public class GetCityResponseDto
{
    public int CityId { get; set; }

    public string CityCode { get; set; } = null!;

    public string? CityName { get; set; }
}