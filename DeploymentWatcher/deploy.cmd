rem ***************************************************************************
rem ********************  SET DEPLOYMENT VARS *********************************
rem ***************************************************************************

SET workingDir=%~dp0
SET deploymentDir="C:\DeploymentWatcher"
SET deploymentSite="C:\DeploymentPath"
SET serviceExeLocation="C:\DeploymentWatcher\DeploymentWatcher.exe"
SET serviceName="DeploymentWatcher"

rem ***************************************************************************
rem ********************  DO ALL THE DEPLOYMENTS ******************************
rem ***************************************************************************

cd %windir%\system32\inetsrv

net stop %serviceName%

C:\Windows\Microsoft.Net\Framework64\v4.0.30319\installutil /u %serviceExeLocation%

rmdir -r %deploymentDir%

mkdir %deploymentDir%

echo f | xcopy "%workingDir%contents\*" "%deploymentDir%\" /r /y

rmdir -r %deploymentSite%

mkdir %deploymentSite%

C:\Windows\Microsoft.Net\Framework64\v4.0.30319\installutil %serviceExeLocation%

net start %serviceName%

rem ***************************************************************************
rem ********************  LIST CONFIGURATIONS *********************************
rem ***************************************************************************

sc qc %serviceName%