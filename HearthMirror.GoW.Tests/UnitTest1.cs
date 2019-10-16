using System.Linq;
using NUnit.Framework;

namespace HearthMirror.GoW.Tests
{
    public class UnitTest1
    {
        [Test]
        public void TestMethod1()
        {
            var sys = new Gems().GetSuperSystem();
            Assert.IsNotNull(sys);
        }

        [Test]
        public void GetBoard()
        {
            var state = new Gems().GetGameState();
            Assert.NotNull(state);
            Assert.AreEqual(64, state.Board.SelectMany(c => c).Count());
        }
    }
}
