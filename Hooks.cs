﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TechTalk.SpecFlow;
using System.Configuration;
using System.Diagnostics;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
using System.Collections.Specialized;
using NUnit.Framework;
using TechTalk.SpecFlow.Tracing;
using  System.IO;
using System.Reflection;

namespace SpecFlowLambdaSample
{
    [Binding]
    public sealed class Hooks
    {
        
        private LambdaTestDriver LTDriver;
        private string[] tags;
        private ScenarioContext _scenarioContext;

        [BeforeScenario]
        public void BeforeScenario(ScenarioContext ScenarioContext)
        {
            _scenarioContext = ScenarioContext;
            LTDriver = new LambdaTestDriver(ScenarioContext);
            ScenarioContext["LTDriver"] = LTDriver;
        }



        [AfterScenario]
        public void AfterScenario()
        {
            LTDriver.Cleanup();
        }
    }


    public class LambdaTestDriver
    {
        private IWebDriver driver;
       
        private string profile;
        private string environment;
        private ScenarioContext ScenarioContext;
        

        public LambdaTestDriver(ScenarioContext ScenarioContext)
        {
            this.ScenarioContext = ScenarioContext;
        }

       
        public IWebDriver Init(string profile, string environment)
        {
            
            NameValueCollection caps = ConfigurationManager.GetSection("capabilities/" + profile) as NameValueCollection;
            NameValueCollection settings = ConfigurationManager.GetSection("environments/" + environment) as NameValueCollection;
            Console.WriteLine(caps);
            DesiredCapabilities capability = new DesiredCapabilities();

            Console.WriteLine(capability);
            Console.WriteLine(profile+environment);

            foreach (string key in caps.AllKeys)
            {
                capability.SetCapability(key, caps[key]);
            }

            foreach (string key in settings.AllKeys)
            {
                capability.SetCapability(key, settings[key]);
            }

            String username = Environment.GetEnvironmentVariable("LT_USERNAME");
            if (username == null)
            {
                username =  ConfigurationManager.AppSettings.Get("username");
            }

            String accesskey = Environment.GetEnvironmentVariable("LT_ACCESS_KEY");
            if (accesskey == null)
            {
                accesskey = ConfigurationManager.AppSettings.Get("accesskey");
            }

            capability.SetCapability("username", username);
            capability.SetCapability("accesskey", accesskey);
            Console.WriteLine(username);
            Console.WriteLine(accesskey);


            driver = new RemoteWebDriver(new Uri("http://"+username+":"+accesskey + ConfigurationManager.AppSettings.Get("server") + "/wd/hub/"), capability);
            Console.WriteLine(driver);
            return driver;
        }

        public void Cleanup()
        {

            //passingthehooks
            bool passed = TestContext.CurrentContext.Result.Outcome.Status == NUnit.Framework.Interfaces.TestStatus.Passed;

            var status = passed ? "passed" : "failed";

            ((IJavaScriptExecutor)driver).ExecuteScript($"lambda-status={status}");
            // Terminates the remote webdriver session
            driver.Quit();
            
            
            
        }
    }
}
