using NUnit.Framework;
using PartyNight.Foundation.Editor;

namespace PartyNight.Foundation.Tests
{
    public sealed class ProjectSettingsTests
    {
        [Test]
        public void AuthoritativeUnitySettingsValidate()
        {
            Assert.DoesNotThrow(ProjectFoundationValidator.Validate);
        }
    }
}
