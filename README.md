# Niam.XRM.AssemblyReduce
Reduce the size of Dynamics CRM plugin assembly by removing unused methods and properties! Starting from plugin type (IPlugin) as entry point, system will scanning all the types and check for unsed to be removed in the result dll.

## How to use
1. Copy all the references dll in the same folder (input folder).
1. Open Command Prompt, change directory to the exe file (Niam.Xrm.AssemblyReduce.exe).
1. Run this code in the command prompt:
```
Niam.Xrm.AssemblyReduce.exe -i="input\LeadPlugin.dll" -o="output\LeadPlugin.dll" -snk="input\key.snk" 
[-k="namespacedll1.*, namespacedll2.subnamespace"]
```
Let the program run and check the result in the output folder.

| Param         | Description                   | Example Value       |
| ------------- |:-----------------------------:|-------------------|
| -i            | The dll input path     | "input\LeadPlugin.dll" |
| -o            | The dll result path      |   "output\LeadPlugin.dll"|
| -k | The namespace you want to keep (optional)     |    "Niam.Xrm.Framework.* , SimpleJson" |

For the -k param, these param will help you to keep this namespace. For example if you put Niam.Xrm.Framework.* this param will giving flag to the system to keep all the types inside the namespace of Niam.Xrm.Framework.
