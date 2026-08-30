namespace Rating.API.Models;

public class RatingCriteria
{
    public string CriteriaId { get; set; } = Guid.NewGuid().ToString();
    public string EntityType { get; set; } = "PRODUCT"; // PRODUCT, PLACE, SERVICE, DIGITAL, PUBLIC
    public string Name { get; set; } = string.Empty;
    public string IconName { get; set; } = "star";
    public double Weight { get; set; } = 1.0;
    public int DisplayOrder { get; set; } = 0;
}
