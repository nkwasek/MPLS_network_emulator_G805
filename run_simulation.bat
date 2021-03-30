start "ManagementCenter" .\ManagementCenter\ManagementCenter\bin\debug\ManagementCenter.exe "Configs/ManagementCenterConfig.xml"
start "LSR1" .\LabelSwitchingRouter\LabelSwitchingRouter\bin\debug\LabelSwitchingRouter.exe "Configs/LSR1Config.xml"
timeout 0.5
start "LSR2" .\LabelSwitchingRouter\LabelSwitchingRouter\bin\debug\LabelSwitchingRouter.exe "Configs/LSR2Config.xml"
timeout 0.5
start "LSR3" .\LabelSwitchingRouter\LabelSwitchingRouter\bin\debug\LabelSwitchingRouter.exe "Configs/LSR3Config.xml"
timeout 0.5
start "LSR4" .\LabelSwitchingRouter\LabelSwitchingRouter\bin\debug\LabelSwitchingRouter.exe "Configs/LSR4Config.xml"
timeout 0.5
start "LSR5" .\LabelSwitchingRouter\LabelSwitchingRouter\bin\debug\LabelSwitchingRouter.exe "Configs/LSR5Config.xml"
timeout 2
start "CableCloud" .\CableCloud\CableCloud\bin\debug\CableCloud.exe "Configs/CableCloudConfig.xml"
start "Host01" .\Host\Host\bin\debug\Host.exe "Configs/H01Config.xml"
timeout 0.5
start "Host02" .\Host\Host\bin\debug\Host.exe "Configs/H02Config.xml"
timeout 0.5
start "Host03" .\Host\Host\bin\debug\Host.exe "Configs/H03Config.xml"