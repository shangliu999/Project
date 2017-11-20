using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace ETexsys.Model
{
    public class C_AutoRFIDCollectReader
    {
        public string ReaderName { get; set; }

        public string ReaderCode { get; set; }
    }

    public class C_Index
    {
        public int BrandID { get; set; }
        public string BrandName { get; set; }
        public List<C_Index4> Index4 { get; set; }

        public List<C_Index2> Index2 { get; set; }

        public List<C_Index3> Index3 { get; set; }
    }
    public class C_Index2
    {
        public int Cid2 { get; set; }

        public int Cnum2 { get; set; }
    }
    public class C_Index3
    {
        public int Cid3 { get; set; }

        public int Cnum3 { get; set; }
    }
    public class C_Index4
    {
        public int Cid4 { get; set; }

        public string Cname4 { get; set; }

    }

    public class C_TextileSummary
    {
        public string SiteCode { get; set; }

        public int ClassID { get; set; }

        public string ClassName { get; set; }

        public int TextileCount { get; set; }
    }
    public class C_HzlkSummary
    {
        public int BrandID { get; set; }

        public string BrandName { get; set; }

        public int ds { get; set; }

        public int ds2 { get; set; }
    }
    public class C_FBlkSummary
    {

        public int ClassID { get; set; }
        public string BrandName { get; set; }
        public string ClassName { get; set; }
        public string FabricName { get; set; }
        public int num { get; set; }
        public int sums { get; set; }
        public float avgs { get; set; }
        public int ClassLeft { get; set; }
        //public List<FBshow> FbList { get; set; }
        // public List<FBdis> Fdis { get; set; }
    }
    public class FBdis
    {
        public int FID { get; set; }
        public string FName { get; set; }
    }
    public class FBshow
    {
        public int ClassID { get; set; }
        public string ClassName { get; set; }
        public string FabricName { get; set; }
        public string ClassLeft { get; set; }

        public int num { get; set; }

        public int sums { get; set; }
        public float avgs { get; set; }

        public List<FBshow2> N1 { get; set; }

        public int nums { get; set; }
        public int sum2s { get; set; }
    }
    public class FBshow2
    {
        public int SID { get; set; }
        public int SNum { get; set; }

    }

    //该类是用于定义属性节点的，包括属性的名称、属性的类型。
    public class C_PropertyItem
    {
        public C_PropertyItem(string name, Type type)
        {
            this.Name = name;
            this.Type = type;
        }
        public string Name { set; get; }
        public Type Type { set; get; }
    }
    //该类是提供接口，动态生成类型的。
    public class C_TextileTypeFactory
    {
        public static Type GetTextileType(List<C_PropertyItem> itemList)
        {
            TypeBuilder builder = CreateTypeBuilder(
                    "MyDynamicAssembly", "MyModule", "MyType");
            foreach (var item in itemList)
            {
                CreateAutoImplementedProperty(builder, item.Name, item.Type);
            }

            Type resultType = builder.CreateType();
            return resultType;
        }

        private static TypeBuilder CreateTypeBuilder(
            string assemblyName, string moduleName, string typeName)
        {
            TypeBuilder typeBuilder = AppDomain
                .CurrentDomain
                .DefineDynamicAssembly(new AssemblyName(assemblyName),
                                       AssemblyBuilderAccess.Run)
                .DefineDynamicModule(moduleName)
                .DefineType(typeName, TypeAttributes.Public);
            typeBuilder.DefineDefaultConstructor(MethodAttributes.Public);
            return typeBuilder;
        }

        private static void CreateAutoImplementedProperty(
            TypeBuilder builder, string propertyName, Type propertyType)
        {
            const string PrivateFieldPrefix = "m_";
            const string GetterPrefix = "get_";
            const string SetterPrefix = "set_";

            // 定义字段.
            FieldBuilder fieldBuilder = builder.DefineField(
                string.Concat(PrivateFieldPrefix, propertyName),
                              propertyType, FieldAttributes.Private);

            // 定义属性
            PropertyBuilder propertyBuilder = builder.DefineProperty(
                propertyName, System.Reflection.PropertyAttributes.HasDefault, propertyType, null);

            // 属性的getter和setter的特性
            MethodAttributes propertyMethodAttributes =
                MethodAttributes.Public | MethodAttributes.SpecialName |
                MethodAttributes.HideBySig;

            // 定义getter方法
            MethodBuilder getterMethod = builder.DefineMethod(
                string.Concat(GetterPrefix, propertyName),
                propertyMethodAttributes, propertyType, Type.EmptyTypes);

            ILGenerator getterILCode = getterMethod.GetILGenerator();
            getterILCode.Emit(OpCodes.Ldarg_0);
            getterILCode.Emit(OpCodes.Ldfld, fieldBuilder);
            getterILCode.Emit(OpCodes.Ret);

            // 定义setter方法
            MethodBuilder setterMethod = builder.DefineMethod(
                string.Concat(SetterPrefix, propertyName),
                propertyMethodAttributes, null, new Type[] { propertyType });


            ILGenerator setterILCode = setterMethod.GetILGenerator();
            setterILCode.Emit(OpCodes.Ldarg_0);
            setterILCode.Emit(OpCodes.Ldarg_1);
            setterILCode.Emit(OpCodes.Stfld, fieldBuilder);
            setterILCode.Emit(OpCodes.Ret);

            propertyBuilder.SetGetMethod(getterMethod);
            propertyBuilder.SetSetMethod(setterMethod);
        }
    }
}
