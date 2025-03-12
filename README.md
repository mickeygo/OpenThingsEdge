# ThingsEdge
IoT 边缘路由网关。

# JSON 地址配置

JSON 配置整体是一个数组，可包含多个 Channel。

## Channel 类型

 属性名 		| 说明
:---------------|:--------
ChannelId		|全局唯一值（自动生成）。
Name			|通道名称。
Keynote			|通道要旨，可用于设置重要信息，默认为空。
Devices			|数组，设备集合。

## Device 类型

 属性名 		| 说明
:---------------|:--------
DeviceId		|全局唯一值（自动生成）。
Name			|设备名称。
Model			|设备驱动型号，如 S7-200、S7-1200、S7-1500 等。
Host			|服务器地址。
Port			|端口，为 0 时会使用相应协议默认的端口，默认值为 0。
MaxPDUSize      |允许一次性读取的最大 PDU 长度（byte数量），0 表示不指定。
PoolSize        |设备连接使用的线程池，优先级高于全局配置，为 0 时会使用全局配置（全局配置为 0 时会使用实际长度）。
Keynote			|设备要旨，可用于设置重要信息，默认为空。
TagGroups		|数组，标记组集合。
Tags			|数组，隶属于设备的标记集合。
CallbackTags    |回写标记集合。


## TagGroup 属性

 属性名 		| 说明
:---------------|:--------
TagGroupId		|全局唯一值（自动生成）。
Name			|标记组名。
Keynote			|标记要旨，可用于设置重要信息，默认为空。
Tags			|隶属于分组的标记集合。
CallbackTags    |回写标记集合（回写标记在 Group 中不存在时会向上从隶属的 Device 中查找）。

## Tag 类型

 属性名 		| 说明
:---------------|:--------
TagId			|全局唯一值（自动生成）。
Name			|标记名称。
Address			|地址 (字符串格式)。
Length			|数据长度。注：普通类型默认长度设置为 0，大于 1 时表示为数组，为字符串时也需要指定长度。
DataType		|数据类型，分 "Bit"、"Byte"、"Word"、"DWord"、"Int"、"DInt"、"Real"、"LReal"、"String"、"S7String"、"S7WString"。
ScanRate		|扫描频率（毫秒），默认100ms。
Flag			|标记标识，分 "Heartbeat"、"Notice"、"Trigger"、"Switch"，其中 "Normal" 做为 "Notice"、"Trigger" 和 "Switch" 子标记。
PublishMode		|见 "PublishMode" 属性，默认为 "OnlyDataChanged"。注：仅适用 TagFlag.Notice 标记。
NormalTags      |数组，TagFlag.Notice、TagFlag.Trigger 和 TagFlag.Switch 类型的标记集合，在该标记触发时集合中的标记数据也同时一起随着推送。
ExtraData       |额外数据，通过 [JsonExtensionDataAttribute] 注解，包含 JSON 配置的额外属性。

### PublishMode 枚举

 属性名 		| 说明
:---------------|:--------
OnlyDataChanged |扫描后仅当数据有变动时才会推送数据。
EveryScan       |每次扫码都会推送数据。

### Tag 方法

 方法名 		| 说明 							
:-------------------------------|:--------					
IsArray()		                |判断标记是否为数组对象。
GetExtraValue(string)           |从 ExtraData 中提取 JSON 扩展数据的值，返回 string，不存在时返回 null。
GetExtraValue<T>(string)        |从 ExtraData 中提取 JSON 扩展数据的值，返回指定类型，不存在时返回 default。

## DataType 枚举
 属性名 		| 说明 							
:---------------|:--------					
Bit			|对应 bool。
Byte		|字节（8 位），对应 byte。
Word		|字（无符号 16 位），对应 ushort。
Int			|短整型（带符号 16 位），对应 short。
DWord		|双字（无符号 32 位），对应 uint。
DInt		|双整型（带符号 32 位），对应 int。
Real		|单精度浮点型（32 位），对应 float。
LReal		|双精度浮点型（64 位），对应 double。
String		|字符串（ASCII 编码），对应 string。
S7String	|西门子 S7String（ASCII 编码），对应 string。
S7WString	|西门子 S7WString（Unicode 编码），对应 string。

## DriverModel 枚举
 属性名 		| 说明 							
:---------------|:--------					
ModbusTcp			        |设备驱动型号，如西门子1200、西门子1500、三菱、欧姆龙FinsTcp、AB CIP等，地址示例：s=1;x=3;1。
S7_1500		                |西门子 S7 协议，支持 1500 系列，地址示例：DB100.12。
S7_1200		                |西门子 S7 协议，支持 1200 系列，地址示例：DB100.12。
S7_400			            |西门子 S7 协议，支持 400 系列，地址示例：DB100.12。
S7_300		                |西门子 S7 协议，支持 300 系列，地址示例：DB100.12。
S7_S200		                |西门子 S7 协议，支持 S200 系列，地址示例：DB100.12。
S7_S200Smart		        |西门子 S7 协议，支持 S200Smart 系列，地址示例：DB100.12。
Melsec_MC		            |三菱 MC 协议，支持 Q、Qna 系列，地址示例：D100。
Melsec_MCAscii		        |三菱 MC 协议，支持 Q、Qna 系列。
Melsec_MCR	                |三菱 MC 协议，支持 R 系列。
Melsec_A1E	                |三菱协议，采用A兼容1E帧协议实现，使用二进制码通讯。
Melsec_CIP                  |三菱 PLC EIP 协议。
Omron_FinsTcp               |欧姆龙 Fins-Tcp 通信协议，地址示例：D5225、D82.01。
Omron_CIP                   |欧姆龙 CIP 协议，支持 NJ、NX、NY 系列。
Omron_HostLinkOverTcp       |欧姆龙的 HostLink 协议，基于Tcp实现。
Omron_HostLinkCModeOverTcp  |欧姆龙 HostLink 协议的 C-Mode 实现。
AllenBradley_CIP            |罗克韦尔 CIP 协议，支持  1756，1769 等型号。
Inovance_Tcp                |汇川的网口通信协议，支持 AM400、AM400_800、AC800、H3U、XP、H5U 等系列。
Delta_Tcp                   |台达PLC的网口通讯类，支持 DVP-ES/EX/EC/SS型号，DVP-SA/SC/SX/EH 型号以及 AS300 型号。
Fuji_SPH                    |富士PLC的 SPH 通信协议。
Panasonic_Mc                |松下PLC，基于MC协议的实现。
XinJE_Tcp                   |信捷PLC，支持 XC、XD、XL 系列。


## JSON 选项配置示例

地址表配置示例：
```JSON
[
  {
    "Name": "Line01", "Keynote": "",
    "Devices": [
      {
        "Name": "设备01", "Model": "ModbusTcp", "Host": "127.0.0.1", "Port": 0, "Keynote": "",
        "Tags": [
          { "Name": "PLC_Device_Heartbeat", "Address": "s=1;x=3;0", "Length": 0, "DataType": "Int", "ScanRate": 500, "Flag": "Heartbeat" },
          { "Name": "PLC_Equipment_Alarm", "Address": "s=1;x=3;2", "Length": 96, "DataType": "Int", "ScanRate": 5000, "Flag": "Notice" },
          { "Name": "PLC_Equipment_Energy", "Address": "s=1;x=3;3", "Length": 0, "DataType": "Int", "ScanRate": 60000, "Flag": "Notice", "PublishMode": "EveryScan" },
          { "Name": "PLC_Equipment_State", "Address": "s=1;x=3;4", "Length": 0, "DataType": "Int", "ScanRate": 1000, "Flag": "Notice" },
        ],
        "CallbackTags": [
          { "Name": "MES_Callback_ErrorMessage", "Address": "s=1;x=3;5", "Length": 30, "DataType": "String" },
        ],
        "TagGroups": [
          {
            "Name": "OP101", "Keynote": "",
            "Tags": [
              {
                "Name": "PLC_Inbound_Sign", "Address": "s=1;x=3;20", "Length": 0, "DataType": "Int", "ScanRate": 500, "Flag": "Trigger",
                "NormalTags": [
                  { "Name": "PLC_Inbound_SN", "Address": "s=1;x=3;21", "Length": 50, "DataType": "String" },
                  { "Name": "PLC_Inbound_ProductCode", "Address": "s=1;x=3;22", "Length": 20, "DataType": "String" },
                ],
              },
              {
                "Name": "PLC_Outbound_Sign", "Address": "s=1;x=3;30", "Length": 0, "DataType": "Int", "ScanRate": 200, "Flag": "Trigger",
                "NormalTags": [
                  { "Name": "PLC_Outbound_SN", "Address": "s=1;x=3;31", "Length": 50, "DataType": "String" },
                  { "Name": "PLC_Outbound_PassResult", "Address": "s=1;x=3;32", "Length": 0, "DataType": "Int" },
                  { "Name": "PLC_Outbound_CycleTime", "Address": "s=1;x=3;33", "Length": 0, "DataType": "Real" },           
                ],
              },
              {
                "Name": "PLC_ScanKey_Sign", "Address": "s=1;x=3;40", "Length": 0, "DataType": "Int", "ScanRate": 500, "Flag": "Trigger",
                "NormalTags": [
                  { "Name": "PLC_ScanKey_SN", "Address": "s=1;x=3;41", "Length": 50, "DataType": "String" },
                  { "Name": "PLC_ScanKey_Barcode", "Address": "s=1;x=3;42", "Length": 50, "DataType": "String" },
                ],
              },
              {
                "Name": "PLC_ScanBatch_Sign", "Address": "s=1;x=3;50", "Length": 0, "DataType": "Int", "ScanRate": 500, "Flag": "Trigger",
                "NormalTags": [
                  { "Name": "PLC_ScanBatch_Barcode", "Address": "s=1;x=3;51", "Length": 50, "DataType": "String" },
                ],
              },
              {
                "Name": "PLC_StepTask_Sign", "Address": "s=1;x=3;60", "Length": 0, "DataType": "Int", "ScanRate": 500, "Flag": "Trigger",
                "NormalTags": [
                  { "Name": "PLC_StepTask_SN", "Address": "s=1;x=3;62", "Length": 50, "DataType": "String" },
                  { "Name": "PLC_StepTask_Angle", "Address": "s=1;x=3;62", "Length": 0, "DataType": "Real" },
                  { "Name": "PLC_StepTask_Torque", "Address": "s=1;x=3;63", "Length": 0, "DataType": "Real" },
                  { "Name": "PLC_StepTask_PassResult", "Address": "s=1;x=3;64", "Length": 0, "DataType": "Int" },
                ],
              },
              {
                "Name": "PLC_Switch1", "Address": "s=1;x=3;70", "Length": 0, "DataType": "Int", "ScanRate": 100, "Flag": "Switch", "DisplayName": "ArcWelding",
                "NormalTags": [
                  { "Name": "PLC_Switch1_SN", "Address": "s=1;x=3;71", "Length": 50, "DataType": "String", "CurveUsage": "Master" },
                  { "Name": "PLC_Switch1_No", "Address": "s=1;x=3;72", "Length": 10, "DataType": "String", "CurveUsage": "Master" },
                  { "Name": "PLC_Switch1_Current", "Address": "s=1;x=3;73", "Length": 0, "DataType": "Int", "CurveUsage": "Data", "DisplayName": "Current" },
                  { "Name": "PLC_Switch1_Voltage", "Address": "s=1;x=3;74", "Length": 0, "DataType": "Int", "CurveUsage": "Data", "DisplayName": "Voltage" },
                ],
              },
              {
                "Name": "PLC_Switch2", "Address": "s=1;x=3;80", "Length": 0, "DataType": "Int", "ScanRate": 100, "Flag": "Switch", "DisplayName": "ArcWelding",
                "NormalTags": [
                  { "Name": "PLC_Switch2_SN", "Address": "s=1;x=3;81", "Length": 50, "DataType": "String", "CurveUsage": "Master" },
                  { "Name": "PLC_Switch2_No", "Address": "s=1;x=3;82", "Length": 10, "DataType": "String", "CurveUsage": "Master" },
                  { "Name": "PLC_Switch2_Current", "Address": "s=1;x=3;83", "Length": 0, "DataType": "Int", "CurveUsage": "Data", "DisplayName": "Current" },
                  { "Name": "PLC_Switch2_Voltage", "Address": "s=1;x=3;84", "Length": 0, "DataType": "Int", "CurveUsage": "Data", "DisplayName": "Voltage" },
                ],
              },
            ],
            "CallbackTags": [
              { "Name": "PLC_Inbound_Sign_Response", "Address": "s=1;x=3;101", "Length": 0, "DataType": "Int" },
		      { "Name": "PLC_Outbound_Sign_Response", "Address": "s=1;x=3;102", "Length": 0, "DataType": "Int" },
              { "Name": "PLC_ScanKey_Sign_Response", "Address": "s=1;x=3;103", "Length": 0, "DataType": "Int" },
              { "Name": "PLC_ScanBatch_Sign_Response", "Address": "s=1;x=3;104", "Length": 0, "DataType": "Int" },
              { "Name": "PLC_StepTask_Sign_Response", "Address": "s=1;x=3;105", "Length": 0, "DataType": "Int" },
              { "Name": "MES_Callback_ReworkOperation", "Address": "s=1;x=3;110", "Length": 10, "DataType": "String" },
              { "Name": "MES_Callback_Error", "Address": "s=1;x=3;120", "Length": 10, "DataType": "String" },
            ],
          },
        ],
      },
    ],
  },
]
```

注：上述仅示例，地址不准确。

# 全局参数配置

## appsettings.json 配置参考
```JSON
{
  "Exchange": {
    "DefaultScanRate": 500, // 默认的标记扫描频率, 默认为 500ms。
    "AllowReadMulti": true, // 是否尝试批量读取，默认为 true。
    "HeartbeatShouldAckZero": true, // Heartbeat 心跳收到值后，是否要重置值并回写给设备，默认为 true。
    "HeartbeatListenUseHighLevel": true, // 监听心跳数据是否采用高电平值，默认为 true。
    "TriggerConditionValue": 1, // 触发标记的触发条件值，值大于 0 才有效，默认为 1。
    "TriggerTagWriteCallbackState": true, // 返回状态值回写到触发标记地址中，默认为 true。
    "TriggerStateWriteTagUseOther": false, // 使用其他标记地址来回写返回状态值，默认为 true（使用另外的标记来存储回写值时，该标记只能位于数据回写集合 CallbackTags 中）。
    "TriggerStateWriteOtherTagSuffix": "Response", // 回写状态标记的后缀名，在参数 "TriggerStateWriteTagUseOther" 为 true 时有效果，默认为 "Response"。
    "TriggerAckCodeWhenEqual": -1, // 在返回值与触发值相等时，写回给设备的状态码，默认为 -1（返回状态值在使用同一地址时有效）。
    "MaxPDUSize": 0, // 针对于 S7 等协议，PLC 一起读取运行的最多 PDU 长度（byte数量），为 0 时会使用默认长度。
    "SocketPoolSize": 1, // Socket 连接池最大数量，默认为 1。
    "NetworkConnectTimeout": 3000, // 网络连接超时时长（单位：ms），默认 3s。
    "NetworkKeepAliveTime": 60000, // Socket 保活时长（单位：ms），只有在大于 0 时才启用，默认 60s。
    "NoticePublishIncludeLast": true, // 通知消息发送时是否要带上一次信号点的值，默认为 true。
    "SwitchScanRate": 31, // 开关启动后数据扫码频率（单位：ms），默认为 31ms。
    "Curve": { // 曲线配置
      "LocalRootDirectory": null, // 曲线文件本地存储根目录。可以是完整路径，也可以是相对路径，默认为程序根目录下的 "curves" 文件夹。
      "FileType": "CSV", // 曲线存储文件格式，JSON / CSV，默认为 CSV 格式。
      "CurveNamedSeparator": "_", // 曲线存储文件名称的分隔符，默认 "_"。
      "DirIncludeChannelName": true, // 目录是否包含通道名称，默认为 true。
      "DirIncludeCurveName": true, // 目录是否包含曲线名称，默认为 true。
      "DirIncludeDate": true, // 文件路径是否包含日期，格式为 "yyyyMMdd"，默认为 true。
      "DirIncludeFirstMaster": true, // 文件路径中是否包含第一个主数据，若有需要可按配置顺序将关键信息放置第一位，默认为 true。
      "DirIncludeGroupName": true, // 目录是否包含分组名称，默认为 true。
      "SuffixIncludeDatetime": true, // 文件后缀是否包含日期，格式为 "yyyyMMddHHmmss"，默认为 true。
      "AllowMaxWriteCount": 4096, // 文件中允许写入最大的次数，默认为 4096。
      "RemoveTailCountBeforeSaving": 0, // 曲线数据保存时要移除的尾部条数，0 表示不移除，默认为 0。
      "ReturnRelativeFilePath": true, // 是否返回曲线保存文件的相对路径，false 表示返回的文件绝对路径，默认为 true。
      "AllowCopy": false, // 是否要推送文件到远端服务器。
      "RemoteRootDirectory": null, // 曲线文件远端存储根目录（共享目录）。
      "RetainedDayLimit": 0 // 本地文件保存最大天数，会删除最近访问时间超过指定天数的文件和文件夹，0 表示不删除，默认 0。
    },
  }
}
```

## 程序启动项中设置
```Shell
var host = Host.CreateDefaultBuilder();
host.AddThingsEdgeExchange(static builder =>
{
    builder.UseDeviceFileProvider()
        .UseDeviceHeartbeatForwarder<MyHeartbeatForwarder>()
        .UseNativeNoticeForwarder<MyNoticeForwarder>()
        .UseNativeTriggerForwarder<MyTriggerForwader>()
        .UseNativeSwitchForwarder<MySwitchForwader>()
        .UseOptions(options =>
        {
            options.SocketPoolSize = 5;
        });
});
```
注：
* UseDeviceFileProvider() 使用设备基于本地JSON文件的提供者，默认目录为 "[执行目录]/config/"，可以使用单一的配置文件 tags.conf（固定命名），也可以采用文件夹多层级配置，单一文件的优先级大于目录层级。
* UseDeviceCustomProvider<T>() 自定义实现地址数据配置，需要实现 IAddressProvider 接口。
* UseDeviceHeartbeatForwarder 使用设备心跳信息处理服务，需要实现 IHeartbeatForwarder 接口。
* UseNativeNoticeForwarder 使用本地通知消息处理服务，需要实现 INoticeForwarder 接口。
* UseNativeTriggerForwarder 使用本地的请求处理服务，需要实现 ITriggerForwarder 接口。
* UseNativeSwitchForwarder 使用开关消息处理服务，需要实现 ISwitchForwarder 接口。
* UseOptions() 选项中的设置级别默认高于配置文件节点 "Exchange" 设置的值。


# 接受数据接口

 事件名 				| 说明
:---------------------------|:--------
IHeartbeatForwarder	    |设备心跳数据推送接口。
IHeartbeatForwarder	    |通知数据传送接口。
ITriggerForwarder       |设备心跳数据推送接口。
ISwitchForwarder        |开关数据传送接口。

## PayloadData 对象

 属性名 		| 说明
:---------------|:--------
TagId			|同 Tag.TagId。
TagName			|同 Tag.TagName。
Address			|同 Tag.Address。
Length			|同 Tag.Length。
Value           |读取的地址的存储数据。
DataType		|同 Tag.DataType。
ExtraData       |同 Tag.ExtraData。


### PayloadData 方法

 方法名 		| 说明 							
:-------------------------------|:--------					
IsArray()		                |同 Tag.IsArray() 方法，判断标记是否为数组对象。
GetExtraValue(string)           |同 Tag.GetExtraValue(string) 方法。
GetExtraValue<T>(string)        |同 Tag.GetExtraValue<T>(string) 方法。
GetExtraValue<T>(string, T)     |同 GetExtraValue<T>(string) 方法，没找到值时可设定默认值。
GetString(bool)                 |提取对象文本值，若值不为 string 类型时，会转换为字符串。
GetStringArray(bool)            |提取对象值，并将值转换为字符串数组，若是单一值，会组合成只有一个元素的数组。
GetRawString()                  |获取原始的字符串数据。
GetBit()                        |获取 TagDataType.Bit 类型的值。
GetByte()                       |获取 TagDataType.Byte 类型的值。
GetWord()                       |获取 TagDataType.Word 类型的值。
GetDWord()                      |获取 TagDataType.DWord 类型的值。
GetInt()                        |获取 TagDataType.Int 类型的值。
GetDInt()                       |获取 TagDataType.DInt 类型的值。
GetReal()                       |获取 TagDataType.Real 类型的值。
GetLReal()                      |获取 TagDataType.LReal 类型的值。
GetBitArray()                   |获取 TagDataType.Bit 数组类型的值。
GetByteArray()                  |获取 TagDataType.Byte 数组类型的值。
GetWordArray()                  |获取 TagDataType.Word 数组类型的值。
GetDWordArray()                 |获取 TagDataType.DWord 数组类型的值。
GetIntArray()                   |获取 TagDataType.Int 数组类型的值。
GetDIntArray()                  |获取 TagDataType.DInt 数组类型的值。
GetRealArray()                  |获取 TagDataType.Real 数组类型的值。
GetLRealArray()                 |获取 TagDataType.LReal 数组类型的值。
TryGetAsBoolean(bool?)          |尝试获取值并将值转换为 bool 类型，仅将原始类型 Bit 转换为 Boolean 类型。
TryGetAsInt32(int?)             |尝试获取值并将值转换为 int 类型，会将原始类型 Bit、Byte、Word、Int 和 DInt 转换为 Int32 类型。
TryGetAsDouble(double?)         |尝试获取值并将值转换为 double 类型，会将原始类型 Bit、Byte、Word、DWord、Int、DInt、Real 和 LReal 转换为 double 类型。
TryGetAsBooleanArray(bool[]?)   |尝试获取值并将值转换为 bool 数组类型。
TryGetAsInt32Array(int[]?)      |尝试获取值并将值转换为 int 数组类型。
TryGetAsDoubleArray(double[]?)  |尝试获取值并将值转换为 double 数组类型。


## Switch 开关数据

Switch 在 Tag 配置中额外添加一些配置。
* DisplayName 在信号标记上设置，表示是该曲线的名称，配置项 "DirIncludeCurveName" 会使用此数据。
* CurveUsage "Master" 表示是主数据，如零件码、编号、序号等，可设置多个，程序会按设置的顺序进行组合；"Data" 表示是曲线数据，该标记上的 "DisplayName" 会保存为 CSV 的 Header 或是 JSON 节点名称。

示例：
```JSON
{
    "Name": "PLC_Switch1", "Address": "s=1;x=3;40", "Length": 0, "DataType": "Int", "ScanRate": 100, "Flag": "Switch", "DisplayName": "ArcWelding",
    "NormalTags": [
        { "Name": "PLC_Switch1_SN", "Address": "s=1;x=3;41", "Length": 50, "DataType": "String", "CurveUsage": "Master" },
        { "Name": "PLC_Switch1_No", "Address": "s=1;x=3;42", "Length": 0, "DataType": "Int", "CurveUsage": "Master" },
        { "Name": "PLC_Switch1_Current", "Address": "s=1;x=3;43", "Length": 0, "DataType": "Int", "CurveUsage": "Data", "DisplayName": "Current" },
        { "Name": "PLC_Switch1_Voltage", "Address": "s=1;x=3;44", "Length": 0, "DataType": "Int", "CurveUsage": "Data", "DisplayName": "Voltage" },
    ],
},
```
注：
* 如果按默认的配置，返回的文件为：root/Line1/Welding/20241225/SN001/OP010/SN001_2_20241225222424.csv，其中 root 为设置的根目录。
* 曲线数据的值必须可转换为 double 类型，若不能转换将会使用 0 代替。


# API

 属性名 			| 说明
:---------------------------|:--------
IExchange			        |交换机引擎开启、关闭接口，程序启动后需要调用 StartAsync() 方法开始作业。
ITagReaderWriter	        |设备Tag数据读写接口。
