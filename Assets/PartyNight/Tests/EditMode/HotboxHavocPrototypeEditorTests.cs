using NUnit.Framework;
using PartyNight.Foundation.Editor;

namespace PartyNight.Foundation.Tests
{
    public sealed class HotboxHavocPrototypeEditorTests
    {
        [Test]
        public void PrototypeConfigurationPassesEditorValidation()
        {
            Assert.DoesNotThrow(HotboxHavocPrototypeValidator.Validate);
        }
    }
}
