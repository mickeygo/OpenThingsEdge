# ThingsEdge
IoT 边缘路由网关。

# 数据 API

## Channel 属性

 属性名 		| 说明
:---------------|:--------
ChannelId		|全局唯一值
Name			|通道名称
Keynote			|通道要旨，可用于设置重要信息
Devices			|设备集合

## Device 属性

 属性名 		| 说明
:---------------|:--------
DeviceId		|全局唯一值
Name			|设备名称
Model			|设备驱动型号，如 S7-200、S7-1200、S7-1500 等
Host			|服务器地址
Port			|端口，不为 0 时表示使用该端口
Keynote			|设备要旨，可用于设置重要信息
TagGroups		|标记组集合
Tags			|隶属于设备的标记集合


## TagGroup 属性

 属性名 		| 说明
:---------------|:--------
TagGroupId		|全局唯一值
Name			|标记组名
Keynote			|标记要旨，可用于设置重要信息
Tags			|隶属于分组的标记集合


## Tag 属性

 属性名 		| 说明
:---------------|:--------
TagId			|全局唯一值
Name			|标记名称
Address			|地址 (字符串格式)
Length			|数据长度。注：普通类型默认长度设置为 0，当为数组或字符串时，需指定长度。
DataType		|数据类型，分 "Bit"、"Byte"、"Word"、"DWord"、"Int"、"DInt"、"Real"、"LReal"、"String"、"S7String"、"S7WString"
ClientAccess	|客户端访问模式，默认可读写。
ScanRate		|扫描速率（毫秒），默认100ms。
Flag			|标记标识，分 "Heartbeat"、"Trigger"、"Notice"、"Switch"，其中 "Normal" 做为 "Trigger" 和 "Switch" 子标记。
PublishMode		|是否每次扫描后推送数据，为 true 时表示只有在数据有变化的情况下才会推送数据，默认为 "OnlyDataChanged"。注：仅适用 <see cref="TagFlag.Notice"/> 标记
Keynote			|标记要旨，可用于设置重要信息
DisplayName		|标记显示名称
Description		|标记说明
Identity		|标记身份标识，默认为 "Master"
CurveUsage		|曲线用途分类
Group			|标记分组标识，可用于定义将多个标记数据归为同一组，为空表示不进行分组。注：分组中的数据类型要保持一致，如果是数组，组内各标记数据类型也应都为数组，且长度一致。
ValueUsage		|标记值的用途标识
NormalTags		|只有 "Trigger" 类型的标记集合，在该标记触发时集合中的标记数据也同时一起随着推送

## Tag 方法
 属性名 		| 说明 							
:---------------|:--------					
IsArray			| 判断标记是否为数组对象。	

## JSON 选项配置示例
```JSON
[
  {
    "Name": "Line01", "Keynote": "",
    "Devices": [
      "Name": "设备01", "Model": "ModbusTcp", "Host": "127.0.0.1", "Port": 0, "Keynote": "",
      "TagGroups": [
        "Name": "OP101", "Keynote": "",
        "Tags": [
          {
            "Name": "PLC_Entry_Sign", "Address": "s=1;x=3;20", "Length": 0, "DataType": "Int", "ScanRate": 500, "Flag": "Trigger", "Keynote": "", "DisplayName": "进站信号", "Identity":"Master", "Description": "",
            "NormalTags": [
               { "Name": "PLC_Entry_SN", "Address": "s=1;x=3;112", "Length": 20, "DataType": "String", "ScanRate": 0, "Flag": "Normal", "Keynote": "", "DisplayName": "SN", "Identity":"Master", "Description": "SN" },
               { "Name": "PLC_Entry_Formual", "Address": "s=1;x=3;132", "Length": 0, "DataType": "Int", "ScanRate": 0, "Flag": "Normal", "Keynote": "", "DisplayName": "配方号", "Identity":"Master", "Description": "" },
               { "Name": "PLC_Entry_Rfid", "Address": "s=1;x=3;140", "Length": 10, "DataType": "String", "ScanRate": 0, "Flag": "Normal", "Keynote": "", "DisplayName": "RFID", "Identity":"Master", "Description": "" },
            ]
          },
          {
            "Name": "PLC_Archive_Sign", "Address": "s=1;x=3;30", "Length": 0, "DataType": "Int", "ScanRate": 200, "Flag": "Trigger", "Keynote": "", "Identity":"Master", "Description": "",
            "NormalTags": [
              { "Name": "PLC_Archive_SN", "Address": "s=1;x=3;352", "Length": 20, "DataType": "String", "ScanRate": 0, "Flag": "Normal", "Keynote": "", "DisplayName": "SN", "Identity":"Master", "Description": "" },
              { "Name": "PLC_Archive_PassResult", "Address": "s=1;x=3;372", "Length": 0, "DataType": "Int", "ScanRate": 0, "Flag": "Normal", "Keynote": "", "DisplayName": "结果", "Identity":"Master", "Description": "" },
              { "Name": "PLC_Archive_Formual", "Address": "s=1;x=3;374", "Length": 0, "DataType": "Int", "ScanRate": 0, "Flag": "Normal", "Keynote": "", "DisplayName": "配方号", "Identity":"Master", "Description": "" },
              { "Name": "PLC_Archive_Operator", "Address": "s=1;x=3;376", "Length": 10, "DataType": "String", "ScanRate": 0, "Flag": "Normal", "Keynote": "", "DisplayName": "操作人", "Identity":"Master", "Description": "" },
              { "Name": "PLC_Archive_Shift", "Address": "s=1;x=3;386", "Length": 10, "DataType": "String", "ScanRate": 0, "Flag": "Normal", "Keynote": "", "DisplayName": "班次", "Identity":"Master", "Description": "" },
              { "Name": "PLC_Archive_Rfid", "Address": "s=1;x=3;396", "Length": 10, "DataType": "String", "ScanRate": 0, "Flag": "Normal", "Keynote": "", "DisplayName": "RFID", "Identity":"Master", "Description": "" },
              { "Name": "PLC_Archive_CycleTime", "Address": "s=1;x=3;410", "Length": 0, "DataType": "Real", "ScanRate": 0, "Flag": "Normal", "Keynote": "", "DisplayName": "节拍", "Identity":"Master", "Description": "" },
              { "Name": "PLC_Archive_GlueWidth", "Address": "s=1;x=3;420", "Length": 0, "DataType": "Real", "ScanRate": 0, "Flag": "Normal", "Keynote": "", "DisplayName": "胶宽", "Identity":"Attach", "Description": "" },
              { "Name": "PLC_Archive_GlueHeight", "Address": "s=1;x=3;424", "Length": 0, "DataType": "Real", "ScanRate": 0, "Flag": "Normal", "Keynote": "", "DisplayName": "胶高", "Identity":"Attach", "Description": "" },
            ]
          },
          {
            "Name": "PLC_Scan_Sign", "Address": "s=1;x=3;440", "Length": 0, "DataType": "Int", "ScanRate": 500, "Flag": "Trigger", "Keynote": "", "DisplayName": "扫关键物料信号", "Identity":"Master", "Description": "",
            "NormalTags": [
              { "Name": "PLC_Scan_Barcode", "Address": "s=1;x=3;442", "Length": 20, "DataType": "String", "ScanRate": 0, "Flag": "Normal", "Keynote": "", "DisplayName": "Barcode", "Identity":"Master", "Description": "" },
            ]
          },
          { "Name": "MES_Callback_ReworkProc", "Address": "s=1;x=3;460", "Length": 10, "DataType": "String", "ScanRate": 0, "Flag": "Normal", "Keynote": "", "DisplayName": "", "Identity":"Master", "Description": "" },
          {
            "Name": "Switch1", "Address": "s=1;x=3;40", "Length": 0, "DataType": "Int", "ScanRate": 100, "Flag": "Switch", "DisplayName": "拧紧", "Keynote": "", "Identity":"Master", "Description": "",
            "NormalTags": [
              { "Name": "Switch1_SN", "Address": "s=1;x=3;41", "Length": 0, "DataType": "Int", "ScanRate": 100, "Flag": "Normal", "CurveUsage": "SwitchSN", "DisplayName": "", "Identity":"Master", "Description": "" },
              { "Name": "Switch1_No", "Address": "s=1;x=3;42", "Length": 0, "DataType": "Int", "ScanRate": 100, "Flag": "Normal", "CurveUsage": "SwitchNo", "DisplayName": "","Identity":"Master", "Description": "" },
              { "Name": "Switch1_Data1", "Address": "s=1;x=3;43", "Length": 0, "DataType": "Int", "ScanRate": 100, "Flag": "Normal", "CurveUsage": "SwitchCurve", "DisplayName": "x", "Identity":"Master", "Description": "" },
              { "Name": "Switch1_Data2", "Address": "s=1;x=3;44", "Length": 0, "DataType": "Int", "ScanRate": 100, "Flag": "Normal", "CurveUsage": "SwitchCurve", "DisplayName": "y", "Identity":"Master", "Description": "" }
            ]
          },
        ]
      ],
      "Tags": [
          { "Name": "Heartbeat", "Address": "s=1;x=3;0", "Length": 0, "DataType": "Int", "ScanRate": 500, "Flag": "Heartbeat", "Keynote": "", "DisplayName": "心跳", "Identity":"Master", "Description": "" },
          { "Name": "PLC_Alarm", "Address": "s=1;x=3;1", "Length": 10, "DataType": "Int", "ScanRate": 5000, "Flag": "Notice", "PublishMode": "OnlyDataChanged", "Keynote": "", "DisplayName": "警报", "Identity":"Master", "Description": "" },
          { "Name": "PLC_Equipment_State", "Address": "s=1;x=3;60", "Length": 0, "DataType": "Int", "ScanRate": 500, "Flag": "Notice", "Keynote": "", "DisplayName": "设备状态", "Identity":"Master", "Description": "" },
          { "Name": "PLC_Equipment_Mode", "Address": "s=1;x=3;62", "Length": 0, "DataType": "Int", "ScanRate": 500, "Flag": "Notice", "Keynote": "", "DisplayName": "设备运行模式", "Identity":"Master", "Description": "" },
      ]
    ]
  }
]
```


# 配置
```JSON
{
  "Ops": {
    "DefaultScanRate": 100, // 默认的标记扫描速率, 默认为 100ms
    "SwitchScanRate": 100, // 开关启动后曲线数据扫描速率，默认为 100ms
    "AllowReadMulti": true, // 是否尝试批量读取，默认为 true
    "AckWhenCallbackError": false, // 在触发标志位值回写失败时，是否触发回执值，默认为 false
    "AckMaxVersion": 3, // 在触发标志位值回写失败时，允许触发回执的最大次数，默认为 3
    "Curve": {
      "LocalRootDirectory": null, // 曲线文件本地存储根目录。可以是完整路径，也可以是相对路径，默认为根目录下的 "curves" 文件夹
      "FileType": "JSON", // 曲线存储文件格式，JSON / CSV，默认为 CSV 格式
      "CurveNamedSeparator": "_", // 曲线存储文件名称的分隔符，默认 "_"
      "DirIncludeChannelName": true, // 目录是否包含通道名称，默认为 true
      "DirIncludeCurveName": true, // 目录是否包含曲线名称，默认为 true
      "DirIncludeDate": true, // 文件路径是否包含日期，格式为 "yyyyMMdd"，默认为 true
      "AllowCategoryBySN": true, // 是否允许根据SN来打包数据，在 SN 存在的条件下为 true 时会 SN 建立文件夹，默认为 true
      "DirIncludeGroupName": true, // 目录是否包含分组名称，默认为 true
      "SuffixIncludeDatetime": false, // 文件后缀是否包含日期，格式为 "yyyyMMddHHmmss"
      "AllowMaxWriteCount": 32767, // 文件中允许写入最大的次数，默认为 "short.MaxValue" 
      "AllowCopy": false, // 是否要推送文件到远端服务器
      "RemoteRootDirectory": null, // 曲线文件远端存储根目录（共享目录）
      "RetainedSizeLimit": 0 // 本地文件允许占用的最大空间（单位 M），超过后会删除最原始的文件，0 表示不删除，默认为 0
    }
  },
  "MqttBroker": { // 
    "ServerUri": "127.0.0.1", // MQTT 服务器地址
    "Port": null, // MQTT 服务器连接地址，为 null 表示使用默认端口
    "ClientId": "ThingsEdge", // 客户端唯一 Id
    "MaxPendingMessages": 32767, // 允许本地存储最大消息数量，默认为 "short.MaxValue"
    "TopicFormater": null, // Topic 格式器，系统内部默认会采用 {ChannelName}/{DeviceName}/{TagGroupName} 模式匹配，匹配规则不区分大小写
    "TopicFormatMatchLower": true, // Topic 格式化时匹配的数据是否要转为小写，默认为 true
    "ProtocolVersion": "V311" // MQTT 协议版本，默认为 3.1.1
  },
  "HttpDestination": { // 基于 RESTful 格式的目标服务参数选项
    "BaseAddress": "http://localhost:7214", // 请求服务基地址
	"Timeout": 10000, // 请求目标超时设置，单位毫秒，默认为 10s
	"EnableBasicAuth": false, // 目标服务是否启用 Basic Authentication 验证
	"UserName": null, // 验证使用的用户名
	"Password": null, // 验证使用的密码
	"DisableCertificateValidationCheck": true, // 是否禁用凭证检测（https），默认为 true
	"HealthRequestUrl": null, // 目标服务健康检测地址，默认为 "/api/health"
	"RequestUrl": null // 目标服务接收数据的地址，默认为 "/api/iotgateway"
  }
}
```
