using JetBrains.ReSharper.Feature.Services.CodeCompletion.Infrastructure.LookupItems;
using JetBrains.ReSharper.FeaturesTestFramework.Completion;
using JetBrains.ReSharper.TestFramework;
using NUnit.Framework;

namespace ReSharperPlugin.ArgumentsLineComplete.Tests.test.Src
{
    [TestFixture]
    [TestNetFramework45]
    public class CSharpMultipleArgumentsTest : CodeCompletionTestBase
    {
        protected override CodeCompletionTestType TestType => CodeCompletionTestType.List;

        protected override string RelativeTestDataPath => "completion";

        protected override bool CheckAutomaticCompletionDefault() => true;

        [Test] public void TestBasicList1() => DoNamedTest();
        [Test] public void TestBasicList2() => DoNamedTest();
        [Test] public void TestBasicList3() => DoNamedTest();
        [Test] public void TestCorrelationWithBase1() => DoNamedTest();
        [Test] public void TestCorrelationWithBase2() => DoNamedTest();
        [Test] public void TestCorrelationWithBase3() => DoNamedTest();
        [Test] public void TestObjectInitializer1() => DoNamedTest();
        [Test] public void TestObjectInitializer2() => DoNamedTest();
        [Test] public void TestIndexer1() => DoNamedTest();
        [Test] public void TestIndexer2() => DoNamedTest();
    }

    [TestFixture]
    public class CSharpMultipleArgumentsRelevanceTest : CSharpMultipleArgumentsTest
    {
        protected override LookupListSorting Sorting => LookupListSorting.ByRelevance;

        protected override string GetGoldTestDataPath(string fileName)
        {
            return ApplyTestDataPathSuffix(base.GetGoldTestDataPath(fileName), GoldFileNameSuffix, ".relevance");
        }
    }
}