# Deployment-Watcher
Use C# file watcher to run deployment command scripts and write output logs. Mimics basic SCCM deployment of cmd scripts and packages. Built to work with VS web packages.

## Purpose
DeploymentWatcher is a service that monitors a directory C:\DeploymentPath and runs newly added .cmd scripts. 

## Example Use Case
1. Install Service On Web Server
2. Share C:\DeploymentPath to networked computers
3. Create web deployment package in Visual Studio
4. Copy package to DeploymentPath
5. The service installs package by running .cmd script

## Installation Instructions
* Clone repository
* Copy install folder to desired machine
* Run deploy.cmd as administrator
* Done!

## How to use
Add .cmd scripts to C:\DeploymentPath and wait for a DeploymentLog.txt to appear with deployment results.
