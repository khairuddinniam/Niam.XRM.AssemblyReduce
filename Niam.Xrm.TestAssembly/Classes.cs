using System;
using Microsoft.Xrm.Sdk;

namespace Niam.Xrm.TestAssembly
{
    public class UnusedClass { }

    interface UsedAsFieldInterface { }

    interface UsedAsClassInterfaceInterface { }

    struct UsedAsPropertyStruct { }

    class UsedAsPropertySetClass { }

    class UsedAsPropertyGetClass { }

    class UsedAsMethodReturnTypeClass { }

    class UsedAsMethodParamClass { }

    class UsedAsGenericMethodParamClass { }

    class UsedAsStaticGenericMethodParamClass { }

    class UsedAsMethodVariableClass { }

    class UsedInsideMethodClass
    {
        public static void SomeMethod() { }
    }

    class UsedAsGenericMethodParamClass2 { }

    class UsedInsideMethodClass2
    {
        public static void GenericMethod<T>() { }
    }

    public class UsedAsGenericParamClass : Entity { }

    public class DirectImplementPlugin : IPlugin
    {
        public string Text { get; set; }

        public void Execute(IServiceProvider serviceProvider)
        {
            throw new NotImplementedException();
        }

        public void SomeMethod()
        {
            UsedAsMethodVariableClass name = null;
        }

        void Hello()
        {
            UsedInsideMethodClass.SomeMethod();
            UsedInsideMethodClass2.GenericMethod<UsedAsGenericMethodParamClass2>();
        }
    }

    public abstract class PluginBase : IPlugin, UsedAsClassInterfaceInterface
    {
        private readonly UsedAsFieldInterface _usedAsFieldInterface;

        public void Execute(IServiceProvider serviceProvider)
        {
            throw new NotImplementedException();
        }

        UsedAsMethodReturnTypeClass First() => null;

        void Second(UsedAsMethodParamClass arg) { }

        void Generic<T>() { }

        void UseGeneric()
        {
            Generic<UsedAsGenericMethodParamClass>();
        }

        static void StaticGeneric<T>() { }

        static void UseStaticGeneric()
        {
            StaticGeneric<UsedAsStaticGenericMethodParamClass>();
        }
    }

    public class UsingCustomBasePlugin : PluginBase
    {
    }

    public abstract class PluginBaseGeneric<T> : IPlugin
    {
        internal UsedAsPropertyStruct StructProperty { get; set; }

        internal UsedAsPropertyGetClass GetProperty
        {
            get
            {
                return null;
            }
        }

        internal UsedAsPropertySetClass SetProperty
        {
            set { }
        }

        public void Execute(IServiceProvider serviceProvider)
        {
            throw new NotImplementedException();
        }
    }

    public class UsingCustomBaseGenericPlugin : PluginBaseGeneric<UsedAsGenericParamClass>
    {
    }
}
