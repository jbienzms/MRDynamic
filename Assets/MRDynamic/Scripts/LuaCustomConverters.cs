/// Special thanks to <see href="https://gist.github.com/marcinjackowiak">marcinjackowiak</see>
/// for his sample file which this one was based on. His version can be found <see href="https://gist.github.com/marcinjackowiak/24e01314ce9956487515dcb328fa0877">here</see>.
using UnityEngine;
using MoonSharp.Interpreter;
using System;

/// <summary>
/// These are custom converters for Vector2 and Vector3 structs.
///
/// Add the following call to your code:
/// LuaCustomConverters.RegisterAll();
///
/// To create Vector2 in lua:
/// position = {1.0, 1.0}
///
/// To Vector3 in lua:
/// position = {1.0, 1.0, 1.0}
///
/// </summary>
public static class LuaCustomConverters
{

    public static void RegisterAll()
    {
        // Vector 2

        Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(DataType.Table, typeof(Vector2), dynVal =>
        {
            Table table = dynVal.Table;
            float x = (float)((Double)table["x"]);
            float y = (float)((Double)table["y"]);
            return new Vector2(x, y);
        });

        Script.GlobalOptions.CustomConverters.SetClrToScriptCustomConversion<Vector2>((script, vector) =>
        {
            DynValue x = DynValue.NewNumber((double)vector.x);
            DynValue y = DynValue.NewNumber((double)vector.y);
            Table table = new Table(script);
            table.Set("x", x);
            table.Set("y", y);
            return DynValue.NewTable(table);
        });

        // Vector3

        Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(DataType.Table, typeof(Vector3), dynVal =>
        {
            Table table = dynVal.Table;
            float x = (float)((Double)table["x"]);
            float y = (float)((Double)table["y"]);
            float z = (float)((Double)table["z"]);
            return new Vector3(x, y, z);
        });

        Script.GlobalOptions.CustomConverters.SetClrToScriptCustomConversion<Vector3>((script, vector) =>
        {
            DynValue x = DynValue.NewNumber((double)vector.x);
            DynValue y = DynValue.NewNumber((double)vector.y);
            DynValue z = DynValue.NewNumber((double)vector.z);
            Table table = new Table(script);
            table.Set("x", x);
            table.Set("y", y);
            table.Set("z", z);
            return DynValue.NewTable(table);
        });
    }
}