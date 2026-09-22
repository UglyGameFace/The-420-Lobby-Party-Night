using NUnit.Framework;
using PartyNight.Foundation.Editor;

namespace PartyNight.Foundation.Tests
{
    public sealed class GameplayFoundationEditorTests
    {
        [Test]
        public void CommittedFoundationSceneHasRuntimeCompositionRoot()
        {
            Assert.DoesNotThrow(GameplayFoundationValidator.Validate);
        }
    }
}
