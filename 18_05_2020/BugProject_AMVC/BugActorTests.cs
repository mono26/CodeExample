using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using BugProject.Actors;
using BugProject.Core.Actors;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace BugProject.Tests
{
    public class BugActorTests
    {
        [UnityTest]
        public IEnumerator _0_Get_Unique_Id_With_Two_Actors()
        {
            BugActor testActor1 = CreateTestActor();
            BugActor testActor2 = CreateTestActor();

            yield return null;

            Assert.AreNotEqual(testActor1.GetUniqueId, testActor2.GetUniqueId);
        }

        private BugActor CreateTestActor()
        {
            return new GameObject("test_actor").AddComponent<BugActor>();
        }

        [UnityTest]
        public IEnumerator _1_Get_One_Component_In_Map_With_Two_Subscriptions_Of_Same_Components()
        {
            BugActor testActor = CreateTestActor();
            testActor.AttachActorComponent<Movement>(Movement.GetUniqueId);
            testActor.AttachActorComponent<Movement>(Movement.GetUniqueId);

            yield return null;

            FieldInfo fieldInfo = testActor.GetType().GetField("componentsMap", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.AreEqual(1, ((Dictionary<string, BugActorComponent>)fieldInfo.GetValue(testActor)).Count);
        }

        [UnityTest]
        public IEnumerator _2_Get_Two_Component_In_Map_With_Two_Subscriptions_Of_Same_Components_And_One_Different()
        {
            BugActor testActor = CreateTestActor();
            testActor.AttachActorComponent<Movement>(Movement.GetUniqueId);
            testActor.AttachActorComponent<Movement>(Movement.GetUniqueId);
            testActor.AttachActorComponent<Jump>(Jump.GetUniqueId);

            yield return null;

            FieldInfo fieldInfo = testActor.GetType().GetField("componentsMap", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.AreEqual(2, ((Dictionary<string, BugActorComponent>)fieldInfo.GetValue(testActor)).Count);
        }
    }
}
