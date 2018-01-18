using System;
using Microsoft.Xrm.Sdk;
using Niam.Xrm.TestAssembly;
using System.Collections.Generic;

[assembly: UsedAsAssemblyAttribute]

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

    class UsedAsOperandClass { }

    class UsedAsGenericParamConstraintClass { }

    class UsedAsGenericParamReturnTypeMethodClass { }

    class UsedAsGenericParamFieldTypeMethodClass { }

    public class UsedAsGenericParamConstraintClass2 { }

    public class UsedAsGenericParamClass : Entity { }

    public class UsedAsClassAttribute : Attribute { }

    public class UsedAsFieldAttribute : Attribute { }

    public class UsedAsPropertyAttribute : Attribute { }

    public class UsedAsMethodAttribute : Attribute { }

    public class UsedAsMethodParamAttribute : Attribute { }

    public class UsedInsideAssemblyAttribute { }

    [AttributeUsage(AttributeTargets.Assembly)]
    public class UsedAsAssemblyAttribute : Attribute
    {
        UsedInsideAssemblyAttribute _field1 = null;
    }

    [UsedAsClassAttribute]
    public class DirectImplementPlugin : IPlugin
    {
        [UsedAsPropertyAttribute]
        public string Text { get; set; }

        public void Execute(IServiceProvider serviceProvider)
        {
            throw new NotImplementedException();
        }

        public void SomeMethod()
        {
            UsedAsMethodVariableClass var1 = null;
            object var2 = (UsedAsOperandClass) (object) 123;
        }

        void Hello()
        {
            UsedInsideMethodClass.SomeMethod();
            UsedInsideMethodClass2.GenericMethod<UsedAsGenericMethodParamClass2>();
        }
    }

    public abstract class PluginBase : IPlugin, UsedAsClassInterfaceInterface
    {
        [UsedAsFieldAttribute]
        private readonly UsedAsFieldInterface _usedAsFieldInterface;

        private List<UsedAsGenericParamFieldTypeMethodClass> _list;

        [UsedAsMethodAttribute]
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

        public void Execute([UsedAsMethodParamAttribute] IServiceProvider serviceProvider)
        {
            throw new NotImplementedException();
        }
    }

    public class GenericPlugin<T> : IPlugin
        where T : UsedAsGenericParamConstraintClass2
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            throw new NotImplementedException();
        }
    }

    public class UsingCustomBaseGenericPlugin : PluginBaseGeneric<UsedAsGenericParamClass>
    {
        void SomeMethod<T>() where T : UsedAsGenericParamConstraintClass
        {
        }

        List<UsedAsGenericParamReturnTypeMethodClass> ReturnGenericMethod() => null;
    }
}
