# Niam.XRM.AssemblyReduce
Reduce the size of Dynamics CRM plugin assembly by removing unused types, methods and properties! Starting from plugin types as entry point.
In Dynamic CRM.

## How to use
1. Copy all the references dll in the same folder (input folder).
1. Open Command Prompt, change directory to the exe file (Niam.Xrm.AssemblyReduce.exe).
1. Run this code in the command prompt:
```
Niam.Xrm.AssemblyReduce.exe -i="input\LeadPlugin.dll" -o="output\LeadPlugin.dll" -snk="input\key.snk"
```

Let the program run and check the result in the output folder.
