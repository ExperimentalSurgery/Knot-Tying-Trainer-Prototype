using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.Callbacks;
using UnityEngine;

namespace DFKI.NMY
{
    public class SerialManagerBuildPostProcessHelper : IPostprocessBuildWithReport
    {
	    
         public void OnPostprocessBuild(BuildReport report){
             Debug.Log("...Manually copying SerialInfoManager Directory to build path");
             
             string p1 = Application.dataPath;
             p1 = p1.Replace("/Assets", "");
             
             string sourceDir = p1 +"/SerialPortInfo";
             string targetDir = report.summary.outputPath + "/SerialPortInfo";

             // Exclude exe from path
             targetDir = targetDir.Replace(Application.productName + ".exe", "");
             FileUtil.CopyFileOrDirectory(sourceDir, targetDir);
         }
        
         public int callbackOrder { get; }
        
    }
}
