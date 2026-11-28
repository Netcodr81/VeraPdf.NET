using VeraPdf.Library.Components;
using ComponentFactory = VeraPdf.Library.Components.Components;

namespace VeraPdf.Library.Tests;

public class ComponentsTests
{
    [Fact]
    public void ExposesDefaultDetailsAndDuration()
    {
        Assert.Same(ComponentDetails.Default, ComponentFactory.DefaultDetails);
        Assert.Same(AuditDuration.Default, ComponentFactory.DefaultDuration);
    }

    [Fact]
    public void CreatesVeraPdfAndCallerSpecifiedDetails()
    {
        var id = new Uri("urn:example:component");

        var veraDetails = ComponentFactory.VeraDetails(id, "validator", "1.0.0", "PDF validator");
        var customDetails = ComponentFactory.DetailsFromValues(id, "parser", "2.0.0", "provider", "PDF parser");

        Assert.Equal("The veraPDF Consortium.", veraDetails.Provider);
        Assert.Equal("PDF validator", veraDetails.Description);
        Assert.Equal("provider", customDetails.Provider);
        Assert.Throws<ArgumentNullException>(() =>
            ComponentFactory.DetailsFromValues(null!, "parser", "2.0.0", "provider", "PDF parser"));
        Assert.Throws<ArgumentException>(() =>
            ComponentFactory.DetailsFromValues(id, "", "2.0.0", "provider", "PDF parser"));
    }

    [Fact]
    public void TimerProducesAValidAuditDuration()
    {
        var duration = ComponentFactory.Timer.Start().Stop();

        Assert.True(duration.Start >= 0);
        Assert.True(duration.Finish >= duration.Start);
    }
}
