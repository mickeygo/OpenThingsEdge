namespace ThingsEdge.Communication.Languages;

/// <summary>
/// English Version Text
/// </summary>
public class English : DefaultLanguage
{
    public override string ConnectTimeout => "Connected {0} Timeout, timeout is {1}";

    public override string UnknownError => "Unknown Error";

    public override string ErrorCode => "Error Code: ";

    public override string TextDescription => "Description: ";

    public override string SuccessText => "Success";

    public override string TwoParametersLengthIsNotSame => "Two Parameter Length is not same";

    public override string NotSupportedDataType => "The entered address fails to be resolved because the address format is not supported or the correct address is not entered. Please try again";

    public override string NotSupportedFunction => "The current feature logic does not support";

    public override string DataLengthIsNotEnough => "Receive length is not enough，Should:{0},Actual:{1}";

    public override string ReceiveDataTimeout => "Receive timeout: ";

    public override string ReceiveDataLengthTooShort => "Receive length is too short: ";

    public override string MessageTip => "Message prompt:";

    public override string IpAddressError => "IP address input exception, format is incorrect";

    public override string Send => "Send";

    public override string Receive => "Recv";

    public override string CheckDataTimeout => "When waiting to check the data, a timeout occurred. The timeout period is:";

    public override string WriteWarning => "Do you really confirm the write operation? " + Environment.NewLine + " Please check to make sure the current situation is safe!";

    public override string AddressFormatWrong => "An exception occurred during geocoding: ";

    public override string UserCancelOperate => "The user canceled the current operation";

    public override string TooManyLock => $"Too many read and write operations have occurred, resulting in a large accumulation of locks (1000), so this read and write fails, please try again later.";

    public override string AddressOffsetEven => "The address offset must be even";

    public override string SocketIOException => "Socket transport error: ";

    public override string SocketSendException => "Synchronous Data Send exception: ";

    public override string SocketHeadReceiveException => "Command header receive exception: ";

    public override string SocketContentReceiveException => "Content Data Receive exception: ";

    public override string SocketContentRemoteReceiveException => "Recipient content Data Receive exception: ";

    public override string SocketAcceptCallbackException => "Asynchronously accepts an incoming connection attempt: ";

    public override string SocketReAcceptCallbackException => "To re-accept incoming connection attempts asynchronously";

    public override string SocketSendAsyncException => "Asynchronous Data send Error: ";

    public override string SocketEndSendException => "Asynchronous data end callback send Error";

    public override string SocketReceiveException => "Asynchronous Data send Error: ";

    public override string SocketEndReceiveException => "Asynchronous data end receive instruction header error";

    public override string SocketRemoteCloseException => "An existing connection was forcibly closed by the remote host";

    public override string CommandHeadCodeCheckFailed => "Command header check failed";

    public override string CommandLengthCheckFailed => "Command length check failed";

    public override string NetClientAliasFailed => "Client's alias receive failed: ";

    public override string NetEngineStart => "Start engine";

    public override string NetEngineClose => "Shutting down the engine";

    public override string NetClientAccountTimeout => "Wait for account check timeout：";

    public override string NetClientBreak => "Abnormal offline";

    public override string NetClientFull => "The server hosts the upper limit and receives an exceeded request connection.";

    public override string NetClientLoginFailed => "Error in Client logon: ";

    public override string NetHeartCheckFailed => "Heartbeat Validation exception: ";

    public override string DataSourceFormatError => "Data source format is incorrect";

    public override string ServerFileCheckFailed => "Server confirmed file failed, please re-upload";

    public override string ReConnectServerSuccess => "Re-connect server succeeded";

    public override string ReConnectServerAfterTenSeconds => "Reconnect the server after 10 seconds";

    public override string ConnectServerSuccess => "Connection Server succeeded";

    public override string GetClientIpAddressFailed => "Client IP Address acquisition failed";

    public override string ConnectionIsNotAvailable => "The current connection is not available";

    public override string DataTransformError => "Data conversion failed, source data: ";

    public override string RemoteClosedConnection => "Remote shutdown of connection";

    public override string ModbusTcpFunctionCodeNotSupport => "Unsupported function code";

    public override string ModbusTcpFunctionCodeOverBound => "Data read out of bounds";

    public override string ModbusTcpFunctionCodeQuantityOver => "Read length exceeds maximum value";

    public override string ModbusTcpFunctionCodeReadWriteException => "Read and Write exceptions";

    public override string ModbusTcpReadCoilException => "Read Coil anomalies";

    public override string ModbusTcpWriteCoilException => "Write Coil exception";

    public override string ModbusTcpReadRegisterException => "Read Register exception";

    public override string ModbusTcpWriteRegisterException => "Write Register exception";

    public override string ModbusAddressMustMoreThanOne => "The address value must be greater than 1 in the case where the start address is 1";

    public override string ModbusAsciiFormatCheckFailed => "Modbus ASCII command check failed, not MODBUS-ASCII message";

    public override string ModbusCRCCheckFailed => "The CRC checksum check failed for Modbus";

    public override string ModbusLRCCheckFailed => "The LRC checksum check failed for Modbus";

    public override string ModbusMatchFailed => "Not the standard Modbus protocol";

    public override string ModbusBitIndexOverstep => "The index of the bit access is out of range, it should be between 0-15";

    public override string MelsecPleaseReferToManualDocument => "Please check Mitsubishi's communication manual for details of the alarm.";

    public override string MelsecReadBitInfo => "The read bit variable array can only be used for bit soft elements, if you read the word soft component, call the Read method";

    public override string MelsecCurrentTypeNotSupportedWordOperate => "The current type does not support word read and write";

    public override string MelsecCurrentTypeNotSupportedBitOperate => "The current type does not support bit read and write";

    public override string MelsecFxReceiveZero => "The received data length is 0";

    public override string MelsecFxAckNagative => "Invalid data from PLC feedback";

    public override string MelsecFxAckWrong => "PLC Feedback Signal Error: ";

    public override string MelsecFxCrcCheckFailed => "PLC Feedback message and check failed!";

    public override string MelsecError02 => "The specified range of the \"read/write\" (in/out) device is incorrect.";

    public override string MelsecError51 => "When using random access buffer memory for communication, the start address specified by the external device is set outside the range of 0-6143. Solution: Check and correct the specified start address.";

    public override string MelsecError52 => "1. When using random access buffer memory for communication, the start address + data word count specified by the external device (depending on the setting when reading) is outside the range of 0-6143. \r\n2. Data of the specified word count (text) cannot be sent in one frame. (The data length value and the total text of the communication are not within the allowed range.)";

    public override string MelsecError54 => "When \"ASCII Communication\" is selected in [Operation Settings]-[Communication Data Code] via GX Developer, ASCII codes from external devices that cannot be converted to binary codes are received.";

    public override string MelsecError55 => "When [Operation Settings]-[Cannot Write in Run Time] cannot be set by GX Developer (No check mark), if the PLCCPU is in the running state, the external device requests to write data.";

    public override string MelsecError56 => "The device specified from the outside is incorrect.";

    public override string MelsecError58 => "1. The command start address (start device number and start step number) specified by the external device can be set outside the specified range.\r\n2. The block number specified for the extended file register does not exist.\r\n3. File register (R) cannot be specified.\r\n4. Specify the word device for the bit device command.\r\n5. The start number of the bit device is specified by a certain value. This value is not a multiple of 16 in the word device command.";

    public override string MelsecError59 => "The register of the extension file cannot be specified.";

    public override string MelsecError430D => "The request data is abnormal. Therefore, operations cannot be performed on the CPU module in which user authentication is enabled. Set the user authentication function to disabled.";

    public override string MelsecErrorC04D => "In the information received by the Ethernet module through automatic open UDP port communication or out-of-order fixed buffer communication, the data length specified in the application domain is incorrect.";

    public override string MelsecErrorC050 => "When the operation setting of ASCII code communication is performed in the Ethernet module, ASCII code data that cannot be converted into binary code is received.";

    public override string MelsecErrorC051_54 => "The number of read/write points is outside the allowable range.";

    public override string MelsecErrorC055 => "The number of file data read/write points is outside the allowable range.";

    public override string MelsecErrorC056 => "The read/write request exceeded the maximum address.";

    public override string MelsecErrorC057 => "The length of the requested data does not match the data count of the character area (partial text).";

    public override string MelsecErrorC058 => "After the ASCII binary conversion, the length of the requested data does not match the data count of the character area (partial text).";

    public override string MelsecErrorC059 => "The designation of commands and subcommands is incorrect.";

    public override string MelsecErrorC05A_B => "The Ethernet module cannot read and write to the specified device.";

    public override string MelsecErrorC05C => "The requested content is incorrect. (Request to read/write to word device in bits.)";

    public override string MelsecErrorC05D => "Monitoring registration is not performed.";

    public override string MelsecErrorC05E => "The communication time between the Ethernet module and the PLC CPU exceeds the time of the CPU watchdog timer.";

    public override string MelsecErrorC05F => "The request cannot be executed on the target PLC.";

    public override string MelsecErrorC060 => "The requested content is incorrect. (Incorrect data is specified for the bit device, etc.)";

    public override string MelsecErrorC061 => "The length of the requested data does not match the number of data in the character area (partial text).";

    public override string MelsecErrorC062 => "When the online correction is prohibited, the remote protocol I/O station (QnA compatible 3E frame or 4E frame) write operation is performed by the MC protocol.";

    public override string MelsecErrorC070 => "Cannot specify the range of device memory for the target station";

    public override string MelsecErrorC072 => "The requested content is incorrect. (Request to write to word device in bit units.) ";

    public override string MelsecErrorC074 => "The target PLC does not execute the request. The network number and PC number need to be corrected.";

    public override string MelsecFxLinksError02 => "Sum check error, the sum check code in the received data is inconsistent with the sum check generated from the received data";

    public override string MelsecFxLinksError03 => "The communication protocol is abnormal, and the control sequence used for communication is different from the control sequence set with the parameters. Or part of it is different from the specified control order. Or the command specified in the control sequence does not exist.";

    public override string MelsecFxLinksError06 => "Error in character A, B, C area " + Environment.NewLine + " 1. The control sequence set by the parameter is different. " + Environment.NewLine + "2. A device number that does not exist in the target PLC is specified. " + Environment.NewLine + "3. The device number is not specified in the specified number of characters (5 characters, or 7 characters).";

    public override string MelsecFxLinksError07 => "The data written in the device is not hexadecimal ASCII code.";

    public override string MelsecFxLinksError0A => "There is no site for this PC number.";

    public override string MelsecFxLinksError10 => "There is no site for this PC number.";

    public override string MelsecFxLinksError18 => "Remote RUN/SOP cannot be executed. RUN or STOP is determined in the programmable controller hardware. (For example, using the RUN/STOP switch, etc.)";

    public override string SiemensDBAddressNotAllowedLargerThan255 => "DB block data cannot be greater than 255";

    public override string SiemensReadLengthMustBeEvenNumber => "The length of the data read must be an even number";

    public override string SiemensWriteError => "Writes the data exception, the code name is: ";

    public override string SiemensReadLengthCannotLargerThan19 => "The number of arrays read does not allow greater than 19";

    public override string SiemensDataLengthCheckFailed => "Block length checksum failed, please check if Put/get is turned on and DB block optimization is turned off";

    public override string SiemensFWError => "An exception occurred, the specific information to find the Fetch/write protocol document";

    public override string SiemensReadLengthOverPlcAssign => "The range of data read exceeds the setting of the PLC";

    public override string SiemensError000A => "Object does not exist:  Occurs when trying to request a Data Block that does not exist.";

    public override string SiemensError0006 => "The data type of the current operation is not supported";

    public override string OmronAddressMustBeZeroToFifteen => "The bit address entered can only be between 0-15";

    public override string OmronReceiveDataError => "Data Receive exception";

    public override string OmronStatus0 => "Communication is normal.";

    public override string OmronStatus1 => "The message header is not fins";

    public override string OmronStatus2 => "Data length too long";

    public override string OmronStatus3 => "This command does not support";

    public override string OmronStatus20 => "Exceeding connection limit";

    public override string OmronStatus21 => "The specified node is already in the connection";

    public override string OmronStatus22 => "Attempt to connect to a protected network node that is not yet configured in the PLC";

    public override string OmronStatus23 => "The current client's network node exceeds the normal range";

    public override string OmronStatus24 => "The current client's network node is already in use";

    public override string OmronStatus25 => "All network nodes are already in use";

    public override string AllenBradley04 => "The IOI could not be deciphered. Either it was not formed correctly or the match tag does not exist.";

    public override string AllenBradley05 => "The particular item referenced (usually instance) could not be found.";

    public override string AllenBradley06 => "The amount of data requested would not fit into the response buffer. Partial data transfer has occurred.";

    public override string AllenBradley0A => "An error has occurred trying to process one of the attributes.";

    public override string AllenBradley0C => "An error occurred while attempting a read and write operation, which triggers an error during program loading.";

    public override string AllenBradley13 => "Not enough command data / parameters were supplied in the command to execute the service requested.";

    public override string AllenBradley1C => "An insufficient number of attributes were provided compared to the attribute count.";

    public override string AllenBradley1E => "A service request in this service went wrong.";

    public override string AllenBradley20 => "The data type of the parameter in the command is inconsistent with the data type of the actual parameter.";

    public override string AllenBradley26 => "The IOI word length did not match the amount of IOI which was processed.";

    public override string AllenBradleySessionStatus00 => "success";

    public override string AllenBradleySessionStatus01 => "The sender issued an invalid or unsupported encapsulation command.";

    public override string AllenBradleySessionStatus02 => "Insufficient memory resources in the receiver to handle the command. This is not an application error. Instead, it only results if the encapsulation layer cannot obtain memory resources that it need.";

    public override string AllenBradleySessionStatus03 => "Poorly formed or incorrect data in the data portion of the encapsulation message.";

    public override string AllenBradleySessionStatus64 => "An originator used an invalid session handle when sending an encapsulation message.";

    public override string AllenBradleySessionStatus65 => "The target received a message of invalid length.";

    public override string AllenBradleySessionStatus69 => "Unsupported encapsulation protocol revision.";

    public override string PanasonicReceiveLengthMustLargerThan9 => "The received data length must be greater than 9";

    public override string PanasonicAddressParameterCannotBeNull => "Address parameter is not allowed to be empty";

    public override string PanasonicAddressBitStartMulti16 => "The starting address for bit writing needs to be a multiple of 16, for example: R0.0, R2.0, L3.0, Y4.0";

    public override string PanasonicBoolLengthMulti16 => "The data length written in batch bool needs to be a multiple of 16, otherwise it cannot be written";

    public override string PanasonicMewStatus20 => "Error unknown";

    public override string PanasonicMewStatus21 => "Nack error, the remote unit could not be correctly identified, or a data error occurred.";

    public override string PanasonicMewStatus22 => "WACK Error: The receive buffer for the remote unit is full.";

    public override string PanasonicMewStatus23 => "Multiple port error: The remote unit number (01 to 16) is set to repeat with the local unit.";

    public override string PanasonicMewStatus24 => "Transport format error: An attempt was made to send data that does not conform to the transport format, or a frame data overflow or a data error occurred.";

    public override string PanasonicMewStatus25 => "Hardware error: Transport system hardware stopped operation.";

    public override string PanasonicMewStatus26 => "Unit Number error: The remote unit's numbering setting exceeds the range of 01 to 63.";

    public override string PanasonicMewStatus27 => "Error not supported: Receiver data frame overflow. An attempt was made to send data of different frame lengths between different modules.";

    public override string PanasonicMewStatus28 => "No answer error: The remote unit does not exist. (timeout).";

    public override string PanasonicMewStatus29 => "Buffer Close error: An attempt was made to send or receive a buffer that is in a closed state.";

    public override string PanasonicMewStatus30 => "Timeout error: Persisted in transport forbidden State.";

    public override string PanasonicMewStatus40 => "BCC Error: A transmission error occurred in the instruction data.";

    public override string PanasonicMewStatus41 => "Malformed: The sent instruction information does not conform to the transmission format.";

    public override string PanasonicMewStatus42 => "Error not supported: An unsupported instruction was sent. An instruction was sent to a target station that was not supported.";

    public override string PanasonicMewStatus43 => "Processing Step Error: Additional instructions were sent when the transfer request information was suspended.";

    public override string PanasonicMewStatus50 => "Link Settings Error: A link number that does not actually exist is set.";

    public override string PanasonicMewStatus51 => "Simultaneous operation error: When issuing instructions to other units, the transmit buffer for the local unit is full.";

    public override string PanasonicMewStatus52 => "Transport suppression Error: Unable to transfer to other units.";

    public override string PanasonicMewStatus53 => "Busy error: Other instructions are being processed when the command is received.";

    public override string PanasonicMewStatus60 => "Parameter error: Contains code that cannot be used in the directive, or the code does not have a zone specified parameter (X, Y, D), and so on.";

    public override string PanasonicMewStatus61 => "Data error: Contact number, area number, Data code format (BCD,HEX, etc.) overflow, overflow, and area specified error.";

    public override string PanasonicMewStatus62 => "Register ERROR: Excessive logging of data in an unregistered state of operations (Monitoring records, tracking records, etc.). )。";

    public override string PanasonicMewStatus63 => "PLC mode error: When an instruction is issued, the run mode is not able to process the instruction.";

    public override string PanasonicMewStatus64 => "Bad external record error: 1. Bad hardware. There may be an abnormality in the built-in ROM (FROM)/main memory/SD memory card. \r\n2. The specified content exceeds the specified capacity during ROM transfer. \r\n3. A read/write error occurred.";

    public override string PanasonicMewStatus65 => "Protection Error: Performs a write operation to the program area or system register in the storage protection state.";

    public override string PanasonicMewStatus66 => "Address Error: Address (program address, absolute address, etc.) Data encoding form (BCD, hex, etc.), overflow, underflow, or specified range error.";

    public override string PanasonicMewStatus67 => "Missing data error: The data to be read does not exist. (reads data that is not written to the comment register.)";

    public override string PanasonicMewStatus68 => "Can not be rewritten in RUN error: You want to edit the command words (ED, SUB, RET, INT, IRET, SSTP, STPE) that cannot be rewritten in RUN. Nothing is written in the control unit.";

    public override string PanasonicMewStatus71 => "Exclusive control error: An instruction that cannot be processed at the same time as the instruction in progress was executed.";

    public override string PanasonicMewStatus78 => "No SD Card Error: No SD card installed.";

    public override string PanasonicMewStatus80 => "Warranty Data Abnormal Error: Warranty data (CRC code) is abnormal.";

    public override string PanasonicMewStatus81 => "No valid data error: No valid data exists.";

    public override string PanasonicMewStatus90 => "Error in log trace: An unprocessable command was executed during log trace.";

    public override string PanasonicMewStatus92 => "SD card not supported error: A business SD card manufactured by Panasonic is not used.";

    public override string PanasonicMc4031 => "Address out of range (starting device + number of writing points)";

    public override string PanasonicMcC051 => "Outside the specified range of equipment points";

    public override string PanasonicMcC056 => "Outside the specified range of the starting device";

    public override string PanasonicMcC059 => "Command search When there is no command consistent with the received data command in the MC protocol command table";

    public override string PanasonicMcC05B => "Outside the specified range of the equipment code";

    public override string PanasonicMcC05C => "When the slave command is a bit unit (0001) and the device code is a word device";

    public override string PanasonicMcC05F => "1. \"Network number\" check \r\n2. \"PC number\" check \r\n3. \"Request target unit IO number\" check \r\n4. The number of received write data is abnormal";

    public override string PanasonicMcC060 => "Write contact data abnormal (other than 0/1)";

    public override string PanasonicMcC061 => "1. The number of received data has not reached the minimum number of bytes received for the start character content check \r\n 2. The number of received data has not reached the minimum number of bytes received";

    public override string FatekStatus02 => "Illegal value";

    public override string FatekStatus03 => "Write disabled";

    public override string FatekStatus04 => "Invalid command code";

    public override string FatekStatus05 => "Cannot be activated (down RUN command but Ladder Checksum does not match)";

    public override string FatekStatus06 => "Cannot be activated (down RUN command but PLC ID ≠ Ladder ID)";

    public override string FatekStatus07 => "Cannot be activated (down RUN command but program syntax error)";

    public override string FatekStatus09 => "Cannot be activated (down RUN command, but the ladder program command PLC cannot be executed)";

    public override string FatekStatus10 => "Illegal address";

    public override string FujiSpbStatus01 => "Write to the ROM";

    public override string FujiSpbStatus02 => "Received undefined commands or commands that could not be processed";

    public override string FujiSpbStatus03 => "There is a contradiction in the data part (parameter exception)";

    public override string FujiSpbStatus04 => "Unable to process due to transfer interlocks from other programmers";

    public override string FujiSpbStatus05 => "The module number is incorrect";

    public override string FujiSpbStatus06 => "Search item not found";

    public override string FujiSpbStatus07 => "An address that exceeds the module range (when writing) is specified";

    public override string FujiSpbStatus09 => "Unable to execute due to faulty program (RUN)";

    public override string FujiSpbStatus0C => "Inconsistent password";

    public override string KeyenceSR2000Error00 => "Receive undefined command";

    public override string KeyenceSR2000Error01 => "The command format does not match. (The number of parameters is wrong)";

    public override string KeyenceSR2000Error02 => "Out of parameter 1 setting range";

    public override string KeyenceSR2000Error03 => "Out of parameter 2 setting range";

    public override string KeyenceSR2000Error04 => "Parameter 2 is not set in HEX (hexadecimal) code";

    public override string KeyenceSR2000Error05 => "Parameter 2 belongs to HEX (hexadecimal) code, but it exceeds the setting range";

    public override string KeyenceSR2000Error10 => "There are more than two preset data! The preset data is wrong";

    public override string KeyenceSR2000Error11 => "The area specified data is incorrect";

    public override string KeyenceSR2000Error12 => "The specified file does not exist";

    public override string KeyenceSR2000Error13 => "Exceeds the setting range of %Tmm-LON, bb command mm";

    public override string KeyenceSR2000Error14 => "Cannot confirm communication with %Tmm-KEYENCE command";

    public override string KeyenceSR2000Error20 => "This command is not allowed to be executed in the current mode (execution error)";

    public override string KeyenceSR2000Error21 => "The buffer is full and the command cannot be executed";

    public override string KeyenceSR2000Error22 => "An error occurred while loading or saving parameters";

    public override string KeyenceSR2000Error23 => "Since AutoID Netwoerk Navigator is being connected, the command sent by RS-232C cannot be received";

    public override string KeyenceSR2000Error99 => "If you think the SR-2000 series is abnormal, please contact KEYENCE";

    public override string KeyenceNanoE0 => "1. The specified device number, bank number, unit number, and address are out of range. " + Environment.NewLine + " 2. Specify the numbers of timers, counters, CTH and CTC that are not used by the program. " + Environment.NewLine + " 3. The monitor is not logged in, but the monitor needs to be read.";

    public override string KeyenceNanoE1 => "1. A command not supported by the CPU Unit was sent. " + Environment.NewLine + " 2. The method of the specified instruction is wrong. " + Environment.NewLine + " 3. Before communication was established, a command other than CR was sent.";

    public override string KeyenceNanoE2 => "1. The \"M1 (switch to RUN mode)\" command was sent when the CPU unit did not store a program. " + Environment.NewLine + " 2. When the RUN/PROG switch of the CPU unit is in the PROG state, the \"M1 (switch to RUN mode)\" command is sent.";

    public override string KeyenceNanoE4 => "Want to change the set values of timers, counters, and CTCs written in the disable program.";

    public override string KeyenceNanoE5 => "When the CPU unit error has not been eliminated, the \"M1 (switch to RUN mode)\" command was sent.";

    public override string KeyenceNanoE6 => "Read from the device selected by the \"RDC\" instruction.";

    public override string GeSRTPNotSupportBitReadWrite => "The current address data does not support read and write operations in bit units";

    public override string GeSRTPAddressCannotBeZero => "The starting address of the current address cannot be 0, it needs to start from 1";

    public override string GeSRTPNotSupportByteReadWrite => "The current address data does not support read and write operations in byte units, and can only be read and written in word units";

    public override string GeSRTPWriteLengthMustBeEven => "The length of the data written to the current address must be an even number";
}
