
SET NAMES utf8mb4;
SET FOREIGN_KEY_CHECKS = 0;

-- ----------------------------
-- Table structure for alarm_setting
-- ----------------------------
DROP TABLE IF EXISTS `alarm_setting`;
CREATE TABLE `alarm_setting`  (
  `Id` bigint(20) NOT NULL COMMENT 'Id',
  `No` int NOT NULL COMMENT '警报编号',
  `Message` varchar(255) NOT NULL COMMENT '警报消息',
  `IsDelete` bit(1) NOT NULL,
  `CreateTime` datetime(0) NOT NULL COMMENT '创建时间',
  `UpdateTime` datetime(0) NOT NULL COMMENT '更新时间',
  PRIMARY KEY (`Id`) USING BTREE
) ENGINE = InnoDB CHARACTER SET = utf8mb4 COLLATE = utf8mb4_general_ci COMMENT = '警报设定表' ROW_FORMAT = Dynamic;

-- ----------------------------
-- Table structure for alarm_record
-- ----------------------------
DROP TABLE IF EXISTS `alarm_record`;
CREATE TABLE `alarm_record`  (
  `Id` bigint(20) NOT NULL COMMENT 'Id',
  `Line` varchar(64) NOT NULL COMMENT '产线',
  `No` int NOT NULL COMMENT '警报定义的编号',
  `Message` varchar(255) NOT NULL COMMENT '警报消息',
  `StartTime` datetime(0) NOT NULL COMMENT '警报开始时间',
  `EndTime` datetime(0) NULL DEFAULT NULL COMMENT '警报结束时间',
  `IsClosed` bit(1) NOT NULL COMMENT '警报是否已关闭',
  `Duration` int NOT NULL COMMENT '警报持续时间，单位秒',
  PRIMARY KEY (`Id`) USING BTREE
) ENGINE = InnoDB CHARACTER SET = utf8mb4 COLLATE = utf8mb4_general_ci COMMENT = '警报记录表' ROW_FORMAT = Dynamic;

-- ----------------------------
-- Table structure for equipment_mode_record
-- ----------------------------
DROP TABLE IF EXISTS `equipment_mode_record`;
CREATE TABLE `equipment_mode_record`  (
  `Id` bigint(20) NOT NULL COMMENT 'Id',
  `Line` varchar(64) NOT NULL COMMENT '产线',
  `EquipmentCode` varchar(64) NOT NULL COMMENT '设备编号',
  `EquipmentName` varchar(64) NOT NULL COMMENT '设备名称',
  `RunningMode` int NOT NULL COMMENT '设备运行模式',
  `RecordTime` datetime(0) NOT NULL COMMENT '记录时间',
  PRIMARY KEY (`Id`) USING BTREE
) ENGINE = InnoDB CHARACTER SET = utf8mb4 COLLATE = utf8mb4_general_ci COMMENT = '设备运行模式记录表' ROW_FORMAT = Dynamic;

-- ----------------------------
-- Table structure for equipment_state_record
-- ----------------------------
DROP TABLE IF EXISTS `equipment_state_record`;
CREATE TABLE `equipment_state_record`  (
  `Id` bigint(20) NOT NULL COMMENT 'Id',
  `Line` varchar(64) NOT NULL COMMENT '产线',
  `EquipmentCode` varchar(64) NOT NULL COMMENT '设备编号',
  `EquipmentName` varchar(64) NOT NULL COMMENT '设备名称',
  `RunningState` int NOT NULL COMMENT '设备运行状态',
  `StartTime` datetime(0) NOT NULL COMMENT '开始时间',
  `EndTime` datetime(0) NULL DEFAULT NULL COMMENT '结束时间',
  `IsEnded` bit(1) NOT NULL COMMENT '该状态是否已结束',
  `Duration` int NOT NULL COMMENT '持续时长，单位秒',
  PRIMARY KEY (`Id`) USING BTREE
) ENGINE = InnoDB CHARACTER SET = utf8mb4 COLLATE = utf8mb4_general_ci COMMENT = '设备状态记录表' ROW_FORMAT = Dynamic;

-- ----------------------------
-- Table structure for sn_transit_record
-- ----------------------------
DROP TABLE IF EXISTS `sn_transit_record`;
CREATE TABLE `sn_transit_record`  (
  `Id` bigint(20) NOT NULL COMMENT 'Id',
  `SN` varchar(64) NOT NULL COMMENT 'SN',
  `Line` varchar(64) NOT NULL COMMENT '产线',
  `Station` varchar(64) NOT NULL COMMENT '工站',
  `EntryTime` datetime(0) NOT NULL COMMENT '进站时间',
  `ArchiveTime` datetime(0) NULL DEFAULT NULL COMMENT '出站时间',
  `IsArchived` bit(1) NOT NULL COMMENT '是否已存档',
  `CycleTime` int NOT NULL COMMENT 'CT 时长，单位秒',
  PRIMARY KEY (`Id`) USING BTREE,
  INDEX `index_sn_transit_record_sn`(`SN`) USING BTREE
) ENGINE = InnoDB CHARACTER SET = utf8mb4 COLLATE = utf8mb4_general_ci COMMENT = 'SN 过站记录表' ROW_FORMAT = Dynamic;

-- ----------------------------
-- Table structure for sn_transit_record_log
-- ----------------------------
DROP TABLE IF EXISTS `sn_transit_record_log`;
CREATE TABLE `sn_transit_record_log`  (
  `Id` bigint(20) NOT NULL COMMENT 'Id',
  `SN` varchar(64) NOT NULL COMMENT 'SN',
  `Line` varchar(64) NOT NULL COMMENT '产线',
  `Station` varchar(64) NOT NULL COMMENT '工站',
  `TransitType` int NOT NULL COMMENT '过站类型，1=>进站; 2=>出站',
  `RecordTime` datetime(0) NOT NULL COMMENT '记录时间',
  PRIMARY KEY (`Id`) USING BTREE,
  INDEX `index_sn_transit_record_log_sn`(`SN`) USING BTREE
) ENGINE = InnoDB CHARACTER SET = utf8mb4 COLLATE = utf8mb4_general_ci COMMENT = 'SN 过站记录日志表' ROW_FORMAT = Dynamic;

SET FOREIGN_KEY_CHECKS = 1;
