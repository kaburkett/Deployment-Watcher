/*
* Install
*/
To install, build release and copy /bin/release contents to C:\DeploymentWatcher

Run cmd as admin, paste following command to install the service:
C:\Windows\Microsoft.Net\Framework64\v4.0.30319\installutil C:\DeploymentWatcher\DeploymentWatcher.exe

Open windows services and start the DeploymentWatcher service.


/*
* Uninstall
*/
Run cmd as admin, paste following command to uninstall:
C:\Windows\Microsoft.Net\Framework64\v4.0.30319\installutil /u C:\DeploymentWatcher\DeploymentWatcher.exe