using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public static class GameViewSetup
{
    public static string Wide(){return Set(1440,900);}
    public static string Portrait(){return Set(540,960);}
    static string Set(int width,int height)
    {
        var asm=typeof(UnityEditor.Editor).Assembly;
        var sizesType=asm.GetType("UnityEditor.GameViewSizes");
        var single=typeof(ScriptableSingleton<>).MakeGenericType(sizesType);
        var sizes=single.GetProperty("instance",BindingFlags.Public|BindingFlags.Static|BindingFlags.FlattenHierarchy).GetValue(null);
        var group=sizesType.GetMethod("GetGroup").Invoke(sizes,new object[]{GameViewSizeGroupType.Standalone});
        var groupType=group.GetType();
        var sizeType=asm.GetType("UnityEditor.GameViewSize");
        var modeType=asm.GetType("UnityEditor.GameViewSizeType");
        string label="PhoViet "+width+"x"+height;
        string[] texts=(string[])groupType.GetMethod("GetDisplayTexts").Invoke(group,null);
        int selected=-1;
        for(int i=0;i<texts.Length;i++)if(texts[i].Contains(label)){selected=i;break;}
        if(selected<0)
        {
            var ctor=sizeType.GetConstructor(new[]{modeType,typeof(int),typeof(int),typeof(string)});
            var size=ctor.Invoke(new[]{Enum.ToObject(modeType,1),(object)width,height,label});
            groupType.GetMethod("AddCustomSize").Invoke(group,new[]{size});selected=texts.Length;
        }
        var viewType=asm.GetType("UnityEditor.GameView");var view=EditorWindow.GetWindow(viewType);
        viewType.GetProperty("selectedSizeIndex",BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic).SetValue(view,selected);
        view.Show();view.Focus();view.Repaint();
        return label;
    }
}
