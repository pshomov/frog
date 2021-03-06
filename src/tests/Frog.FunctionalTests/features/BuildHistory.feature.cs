﻿// ------------------------------------------------------------------------------
//  <auto-generated>
//      This code was generated by SpecFlow (http://www.specflow.org/).
//      SpecFlow Version:1.7.1.0
//      SpecFlow Generator Version:1.7.0.0
//      Runtime Version:4.0.30319.1
// 
//      Changes to this file may cause incorrect behavior and will be lost if
//      the code is regenerated.
//  </auto-generated>
// ------------------------------------------------------------------------------
#region Designer generated code
namespace Frog.FunctionalTests.Features
{
    using TechTalk.SpecFlow;
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("TechTalk.SpecFlow", "1.7.1.0")]
    [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [NUnit.Framework.TestFixtureAttribute()]
    [NUnit.Framework.DescriptionAttribute("Build history")]
    public partial class BuildHistoryFeature
    {
        
        private static TechTalk.SpecFlow.ITestRunner testRunner;
        
#line 1 "BuildHistory.feature"
#line hidden
        
        [NUnit.Framework.TestFixtureSetUpAttribute()]
        public virtual void FeatureSetup()
        {
            testRunner = TechTalk.SpecFlow.TestRunnerManager.GetTestRunner();
            TechTalk.SpecFlow.FeatureInfo featureInfo = new TechTalk.SpecFlow.FeatureInfo(new System.Globalization.CultureInfo("en-US"), "Build history", "In order to get a feeling for how my project has built recent;ly\r\nAs a project ma" +
                    "intainer\r\nI want to be able to see the overall build status of the last few buil" +
                    "ds of my project at a glance", ProgrammingLanguage.CSharp, ((string[])(null)));
            testRunner.OnFeatureStart(featureInfo);
        }
        
        [NUnit.Framework.TestFixtureTearDownAttribute()]
        public virtual void FeatureTearDown()
        {
            testRunner.OnFeatureEnd();
            testRunner = null;
        }
        
        [NUnit.Framework.SetUpAttribute()]
        public virtual void TestInitialize()
        {
        }
        
        [NUnit.Framework.TearDownAttribute()]
        public virtual void ScenarioTearDown()
        {
            testRunner.OnScenarioEnd();
        }
        
        public virtual void ScenarioSetup(TechTalk.SpecFlow.ScenarioInfo scenarioInfo)
        {
            testRunner.OnScenarioStart(scenarioInfo);
            this.FeatureBackground();
        }
        
        public virtual void ScenarioCleanup()
        {
            testRunner.CollectScenarioErrors();
        }
        
        public virtual void FeatureBackground()
        {
#line 6
#line 7
 testRunner.Given("I have a project as \"p1\"");
#line 8
 testRunner.And("I add a test task \"t1\" with comment \"commit 1\" and content \"testoutput\" to projec" +
                    "t \"p1\"");
#line hidden
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("See the last 2 builds at a glance with the latest one on top")]
        public virtual void SeeTheLast2BuildsAtAGlanceWithTheLatestOneOnTop()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("See the last 2 builds at a glance with the latest one on top", ((string[])(null)));
#line 10
this.ScenarioSetup(scenarioInfo);
#line 11
 testRunner.Given("I have registered project \"p1\"");
#line 12
 testRunner.When("I check for updates");
#line 13
 testRunner.And("I am on the status page for project \"p1\"");
#line 14
 testRunner.And("I see the build is completed with status SUCCESS");
#line 15
 testRunner.And("I add a test task \"t2\" with comment \"commit 2\" and content \"testoutput\" to projec" +
                    "t \"p1\"");
#line 16
 testRunner.And("I check for updates");
#line 17
 testRunner.And("I am on the status page for project \"p1\"");
#line 18
 testRunner.Then("I see the build is completed with status SUCCESS");
#line 19
 testRunner.And("I see build history contains 2 items");
#line 20
 testRunner.And("I see build history item 1 contains \"commit 2\"");
#line 21
 testRunner.And("I see build history item 2 contains \"commit 1\"");
#line hidden
            this.ScenarioCleanup();
        }
    }
}
#endregion
