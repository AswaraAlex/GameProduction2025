using System;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Reshape.Unity;

namespace Reshape.ReFramework
{
    [Serializable]
    [HideMonoScript]
    public class SavePack : BaseScriptable
    {
        public const int TYPE_VAR = 1;
        public const int TYPE_INV = 2;
        public const int TYPE_UNITSTORAGE = 3;
        public const int TYPE_DATA = 4;
        
        protected const string SAVE_VAR = "var_";
        protected const string SAVE_INV = "inv_";
        protected const string SAVE_USTORE = "ustore_";
        protected const string SAVE_DATA = "data_";
        protected const string SAVE_DEFAULT = "d_";
        protected const string TAG_PREFIX = ".";
        protected const string NAME_SEPARATOR = "_";

        [DisableInPlayMode]
        [HideInInlineEditors]
        [InlineProperty]
        public StringProperty fileName;

        [Space(2)]
        [DisableInPlayMode]
        [HideInInlineEditors]
        [InlineProperty]
        public StringProperty password;
        
        [Space(2)]
        [DisableInPlayMode]
        [HideInInlineEditors]
        [InlineProperty]
        public StringProperty saveTag;
        
        protected string fileNamePostfix;
        protected bool printLog = true;

        //-----------------------------------------------------------------
        //-- static methods
        //-----------------------------------------------------------------

        public static string GetSaveFileName (string saveFile, string saveTag, int type, string namePostFix = "")
        {
            var name = string.Empty;
            if (type == TYPE_VAR)
                name += SAVE_VAR;
            else if (type == TYPE_INV)
                name += SAVE_INV;
            else if (type == TYPE_UNITSTORAGE)
                name += SAVE_USTORE;
            else if (type == TYPE_DATA)
                name += SAVE_DATA;
            else
                name += SAVE_DEFAULT;
            if (!string.IsNullOrEmpty(saveTag))
                name += TAG_PREFIX + saveTag + NAME_SEPARATOR + saveFile;
            else
                name += saveFile;
            if (!string.IsNullOrEmpty(namePostFix))
                name += NAME_SEPARATOR + namePostFix;
            return name;
        }

        //-----------------------------------------------------------------
        //-- public methods
        //-----------------------------------------------------------------

        //-----------------------------------------------------------------
        //-- protected methods
        //-----------------------------------------------------------------

        protected void SaveFile (Dictionary<string, object> dict, int type)
        {
            var op = ReSave.Save(GetSaveFileName(fileName, saveTag, type, fileNamePostfix), dict, password);
            if (!op.success && printLog)
                ReDebug.LogWarning("VariableSavePack", name + " not successfully save!");
        }
        
        protected Dictionary<string,object> LoadFile (int type)
        {
            var op = ReSave.Load(GetSaveFileName(fileName, saveTag, type, fileNamePostfix), password);
            if (op.success)
                return op.receivedDict;
            if (printLog)
                ReDebug.LogWarning("VariableSavePack", name + " not successfully load!");
            return null;
        }
        
        protected void DeleteFile (int type)
        {
            var op = ReSave.Delete(GetSaveFileName(fileName, saveTag, type, fileNamePostfix), password);
            if (!op.success && printLog)
                ReDebug.LogWarning("VariableSavePack", name + " not successfully delete!");
        }
        
        //-----------------------------------------------------------------
        //-- mono methods
        //-----------------------------------------------------------------

        //-----------------------------------------------------------------
        //-- BaseScriptable methods
        //-----------------------------------------------------------------

        //-----------------------------------------------------------------
        //-- private methods
        //-----------------------------------------------------------------

        //-----------------------------------------------------------------
        //-- editor methods
        //-----------------------------------------------------------------
    }
}