using NUnit.Framework;

namespace PartyNight.Foundation.Tests
{
    public sealed class ProjectIdentityTests
    {
        [Test]
        public void ProductDefinitionMatchesVerticalSlice()
        {
            Assert.That(ProjectIdentity.ProductName, Is.EqualTo("The 420 Lobby: Party Night"));
            Assert.That(ProjectIdentity.InitialMatchMinPlayers, Is.EqualTo(12));
            Assert.That(ProjectIdentity.InitialMatchMaxPlayers, Is.EqualTo(16));
            Assert.That(ProjectIdentity.InitialMatchMaxPlayers, Is.GreaterThan(ProjectIdentity.InitialMatchMinPlayers));
        }
    }
}
