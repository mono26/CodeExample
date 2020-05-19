using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using BugProject.Core.EventSystem;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace BugProject.Tests
{
    public class BugEventControllerTests
    {
        private const string TEST_EVENT_ZERO = "tests.testEvent_0";

        public class SubscribeToEventFunction
        {
            [UnityTest]
            public IEnumerator _0_Get_NonZeroEvents_Event_Map_With_OneSubscription()
            {
                BugEventController testObject = CreateTestObject();

                yield return null;

                testObject.SubscribeToEvent(TEST_EVENT_ZERO, (args) => { });
                FieldInfo fieldInfo = testObject.GetType().GetField("eventsMap", BindingFlags.Instance | BindingFlags.NonPublic);
                Assert.AreNotEqual(0, ((Dictionary<string, List<Action<IBugEventArgs>>>)fieldInfo.GetValue(testObject)).Count);
            }

            private BugEventController CreateTestObject()
            {
                return new GameObject("Test_Bug_Event_Controller").AddComponent<BugEventController>() as BugEventController;
            }

            [UnityTest]
            public IEnumerator _1_Get_OneEvent_Event_Map_With_OneSubscription()
            {
                BugEventController testObject = CreateTestObject();

                yield return null;

                testObject.SubscribeToEvent("tests.testEvent_0", (args) => { });
                FieldInfo fieldInfo = testObject.GetType().GetField("eventsMap", BindingFlags.Instance | BindingFlags.NonPublic);
                Assert.AreEqual(1, ((Dictionary<string, List<Action<IBugEventArgs>>>)fieldInfo.GetValue(testObject)).Count);
            }

            [UnityTest]
            public IEnumerator _2_Get_One_Subscriber_List_With_ThreeSubscriptions_Of_Same_Function()
            {
                BugEventController testObject = CreateTestObject();

                yield return null;

                for (int i = 1; i <= 3; i++)
                {
                    testObject.SubscribeToEvent("tests.testEvent_0", (args) => { });
                }

                FieldInfo fieldInfo = testObject.GetType().GetField("eventsMap", BindingFlags.Instance | BindingFlags.NonPublic);
                Assert.AreEqual(1, ((Dictionary<string, List<Action<IBugEventArgs>>>)fieldInfo.GetValue(testObject))[TEST_EVENT_ZERO].Count);
            }

            [UnityTest]
            public IEnumerator _3_Get_Three_Subscriber_List_With_ThreeDifferentAnonymousSubscriptions()
            {
                BugEventController testObject = CreateTestObject();

                yield return null;

                Action<IBugEventArgs> function1 = (args) => { };
                testObject.SubscribeToEvent("tests.testEvent_0", function1);

                Action<IBugEventArgs> function2 = (args) => { };
                testObject.SubscribeToEvent("tests.testEvent_0", function2);

                Action<IBugEventArgs> function3 = (args) => { };
                testObject.SubscribeToEvent("tests.testEvent_0", function3);

                FieldInfo fieldInfo = testObject.GetType().GetField("eventsMap", BindingFlags.Instance | BindingFlags.NonPublic);
                Assert.AreEqual(3, ((Dictionary<string, List<Action<IBugEventArgs>>>)fieldInfo.GetValue(testObject))[TEST_EVENT_ZERO].Count);
            }

            [UnityTest]
            public IEnumerator _4_Get_ZeroEvents_Subscriber_List_With_OneSubscription_And_OneUnSubscription()
            {
                BugEventController testObject = CreateTestObject();

                yield return null;

                Action<IBugEventArgs> function = args => { };

                testObject.SubscribeToEvent(TEST_EVENT_ZERO, function);
                testObject.UnSubscribeFromEvent(TEST_EVENT_ZERO, function);

                FieldInfo fieldInfo = testObject.GetType().GetField("eventsMap", BindingFlags.Instance | BindingFlags.NonPublic);
                Assert.AreEqual(0, ((Dictionary<string, List<Action<IBugEventArgs>>>)fieldInfo.GetValue(testObject))[TEST_EVENT_ZERO].Count);
            }

            [UnityTest]
            public IEnumerator _5_Get_Two_Subscriber_List_With_ThreeDifferentAnonymousSubscriptions_And_OneUnSubscription()
            {
                BugEventController testObject = CreateTestObject();

                yield return null;

                Action<IBugEventArgs> function1 = (args) => { };
                testObject.SubscribeToEvent("tests.testEvent_0", function1);

                Action<IBugEventArgs> function2 = (args) => { };
                testObject.SubscribeToEvent("tests.testEvent_0", function2);

                Action<IBugEventArgs> function3 = (args) => { };
                testObject.SubscribeToEvent("tests.testEvent_0", function3);

                testObject.UnSubscribeFromEvent(TEST_EVENT_ZERO, function1);

                FieldInfo fieldInfo = testObject.GetType().GetField("eventsMap", BindingFlags.Instance | BindingFlags.NonPublic);
                Assert.AreEqual(2, ((Dictionary<string, List<Action<IBugEventArgs>>>)fieldInfo.GetValue(testObject))[TEST_EVENT_ZERO].Count);
            }
        }
    }
}
